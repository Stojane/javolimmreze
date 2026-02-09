using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Neljuti_se_covece.Klase
{
    public class Potez
    {
        byte[] recvBuff = new byte[1024];
        public Igra igra { get; set; } = new Igra();
        public Korisnik igrac { get; set; } = new Korisnik();
        public int brojFigure { get; set; } = 0;
        public string akcija { get; set; } = string.Empty;
        public int brojBaceneKockice { get; set; } = 0;
        void SendString(Socket s, string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            s.Send(Encoding.UTF8.GetBytes(text));
        }

        string ReceiveString(Socket s)
        {
            int b = s.Receive(recvBuff);
            if (b <= 0) return string.Empty;
            return Encoding.UTF8.GetString(recvBuff, 0, b).Trim();
        }
        public void IzvrsiPotez(Korisnik trenutniIgrac, Socket clientSocket, int brojIgraca)
        {
            int potezOdigran = 0;

            if (igra.zavrsetakIgre == 1)
                return;

            while (potezOdigran == 0)
            {
                switch (akcija)
                {
                    case "aktivacija":
                        {
                            var figura = igrac.trenutnaPozicijaFigura[brojFigure - 1];

                            if (figura.status != "neaktivna")
                            {
                                SendString(clientSocket, "Figura je vec aktivna!\n");
                                break;
                            }

                            if (brojBaceneKockice != 6)
                            {
                                SendString(clientSocket, "Nemoguce aktivirati figuru jer baceni broj nije 6!\n");
                                break;
                            }

                            figura.status = "aktivna";
                            figura.pozicija = 0;
                            potezOdigran = 1;
                            continue;
                        }

                    case "pomeri":
                        {
                            var figura = igrac.trenutnaPozicijaFigura[brojFigure - 1];

                            if (figura.status == "neaktivna")
                            {
                                SendString(clientSocket, "Figura je neaktivna! Ne mozete je pomeriti.\n");
                                break;
                            }

                            if (brojBaceneKockice > figura.udaljenostDoCilja)
                            {
                                SendString(clientSocket, "Nemoguce je pomeriti figuru za toliko polja!\n");
                                break;
                            }

                            if (brojBaceneKockice == figura.udaljenostDoCilja)
                            {
                                figura.pozicija = -1;
                                figura.status = "u cilju";
                                figura.udaljenostDoCilja = 0;
                                igrac.cilj++;

                                if (igrac.cilj == 4)
                                {
                                    SendString(clientSocket,
                                        $"Cestitamo igracu {igrac.ime}, pobedili ste u igri!\n");
                                    igra.zavrsetakIgre = 1;
                                }

                                potezOdigran = 1;
                                continue;
                            }

                            figura.pozicija += brojBaceneKockice;
                            figura.udaljenostDoCilja -= brojBaceneKockice;

                            for (int i = 0; i < brojIgraca; i++)
                            {
                                if (i == igrac.id) continue;

                                igra.pozicijeFigura[i].trenutnaPozicijaFigura.ForEach(f =>
                                {
                                    if (f.status == "aktivna" &&
                                        f.pozicija == figura.pozicija - (i - igrac.id) &&
                                        f.pozicija > 3)
                                    {
                                        SendString(clientSocket,
                                            $"Igrac {igrac.ime} je izbacio figuru igraca {igra.pozicijeFigura[i].ime}!\n");

                                        f.pozicija = -1;
                                        f.status = "neaktivna";
                                        f.udaljenostDoCilja = 44;
                                    }
                                });
                            }

                            potezOdigran = 1;
                            continue;
                        }

                    default:
                        {
                            SendString(clientSocket,
                                "Neispravna akcija! Dozvoljeno: aktivacija, pomeri\n");
                            break;
                        }
                }

                // AKO SMO OVDE – ZNAČI DA JE POTEZ BIO NEISPRAVAN
                SendString(clientSocket, "Unesite broj figure (1-4):\n");
                string s = ReceiveString(clientSocket);

                while (!int.TryParse(s, out int parsed) || parsed < 1 || parsed > 4)
                {
                    SendString(clientSocket, "Neispravan unos! Unesite broj figure (1-4):\n");
                    s = ReceiveString(clientSocket);
                }

                brojFigure = int.Parse(s);

                SendString(clientSocket, "Unesite akciju (aktivacija, pomeri):\n");
                akcija = ReceiveString(clientSocket);
            }
        }

    }
}

