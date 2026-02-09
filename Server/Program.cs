using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Neljuti_se_covece.Klase;

namespace Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            Igra igra = new Igra();
            Random random = new Random();
            Izvestaj izvestaj = new Izvestaj(igra);
            Socket serverSocket = null;
            Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint recvEndPoint = new IPEndPoint(IPAddress.Any, 27015);
            udpSocket.Bind(recvEndPoint);
            udpSocket.Blocking = false;

            Console.WriteLine("Server čeka prijave (UDP polling) 10 sekundi...");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int tcpPort = 51000;

            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint serverLocalEndPoint = new IPEndPoint(IPAddress.Any, tcpPort);
            serverSocket.Bind(serverLocalEndPoint);
            serverSocket.Listen(20);

            List<Socket> clientSockets = new List<Socket>();

            byte[] recvBuf = new byte[1024];

            while (stopwatch.Elapsed.TotalSeconds < 15)
            {
                if (udpSocket.Poll(1000 * 1000, SelectMode.SelectRead))
                {
                    EndPoint senderEndPoint = new IPEndPoint(IPAddress.Any, 0);

                    try
                    {
                        int bytesReceived = udpSocket.ReceiveFrom(recvBuf, ref senderEndPoint);
                        string ime = Encoding.UTF8.GetString(recvBuf, 0, bytesReceived).Trim();
                        IPEndPoint senderEp = (IPEndPoint)senderEndPoint;

                        Korisnik k = new Korisnik(ime);

                        k.UdpEndPoint = senderEp;

                        k.TcpPort = tcpPort;
                        tcpPort += 2;

                        igra.pozicijeFigura.Add(k);

                        byte[] tcpPortBytes = Encoding.UTF8.GetBytes(k.TcpPort.ToString());
                        udpSocket.SendTo(tcpPortBytes, senderEp);

                        Console.WriteLine($"Prijavljen: {k.ime} (id={k.id}) UDP={senderEp} TCP={k.TcpPort}");
                    }
                    catch (SocketException ex)
                    {
                        if (ex.SocketErrorCode != SocketError.WouldBlock)
                            Console.WriteLine(ex.Message);
                    }
                }
                else
                {
                    Console.WriteLine("Još uvek čekam prijave...");
                }
            }

            Console.WriteLine($"\nIsteklo vreme! Ukupno igrača: {igra.pozicijeFigura.Count}");
            Console.WriteLine("Igra počinje sa trenutno prijavljenim brojem igrača.");

            Izvestaj iz = new Izvestaj(igra);
            iz.PrikaziIzvestaj();

            if (igra.pozicijeFigura.Count == 0)
            {
                Console.WriteLine("Nema igrača, zatvaram server.");
                udpSocket.Close();
                serverSocket.Close();
                return;
            }

            udpSocket.Close();

            Console.WriteLine("Accepting TCP connections from players...");
            foreach (Korisnik k in igra.pozicijeFigura)
            {
                Socket acceptedSocket = serverSocket.Accept();
                clientSockets.Add(acceptedSocket);
                IPEndPoint remoteEp = (IPEndPoint)acceptedSocket.RemoteEndPoint;
                k.TcpEndPoint = remoteEp;
                Console.WriteLine($"Accepted connection from {remoteEp} for player {k.ime}");
            }

            while (igra.zavrsetakIgre == 0)
            {
                for (int idx = 0; idx < igra.pozicijeFigura.Count; idx++)
                {
                    var client = clientSockets[idx];
                    client.Send(Encoding.UTF8.GetBytes(izvestaj.PrikaziIzvestaj()));
                }

                Console.WriteLine($"Na potezu je igrac: {igra.pozicijeFigura[igra.trenutniIgracNaPotezu - 1].ime}\n");
                Korisnik trenutniIgrac = igra.pozicijeFigura[igra.trenutniIgracNaPotezu - 1];

                for (int idx = 0; idx < igra.pozicijeFigura.Count; idx++)
                {
                    var client = clientSockets[idx];
                    client.Send(Encoding.UTF8.GetBytes($"Na potezu je igrac: {trenutniIgrac.ime}\n"));
                }

                int brojBaceneKockice = (int)Math.Ceiling((double)random.Next(1, 7));
                if (brojBaceneKockice == 6)
                {
                    igra.trenutniIgracNaPotezu -= 1;
                }
                for (int idx = 0; idx < igra.pozicijeFigura.Count; idx++)
                {
                    var k = igra.pozicijeFigura[idx];
                    if (k.TcpPort == trenutniIgrac.TcpPort)
                    {
                        clientSockets[idx].Send(Encoding.UTF8.GetBytes($"Broj kockice koju ste dobili: {brojBaceneKockice}\n"));
                    }
                    else
                    {
                        clientSockets[idx].Send(Encoding.UTF8.GetBytes($"Broj kockice koju je bacio igrac {k.id}: {brojBaceneKockice}\n"));
                    }
                }

                int brojNeaktivnih = 0;
                if (brojBaceneKockice != 6)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (trenutniIgrac.trenutnaPozicijaFigura[i].status == "neaktivna")
                        {
                            brojNeaktivnih++;
                        }
                    }
                    if (brojNeaktivnih == 4)
                    {
                        int currIdx = igra.pozicijeFigura.IndexOf(trenutniIgrac);
                        clientSockets[currIdx].Send(Encoding.UTF8.GetBytes("Morate baciti sesticu da biste aktivirali figuru! Preskacete potez.\n"));
                        igra.trenutniIgracNaPotezu = (igra.trenutniIgracNaPotezu % igra.pozicijeFigura.Count) + 1;
                        continue;
                    }
                }

                int brojFigure;
                byte[] recvBuff = new byte[1024];
                while (true)
                {
                    int currIdx = igra.pozicijeFigura.IndexOf(trenutniIgrac);
                    var currSocket = clientSockets[currIdx];

                    currSocket.Send(Encoding.UTF8.GetBytes("Unesite broj figure nad kojom zelite da izvrsite akciju\n"));

                    // Wait for client response
                    int bytesReceived = currSocket.Receive(recvBuff);

                    string brojFigureStr = Encoding.UTF8.GetString(recvBuff, 0, bytesReceived).Trim();
                    if (!int.TryParse(brojFigureStr, out brojFigure))
                    {
                        currSocket.Send(Encoding.UTF8.GetBytes("Neispravan unos! Pokusajte ponovo.\n"));
                        continue;
                    }

                    if (brojFigure < 1 || brojFigure > 4)
                    {
                        currSocket.Send(Encoding.UTF8.GetBytes("Neispravan broj figure! Pokusajte ponovo.\n"));
                    }
                    else
                    {
                        break;
                    }
                }

                int currIdxAction = igra.pozicijeFigura.IndexOf(trenutniIgrac);
                var actionSocket = clientSockets[currIdxAction];
                actionSocket.Send(Encoding.UTF8.GetBytes("Unesite akciju koju zelite da izvrsite (aktivacija, pomeri)\n"));

                // Wait for client response
                int bytesAct = actionSocket.Receive(recvBuff);
                string akcija = Encoding.UTF8.GetString(recvBuff, 0, bytesAct).Trim();

                Potez potez = new Potez
                {
                    igra = igra,
                    igrac = trenutniIgrac,
                    brojBaceneKockice = brojBaceneKockice,
                    brojFigure = brojFigure,
                    akcija = akcija
                };
                igra.trenutniIgracNaPotezu = (igra.trenutniIgracNaPotezu % igra.pozicijeFigura.Count) + 1;
                potez.IzvrsiPotez(trenutniIgrac, actionSocket, igra.pozicijeFigura.Count());
            }

            foreach (var s in clientSockets)
            {
                try { s.Shutdown(SocketShutdown.Both); } catch { }
                s.Close();
            }
            serverSocket.Close();
        }
    }
}
