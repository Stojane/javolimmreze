using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Klijent
{
    public class Program
    {
        static void Main(string[] args)
        {
            Socket udpSendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint recvEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 27015);
            Console.Write("Unesite ime: ");
            string message = Console.ReadLine() ?? string.Empty;
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            string tcpPortStr = string.Empty;
            try
            {
                udpSendSocket.SendTo(messageBytes, 0, messageBytes.Length, SocketFlags.None, recvEndPoint);
                Console.WriteLine("Ime poslato serveru.");

                byte[] recvBuf = new byte[1024];
                EndPoint fromEP = new IPEndPoint(IPAddress.Any, 0);

                int bytesReceived = udpSendSocket.ReceiveFrom(recvBuf, ref fromEP);
                tcpPortStr = Encoding.UTF8.GetString(recvBuf, 0, bytesReceived).Trim();

                Console.WriteLine("Primljen TCP port od servera: " + tcpPortStr);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("UDP error: {0}", ex.Message);
            }
            finally
            {
                udpSendSocket.Close();
            }

            if (!string.IsNullOrEmpty(tcpPortStr))
            {
                Socket clientSocket = null;
                try
                {
                    clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    int brojPorta = int.Parse(tcpPortStr);
                    IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), brojPorta);
                    clientSocket.Connect(serverEndPoint);
                    Console.WriteLine("Povezano na server: " + serverEndPoint);

                    byte[] recvBuf = new byte[4096];

                    while (true)
                    {
                        int bytesReceived;
                        try
                        {
                            bytesReceived = clientSocket.Receive(recvBuf);
                        }
                        catch (SocketException se)
                        {
                            Console.WriteLine("Socket error: " + se.Message);
                            break;
                        }

                        if (bytesReceived == 0)
                        {
                            Console.WriteLine("Veza je prekinuta od strane servera.");
                            break;
                        }

                        string serverMsg = Encoding.UTF8.GetString(recvBuf, 0, bytesReceived).Trim();
                        Console.WriteLine("Server: " + serverMsg);

                        // If server asks for a number or an action, prompt user and send response
                        if (serverMsg.IndexOf("Unesite broj figure", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            serverMsg.IndexOf("Unesite broj figure nad kojom", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            serverMsg.IndexOf("Unesite drugu akciju", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            Console.Write("Unesite broj figure (1-4): ");
                            string resp = Console.ReadLine() ?? string.Empty;
                            clientSocket.Send(Encoding.UTF8.GetBytes(resp));
                        }
                        else if (serverMsg.IndexOf("Unesite akciju", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            Console.Write("Unesite akciju (aktivacija, pomeri): ");
                            string resp = Console.ReadLine() ?? string.Empty;
                            clientSocket.Send(Encoding.UTF8.GetBytes(resp));
                        }
                        // otherwise just continue reading server messages
                    }
                }
                finally
                {
                    if (clientSocket != null)
                    {
                        try { clientSocket.Shutdown(SocketShutdown.Both); } catch { }
                        clientSocket.Close();
                    }
                }
            }

            Console.WriteLine("Kraj. Pritisnite bilo koju tipku za izlaz.");
            Console.ReadKey();
        }
    }
}
