using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using Stump.Core.Attributes;
using Stump.DofusProtocol.Messages;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Fights;

namespace Stump.Server.WorldServer.Game.Bugs
{
    /// <summary>
    /// PADGREF 23/10 : replay complet d'un combat (bouton « Replay » de la barre / .replay). Tous les messages envoyés aux
    /// combattants (WorldClientCollection.Observateur) sont gardés en mémoire avec l'heure, les combattants (nom, équipe, apparence)
    /// et les cases de la carte. À la demande, le replay est écrit (JSON compressé gzip) dans padgref_replays et la page
    /// replay.html du site le rejoue (api/replay.php?r=clé). Combat en cours : état au moment du clic ; sinon dernier combat.
    /// </summary>
    public static class ReplayCombat
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>Adresse du site (page replay.html), terminée par « / ». IP du VPS tant que padgref.fr ne répond pas
        /// (world_config.xml : Game > Bugs > ReplayCombat > SiteUrl pour la changer sans recompiler).</summary>
        [Variable(true)]
        public static string SiteUrl = "http://IP_DU_VPS/";

        /// <summary>Nombre maximal de messages gardés par combat (au-delà, la fin n'est plus enregistrée).</summary>
        [Variable(true)]
        public static int MaxMessages = 30000;

        /// <summary>Durée de conservation des replays écrits en base (jours).</summary>
        [Variable(true)]
        public static int ConservationJours = 14;

        /// <summary>
        /// Webhook Discord du salon qui reçoit chaque replay créé (pseudo en jeu + lien). Vide = pas d'envoi. SECRET : seulement
        /// dans world_config.xml du serveur (Game > Bugs > ReplayCombat > WebhookDiscord), jamais dans un paquet ni sur GitHub.
        /// </summary>
        [Variable(true)]
        public static string WebhookDiscord = "";

        /// <summary>Délai minimum entre deux liens de replay d'un même personnage (secondes).</summary>
        [Variable(true)]
        public static int DelaiEntreLiensSecondes = 10;

        // messages sans intérêt pour le replay (discussion : vie privée ; ping)
        static readonly HashSet<string> Ignores = new HashSet<string>
        {
            "ChatServerMessage", "ChatServerWithObjectMessage", "ChatServerCopyMessage", "ChatAdminServerMessage",
            "BasicNoOperationMessage", "BasicAckMessage", "BasicLatencyStatsRequestMessage",
            // messages personnels du témoin sans intérêt pour le replay
            "CharacterStatsListMessage", "InventoryWeightMessage", "ObjectQuantityMessage", "ObjectAddedMessage", "ObjectsAddedMessage",
            "ObjectModifiedMessage", "KamasUpdateMessage", "EmotePlayMessage", "PartyUpdateLightMessage", "FriendUpdateMessage",
            "GuildMemberOnlineStatusMessage", "PrismsListUpdateMessage", "BasicLatencyStatsMessage"
        };

        static readonly ConcurrentDictionary<IFight, Enregistrement> m_enCours = new ConcurrentDictionary<IFight, Enregistrement>();
        static readonly ConcurrentDictionary<int, byte[]> m_dernierParPerso = new ConcurrentDictionary<int, byte[]>();
        // compression du dernier combat encore en cours (clic sur « Replay » juste après la fin du combat)
        static readonly ConcurrentDictionary<int, Task> m_enPreparation = new ConcurrentDictionary<int, Task>();
        static readonly ConcurrentDictionary<int, DateTime> m_derniersLiens = new ConcurrentDictionary<int, DateTime>();
        static bool m_tablePrete;

        // TypeNameHandling.Auto : « $type » quand le type réel diffère du type déclaré (ex. GameFightCharacterInformations dans
        // GameFightShowFighterMessage) -> le replay peut être rejoué À L'IDENTIQUE dans le jeu (ReplayLecteur)
        internal static readonly JsonSerializerSettings Reglages = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
            SerializationBinder = new SeulementProtocole(),
            Error = (s, e) => e.ErrorContext.Handled = true
        };

        /// <summary>Relecture : seuls les types du protocole Dofus sont acceptés dans « $type ».</summary>
        sealed class SeulementProtocole : Newtonsoft.Json.Serialization.DefaultSerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                if (assemblyName == null || !assemblyName.StartsWith("Stump.DofusProtocol"))
                    throw new JsonSerializationException("type refusé : " + typeName);
                return base.BindToType(assemblyName, typeName);
            }
        }

        static Stump.ORM.Database Database => WorldServer.Instance.DBAccessor.Database;

        class Enregistrement
        {
            public readonly DateTime Debut = DateTime.Now;
            public readonly List<object[]> Messages = new List<object[]>();
            public readonly Dictionary<int, Dictionary<string, object>> Combattants = new Dictionary<int, Dictionary<string, object>>();
            public bool Coupe;
            public DateTime? Fin;
            public bool Termine;
            public WorldClient Temoin;
        }

        /// <summary>Appelé à la création de chaque combat (Fight), avec la liste des clients des combattants.</summary>
        public static void Suivre(IFight fight, WorldClientCollection clients)
        {
            var e = new Enregistrement();
            m_enCours[fight] = e;

            // PADGREF 23/10 : on enregistre TOUT ce que reçoit un joueur « témoin » du combat (le premier joueur, remplacé s'il
            // part) : déplacements et sorts sont envoyés joueur par joueur (invisibilité), pas à tout le combat d'un coup.
            Action choisirTemoin = null;
            choisirTemoin = () =>
            {
                lock (e)
                {
                    if (e.Termine || (e.Temoin != null && e.Temoin.Connected && fight.GetAllFighters<CharacterFighter>(y => y.Character.Client == e.Temoin && !y.HasLeft()).Any()))
                        return;
                    if (e.Temoin != null) e.Temoin.Observateur = null;
                    var c = fight.GetAllFighters<CharacterFighter>(x => !x.HasLeft() && x.Character.Client != null && x.Character.Client.Connected).FirstOrDefault();
                    e.Temoin = c?.Character.Client;
                    if (e.Temoin != null) e.Temoin.Observateur = message => Noter(e, message);
                }
            };

            foreach (var team in new[] { fight.ChallengersTeam, fight.DefendersTeam })
            {
                team.FighterAdded += (t, f) => { NoterCombattant(e, f); if (f is CharacterFighter) choisirTemoin(); };
                team.FighterRemoved += (t, f) => { if (f is CharacterFighter cf && cf.Character.Client == e.Temoin) { lock (e) { if (e.Temoin != null) e.Temoin.Observateur = null; e.Temoin = null; } choisirTemoin(); } };
            }
            fight.TurnStarted += (f, a) => choisirTemoin();

            fight.FightEnded += f =>
            {
                e.Fin = DateTime.Now;
                var persos = f.GetAllFighters().OfType<CharacterFighter>().Select(c => c.Character.Id).ToList();
                if (persos.Count == 0)
                    return;
                // le journal texte du combat vient d'être rangé par JournalCombat (abonné avant)
                var journal = JournalCombat.TexteCombat(f) ?? JournalCombat.Dernier(persos[0]);
                // sérialisation hors du fil de la carte (quelques milliers de messages), après l'écran de fin envoyé au témoin
                Task tache = null;
                tache = Task.Run(async () =>
                {
                    await Task.Delay(1500);
                    lock (e) { e.Termine = true; if (e.Temoin != null) e.Temoin.Observateur = null; e.Temoin = null; }
                    m_enCours.TryRemove(f, out _);
                    try
                    {
                        var donnees = Compresser(f, e, journal);
                        foreach (var id in persos)
                            m_dernierParPerso[id] = donnees;
                    }
                    catch (Exception ex)
                    {
                        logger.Error("[Replay] combat {0} : {1}", f.Id, ex.Message);
                    }
                    finally
                    {
                        foreach (var id in persos)
                            m_enPreparation.TryRemove(id, out _);
                    }
                });
                foreach (var id in persos)
                    m_enPreparation[id] = tache;
            };
        }

        static void Noter(Enregistrement e, Message message)
        {
            var nom = message.GetType().Name;
            if (e.Termine || Ignores.Contains(nom))
                return;
            // fin du combat vue par le témoin : plus rien après (retour sur la carte)
            if (message is GameFightEndMessage)
                e.Termine = true;
            lock (e.Messages)
            {
                if (e.Messages.Count >= MaxMessages)
                {
                    e.Coupe = true;
                    return;
                }
                e.Messages.Add(new object[] { (int)(DateTime.Now - e.Debut).TotalMilliseconds, nom, message });
            }
        }

        static void NoterCombattant(Enregistrement e, FightActor f)
        {
            try
            {
                var infos = new Dictionary<string, object>
                {
                    ["nom"] = Nom(f),
                    ["equipe"] = (int)f.Team.Id,
                    ["niveau"] = (int)f.Level,
                    ["case"] = f.Cell?.Id ?? -1,
                    ["pv"] = f.MaxLifePoints,
                    ["genre"] = f is CharacterFighter ? "perso" : f is SummonedFighter ? "invocation" : f is MonsterFighter ? "monstre" : "autre",
                };
                try { infos["look"] = f.Look?.ToString(); } catch { }
                // monstres / invocations : numéro du monstre (le site affiche son nom français, jeu/anim/monstres.json)
                if (f is MonsterFighter mf && mf.Monster?.Template != null) infos["monstre"] = mf.Monster.Template.Id;
                if (f is SummonedMonster sm && sm.Monster?.Template != null) infos["monstre"] = sm.Monster.Template.Id;
                lock (e.Combattants)
                    e.Combattants[f.Id] = infos;
            }
            catch (Exception ex)
            {
                logger.Debug("[Replay] combattant {0} : {1}", f, ex.Message);
            }
        }

        static string Nom(FightActor f)
        {
            try
            {
                return f.GetMapRunningFighterName();
            }
            catch
            {
                return f.GetType().Name;
            }
        }

        static byte[] Compresser(IFight f, Enregistrement e, string journal)
        {
            List<object[]> messages;
            lock (e.Messages)
                messages = e.Messages.ToList();
            Dictionary<int, Dictionary<string, object>> combattants;
            lock (e.Combattants)
                combattants = e.Combattants.ToDictionary(x => x.Key, x => x.Value);

            // cases : 0 = obstacle / trou, 1 = praticable, 2 = praticable sans ligne de vue (rare), 3 = obstacle qui laisse voir
            var cases = new StringBuilder();
            foreach (var c in f.Map.Cells)
            {
                var marche = c.Walkable && !c.NonWalkableDuringFight;
                cases.Append(marche ? (c.LineOfSight ? '1' : '2') : (c.LineOfSight ? '3' : '0'));
            }

            // noms des sorts lancés (la page du site n'a pas les textes du jeu)
            var sorts = new Dictionary<int, string>();
            foreach (var m in messages)
            {
                if (m[2] is GameActionFightSpellCastMessage sc && !sorts.ContainsKey(sc.SpellId))
                {
                    try { sorts[sc.SpellId] = Spells.SpellManager.Instance.GetSpellTemplate(sc.SpellId)?.Name; }
                    catch { sorts[sc.SpellId] = null; }
                }
            }

            // racine construite en JObject : seuls les messages portent « $type » (pas d'emballage « $values » des listes)
            var avecTypes = JsonSerializer.Create(Reglages);
            var replay = new Newtonsoft.Json.Linq.JObject
            {
                ["version"] = 1,
                ["combat"] = f.Id,
                ["type"] = f.FightType.ToString(),
                ["typeId"] = (int)f.FightType,
                ["attaquant"] = f.ChallengersTeam?.Leader?.Id ?? 0,
                ["defenseur"] = f.DefendersTeam?.Leader?.Id ?? 0,
                ["carte"] = f.Map.Id,
                ["x"] = f.Map.Position.X,
                ["y"] = f.Map.Position.Y,
                ["zone"] = ZoneNom(f),
                ["debut"] = e.Debut.ToString("yyyy-MM-dd HH:mm:ss"),
                ["fin"] = e.Fin?.ToString("yyyy-MM-dd HH:mm:ss"),
                ["coupe"] = e.Coupe,
                ["cases"] = cases.ToString(),
                ["combattants"] = Newtonsoft.Json.Linq.JToken.FromObject(combattants),
                ["sorts"] = Newtonsoft.Json.Linq.JToken.FromObject(sorts),
                ["messages"] = Newtonsoft.Json.Linq.JToken.FromObject(messages.ToArray(), avecTypes),
                ["journal"] = journal,
            };

            var json = replay.ToString(Formatting.None);
            using (var sortie = new MemoryStream())
            {
                using (var gz = new GZipStream(sortie, CompressionLevel.Optimal))
                {
                    var octets = Encoding.UTF8.GetBytes(json);
                    gz.Write(octets, 0, octets.Length);
                }
                return sortie.ToArray();
            }
        }

        static string ZoneNom(IFight f)
        {
            try
            {
                return f.Map.SubArea?.Record?.Name;
            }
            catch
            {
                return null;
            }
        }

        static void PreparerTable()
        {
            if (m_tablePrete)
                return;
            Database.Execute(@"CREATE TABLE IF NOT EXISTS padgref_replays (
                Cle CHAR(32) NOT NULL PRIMARY KEY, CombatId INT NOT NULL, CharacterId INT NOT NULL, CharacterName VARCHAR(64) NOT NULL,
                MapId INT NOT NULL, CreatedAt DATETIME NOT NULL, EnCours TINYINT NOT NULL DEFAULT 0, Donnees LONGBLOB NOT NULL,
                KEY (CreatedAt)) DEFAULT CHARSET=utf8mb4");
            m_tablePrete = true;
        }

        static readonly System.Net.Http.HttpClient m_http = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        /// <summary>PADGREF 23/10 : chaque replay créé par un joueur est annoncé sur Discord (pseudo en jeu, compte, carte, lien).</summary>
        static void EnvoyerDiscord(Character perso, string lien, bool enCours)
        {
            var webhook = WebhookDiscord;
            if (string.IsNullOrWhiteSpace(webhook))
                return;
            var nom = perso.Name;
            var compte = perso.Account?.Nickname ?? perso.Account?.Login ?? "?";
            var carte = perso.Map != null ? $"{perso.Map.Id} [{perso.Map.Position.X},{perso.Map.Position.Y}]" : "?";
            Task.Run(async () =>
            {
                try
                {
                    System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;
                    var corps = new Newtonsoft.Json.Linq.JObject
                    {
                        ["username"] = "Padgref - Replays",
                        ["embeds"] = new Newtonsoft.Json.Linq.JArray(new Newtonsoft.Json.Linq.JObject
                        {
                            ["title"] = "Replay de " + nom,
                            ["url"] = lien,
                            ["color"] = 0x38E1FF,
                            ["description"] = "**Personnage :** " + nom + "\n**Compte :** " + compte + "\n**Carte :** " + carte +
                                (enCours ? "\n*(combat encore en cours au moment du clic)*" : "") + "\n\n[Ouvrir le replay](" + lien + ")",
                            ["timestamp"] = DateTime.UtcNow.ToString("o")
                        })
                    };
                    using (var contenu = new System.Net.Http.StringContent(corps.ToString(Formatting.None), Encoding.UTF8, "application/json"))
                    using (var reponse = await m_http.PostAsync(webhook, contenu))
                        if (!reponse.IsSuccessStatusCode)
                            logger.Warn("[Replay] Discord : réponse {0}", (int)reponse.StatusCode);
                }
                catch (Exception ex)
                {
                    logger.Warn("[Replay] Discord : envoi impossible : {0}", ex.Message);
                }
            });
        }

        /// <summary>Replay enregistré en base (JSON compressé), ou null.</summary>
        public static byte[] Lire(string cle)
        {
            if (cle == null || cle.Length != 32 || !cle.All(Uri.IsHexDigit))
                return null;
            PreparerTable();
            return Database.ExecuteScalar<byte[]>("SELECT Donnees FROM padgref_replays WHERE Cle = @0", cle.ToLowerInvariant());
        }

        static Dictionary<string, Type> m_typesMessages;

        /// <summary>Replay décompressé : informations générales + messages retypés (pour les renvoyer au client tels quels).</summary>
        public static Newtonsoft.Json.Linq.JObject Ouvrir(byte[] gz, out List<KeyValuePair<int, Message>> messages)
        {
            string json;
            using (var entree = new GZipStream(new MemoryStream(gz), CompressionMode.Decompress))
            using (var lecteur = new StreamReader(entree, Encoding.UTF8))
                json = lecteur.ReadToEnd();

            var racine = Newtonsoft.Json.Linq.JObject.Parse(json);
            if (m_typesMessages == null)
                m_typesMessages = typeof(Message).Assembly.GetTypes().Where(t => typeof(Message).IsAssignableFrom(t) && !t.IsAbstract)
                    .GroupBy(t => t.Name).ToDictionary(g => g.Key, g => g.First());

            var serialiseur = JsonSerializer.Create(Reglages);
            messages = new List<KeyValuePair<int, Message>>();
            foreach (var l in racine["messages"] as Newtonsoft.Json.Linq.JArray ?? new Newtonsoft.Json.Linq.JArray())
            {
                if (!(l is Newtonsoft.Json.Linq.JArray a) || a.Count < 3 || !m_typesMessages.TryGetValue((string)a[1], out var type))
                    continue;
                try
                {
                    if (a[2].ToObject(type, serialiseur) is Message m)
                        messages.Add(new KeyValuePair<int, Message>((int)a[0], m));
                }
                catch (Exception ex)
                {
                    logger.Debug("[Replay] message {0} illisible : {1}", a[1], ex.Message);
                }
            }
            racine.Remove("messages");
            return racine;
        }

        /// <summary>
        /// Écrit le replay du combat en cours (ou du dernier combat) du personnage et renvoie le lien de la page du site ;
        /// null + message d'erreur si rien à montrer.
        /// </summary>
        public static string CreerLien(Character perso, out string erreur) => CreerLien(perso, out erreur, out _);

        /// <summary>Idem, avec le replay écrit (pour le rejouer aussitôt dans le jeu).</summary>
        public static string CreerLien(Character perso, out string erreur, out byte[] replay)
        {
            erreur = null;
            replay = null;
            if (m_derniersLiens.TryGetValue(perso.Id, out var dernier) && (DateTime.Now - dernier).TotalSeconds < DelaiEntreLiensSecondes)
            {
                erreur = "attends quelques secondes avant de redemander un replay.";
                return null;
            }

            if (m_enPreparation.TryGetValue(perso.Id, out var preparation))
                preparation.Wait(5000);

            byte[] donnees;
            int combatId, carte;
            var enCours = perso.IsFighting() && perso.Fight != null && m_enCours.TryGetValue(perso.Fight, out var e);
            if (enCours)
            {
                m_enCours.TryGetValue(perso.Fight, out e);
                donnees = Compresser(perso.Fight, e, JournalCombat.TexteCombat(perso.Fight));
                combatId = perso.Fight.Id;
                carte = perso.Fight.Map.Id;
            }
            else if (m_dernierParPerso.TryGetValue(perso.Id, out donnees))
            {
                combatId = 0;
                carte = perso.Map?.Id ?? 0;
            }
            else
            {
                erreur = "aucun combat enregistré pour ce personnage depuis le démarrage du serveur.";
                return null;
            }

            var octets = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(octets);
            var cle = string.Concat(octets.Select(b => b.ToString("x2")));

            PreparerTable();
            Database.Execute("DELETE FROM padgref_replays WHERE CreatedAt < NOW() - INTERVAL @0 DAY", ConservationJours);
            Database.Execute(@"INSERT INTO padgref_replays (Cle, CombatId, CharacterId, CharacterName, MapId, CreatedAt, EnCours, Donnees)
                VALUES (@0, @1, @2, @3, @4, NOW(), @5, @6)", cle, combatId, perso.Id, perso.Name, carte, enCours ? 1 : 0, donnees);

            m_derniersLiens[perso.Id] = DateTime.Now;
            replay = donnees;
            logger.Info("[Replay] {0} : replay {1} ({2} Ko{3})", perso.Name, cle, donnees.Length / 1024, enCours ? ", combat en cours" : "");
            var lien = SiteUrl + "replay.html?r=" + cle;
            EnvoyerDiscord(perso, lien, enCours);
            return lien;
        }
    }
}
