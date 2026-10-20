// MYSTIC 20/10 : petit serveur « Zaap » local (protocole Thrift binaire, comme le Zaap d'Ankama) pour le client 2.51.
// Le client lancé avec --port --gameName --gameRelease --instanceId --hash --canLogin=true se connecte ici (127.0.0.1) ;
// sur l'écran de connexion (après une déconnexion), son bouton « Se connecter avec » demande un jeton : le launcher
// demande alors un ticket frais à l'AuthServer POUR LE COMPTE DE CETTE FENÊTRE précise, et le client se reconnecte tout seul.
// Appels gérés (client : com.ankama.zaap.ZaapServiceImpl) : connect, userInfo_get, auth_getGameToken, settings_get,
// settings_set, updater_isUpdateAvailable, release_restartOnExit.
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace MysticLauncher
{
    public static class Zaap
    {
        class Fenetre { public int Compte; public string Login; public int Instance; }

        static TcpListener s_ecoute;
        static int s_instance;
        static readonly ConcurrentDictionary<string, Fenetre> s_parHash = new ConcurrentDictionary<string, Fenetre>();
        static readonly ConcurrentDictionary<string, Fenetre> s_parSession = new ConcurrentDictionary<string, Fenetre>();

        /// <summary>Port local d'écoute (0 si le serveur n'a pas pu démarrer).</summary>
        public static int Port { get; private set; }
        /// <summary>Demande un ticket frais pour un compte de jeu : (login, ticket) ou null.</summary>
        public static Func<int, Tuple<string, string>> DemanderTicket;
        public static Action<string> Journal;

        public static void Demarrer()
        {
            if (s_ecoute != null) return;
            try
            {
                s_ecoute = new TcpListener(IPAddress.Loopback, 0);
                s_ecoute.Start();
                Port = ((IPEndPoint)s_ecoute.LocalEndpoint).Port;
                new Thread(Accepter) { IsBackground = true, Name = "Zaap" }.Start();
                Journal?.Invoke("Zaap local prêt sur le port " + Port);
            }
            catch (Exception e) { Port = 0; s_ecoute = null; Journal?.Invoke("Zaap local indisponible : " + e.Message); }
        }

        /// <summary>Prépare une fenêtre de jeu : renvoie les arguments à ajouter au lancement du client (vide si le serveur ne tourne pas).</summary>
        public static string Arguments(int compte, string login)
        {
            if (Port == 0 || string.IsNullOrEmpty(login)) return "";
            var octets = new byte[24];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(octets);
            var hash = BitConverter.ToString(octets).Replace("-", "").ToLowerInvariant();
            var f = new Fenetre { Compte = compte, Login = login, Instance = Interlocked.Increment(ref s_instance) };
            s_parHash[hash] = f;
            return " --port=" + Port + " --gameName=dofus --gameRelease=main --instanceId=" + f.Instance + " --hash=" + hash + " --canLogin=true";
        }

        static void Accepter()
        {
            while (true)
            {
                TcpClient c;
                try { c = s_ecoute.AcceptTcpClient(); } catch { return; }
                // uniquement la machine locale
                if (!IPAddress.IsLoopback(((IPEndPoint)c.Client.RemoteEndPoint).Address)) { c.Close(); continue; }
                new Thread(() => Servir(c)) { IsBackground = true }.Start();
            }
        }

        // ---------------------------------------------------------------- Thrift binaire (TBinaryProtocol, version stricte)
        const byte T_STOP = 0, T_BOOL = 2, T_I32 = 8, T_STRING = 11, T_STRUCT = 12;
        const int VERSION_1 = unchecked((int)0x80010000);

        static int LireI32(Stream s) { var b = Lire(s, 4); return (b[0] << 24) | (b[1] << 16) | (b[2] << 8) | b[3]; }
        static short LireI16(Stream s) { var b = Lire(s, 2); return (short)((b[0] << 8) | b[1]); }
        static byte[] Lire(Stream s, int n)
        {
            if (n < 0 || n > 65536) throw new IOException("taille invalide");
            var b = new byte[n]; int lu = 0;
            while (lu < n) { int r = s.Read(b, lu, n - lu); if (r <= 0) throw new EndOfStreamException(); lu += r; }
            return b;
        }
        static string LireChaine(Stream s) => Encoding.UTF8.GetString(Lire(s, LireI32(s)));

        static void Sauter(Stream s, byte type)
        {
            switch (type)
            {
                case T_BOOL: case 3: Lire(s, 1); break;           // bool, byte
                case 6: Lire(s, 2); break;                        // i16
                case T_I32: Lire(s, 4); break;
                case 4: case 10: Lire(s, 8); break;               // double, i64
                case T_STRING: Lire(s, LireI32(s)); break;
                case T_STRUCT: LireStruct(s, null); break;
                case 13: { var kt = Lire(s, 1)[0]; var vt = Lire(s, 1)[0]; int n = LireI32(s); for (int i = 0; i < n; i++) { Sauter(s, kt); Sauter(s, vt); } break; }
                case 14: case 15: { var et = Lire(s, 1)[0]; int n = LireI32(s); for (int i = 0; i < n; i++) Sauter(s, et); break; }
                default: throw new IOException("type Thrift inconnu " + type);
            }
        }

        /// <summary>Lit une structure d'arguments : champs chaîne et entier rangés par numéro.</summary>
        static void LireStruct(Stream s, Action<short, byte, object> champ)
        {
            while (true)
            {
                byte type = Lire(s, 1)[0];
                if (type == T_STOP) return;
                short id = LireI16(s);
                if (champ != null && type == T_STRING) champ(id, type, LireChaine(s));
                else if (champ != null && type == T_I32) champ(id, type, LireI32(s));
                else Sauter(s, type);
            }
        }

        class Ecrivain
        {
            readonly MemoryStream m = new MemoryStream();
            public void I32(int v) { m.WriteByte((byte)(v >> 24)); m.WriteByte((byte)(v >> 16)); m.WriteByte((byte)(v >> 8)); m.WriteByte((byte)v); }
            public void I16(short v) { m.WriteByte((byte)(v >> 8)); m.WriteByte((byte)v); }
            public void Octet(byte v) => m.WriteByte(v);
            public void Chaine(string v) { var b = Encoding.UTF8.GetBytes(v ?? ""); I32(b.Length); m.Write(b, 0, b.Length); }
            public void Champ(byte type, short id) { Octet(type); I16(id); }
            public byte[] Octets() => m.ToArray();
        }

        static void Servir(TcpClient client)
        {
            using (client)
            using (var s = client.GetStream())
            {
                try
                {
                    while (true)
                    {
                        // en-tête de message
                        int v = LireI32(s);
                        if ((v & unchecked((int)0xFFFF0000)) != VERSION_1) return;
                        string nom = LireChaine(s);
                        int seq = LireI32(s);
                        var args = new System.Collections.Generic.Dictionary<short, object>();
                        LireStruct(s, (id, t, val) => args[id] = val);
                        var w = new Ecrivain();
                        w.I32(VERSION_1 | 2); w.Chaine(nom); w.I32(seq);   // REPLY
                        Repondre(nom, args, w);
                        w.Octet(T_STOP);                                    // fin de la structure de résultat
                        var o = w.Octets();
                        s.Write(o, 0, o.Length); s.Flush();
                    }
                }
                catch (Exception) { /* fenêtre fermée ou message illisible : on coupe */ }
            }
        }

        static string Arg(System.Collections.Generic.Dictionary<short, object> a, short id) => a.TryGetValue(id, out var v) ? v as string : null;

        static void Erreur(Ecrivain w, int code, string details)
        {
            w.Champ(T_STRUCT, 1); w.Champ(T_I32, 1); w.I32(code); w.Champ(T_STRING, 2); w.Chaine(details); w.Octet(T_STOP);
        }

        static void Repondre(string nom, System.Collections.Generic.Dictionary<short, object> a, Ecrivain w)
        {
            Fenetre f;
            switch (nom)
            {
                case "connect":
                    // gameName (1), releaseName (2), instanceId (3), hash (4)
                    if (Arg(a, 4) is string h && s_parHash.TryGetValue(h, out f))
                    {
                        var session = Guid.NewGuid().ToString("N");
                        s_parSession[session] = f;
                        w.Champ(T_STRING, 0); w.Chaine(session);
                        Journal?.Invoke("Zaap : fenêtre de " + f.Login + " reliée au launcher");
                    }
                    else Erreur(w, 2, "Fenêtre inconnue : relance le jeu depuis le launcher.");
                    break;
                case "userInfo_get":
                    if (Arg(a, 1) is string su && s_parSession.TryGetValue(su, out f)) { w.Champ(T_STRING, 0); w.Chaine("{\"login\":\"" + f.Login.Replace("\\", "").Replace("\"", "") + "\"}"); }
                    else Erreur(w, 3, "Session inconnue.");
                    break;
                case "auth_getGameToken":
                    if (Arg(a, 1) is string st && s_parSession.TryGetValue(st, out f))
                    {
                        Tuple<string, string> t = null;
                        try { t = DemanderTicket?.Invoke(f.Compte); } catch { }
                        if (t != null && !string.IsNullOrEmpty(t.Item2))
                        {
                            if (!string.IsNullOrEmpty(t.Item1)) f.Login = t.Item1;
                            w.Champ(T_STRING, 0); w.Chaine(t.Item2);
                            Journal?.Invoke("Zaap : nouveau ticket pour " + f.Login + " (reconnexion)");
                        }
                        else Erreur(w, 2001, "Reconnecte-toi dans le launcher.");
                    }
                    else Erreur(w, 3, "Session inconnue.");
                    break;
                case "settings_get":
                    if (Arg(a, 2) == "language") { w.Champ(T_STRING, 0); w.Chaine("fr"); }
                    else Erreur(w, 4001, "Réglage inconnu.");
                    break;
                case "updater_isUpdateAvailable":
                    w.Champ(T_BOOL, 0); w.Octet(0);
                    break;
                default:   // settings_set, release_restartOnExit : rien à renvoyer
                    break;
            }
        }
    }
}
