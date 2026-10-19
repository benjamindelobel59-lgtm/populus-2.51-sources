using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Stump.Core.Attributes;
using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.DofusProtocol.Types;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Database.Characters;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.HavenBags;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Handlers.Characters;

namespace Stump.Server.WorldServer.Game.Heroique
{
    /// <summary>
    /// MYSTIC : mode héroïque (comme le serveur officiel Oto Mustam).
    ///  - perdre un combat PvM / agression / percepteur / prisme = mort définitive (l'abandon compte comme une défaite) ;
    ///  - un personnage mort reste dans la liste (tombe), ne peut plus entrer en jeu, et peut être « recommencé » (niveau 1) ;
    ///  - mort en PvM : l'inventaire reste figé sur le personnage (un admin peut le ressusciter intact avec .resu) ;
    ///  - mort en agression : inventaire, équipement et kamas répartis entre les vainqueurs ;
    ///  - agressions sans alignement : il faut une guilde (ou une alliance), un plus faible ne peut pas agresser un joueur
    ///    de plus de 50 niveaux au-dessus, zones protégées (Incarnam, villes, havre-sacs, donjons).
    /// État des personnages : table mystic_heroique ; journal des morts : table mystic_morts.
    /// </summary>
    public class HeroicManager : Singleton<HeroicManager>
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        static Stump.ORM.Database Database => WorldServer.Instance.DBAccessor.Database;

        /// <summary>Interrupteur général du mode héroïque.</summary>
        [Variable(true)]
        public static bool Enabled = true;

        /// <summary>Types de combat où la défaite est mortelle (FightTypeEnum) : 1 agression, 2 PvMA, 4 PvM, 5 percepteur, 9 prisme.</summary>
        [Variable(true)]
        public static string DeathFightTypes = "1,2,4,5,9";

        /// <summary>Un agresseur ne peut pas viser un joueur qui a plus de N niveaux de plus que lui (l'inverse est permis).</summary>
        [Variable(true)]
        public static int AggressionMaxLevelAbove = 50;

        /// <summary>Niveau minimum pour pouvoir être agressé.</summary>
        [Variable(true)]
        public static int AggressionMinTargetLevel = 20;

        /// <summary>L'agresseur doit faire partie d'une guilde (et donc éventuellement d'une alliance).</summary>
        [Variable(true)]
        public static bool AggressionRequiresGuild = true;

        /// <summary>Zones (Area) protégées : 45 Incarnam.</summary>
        [Variable(true)]
        public static string ProtectedAreas = "45";

        /// <summary>Sous-zones protégées : cité d'Astrub, quartiers et centres-villes de Bonta et de Brâkmar.</summary>
        [Variable(true)]
        public static string ProtectedSubAreas = "95,44,46,47,48,49,50,51,513,530,532,533,845,502,503,505,506,507,508,509,511,531,534,535";

        /// <summary>Délai (secondes) avant le renvoi automatique à l'écran de sélection après la mort ; 0 = le joueur reste sur sa tombe.</summary>
        [Variable(true)]
        public static int KickDelaySeconds = 0;

        public class Etat
        {
            public int CharacterId { get; set; }
            public sbyte DeathState { get; set; }
            public int DeathCount { get; set; }
            public int DeathMaxLevel { get; set; }
            public DateTime? DerniereMort { get; set; }
            public bool MortPvP { get; set; }
        }

        readonly ConcurrentDictionary<int, Etat> m_etats = new ConcurrentDictionary<int, Etat>();
        HashSet<int> m_typesMortels = new HashSet<int>();
        HashSet<int> m_zonesProtegees = new HashSet<int>();
        HashSet<int> m_sousZonesProtegees = new HashSet<int>();

        [Initialization(InitializationPass.Last)]
        public void Initialize()
        {
            Database.Execute(@"CREATE TABLE IF NOT EXISTS mystic_heroique (CharacterId INT NOT NULL PRIMARY KEY,
                DeathState TINYINT NOT NULL DEFAULT 0, DeathCount INT NOT NULL DEFAULT 0, DeathMaxLevel INT NOT NULL DEFAULT 0,
                DerniereMort DATETIME NULL, MortPvP TINYINT NOT NULL DEFAULT 0) DEFAULT CHARSET=utf8");
            Database.Execute(@"CREATE TABLE IF NOT EXISTS mystic_morts (Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, CharacterId INT NOT NULL,
                Nom VARCHAR(50) NOT NULL, Niveau INT NOT NULL, Date DATETIME NOT NULL, MapId INT NOT NULL, TypeCombat VARCHAR(30) NOT NULL,
                Adversaires VARCHAR(500) NULL, Butin MEDIUMTEXT NULL, Ressuscite TINYINT NOT NULL DEFAULT 0, KEY (CharacterId)) DEFAULT CHARSET=utf8");

            foreach (var e in Database.Fetch<Etat>("SELECT CharacterId, DeathState, DeathCount, DeathMaxLevel, DerniereMort, MortPvP FROM mystic_heroique"))
                m_etats[e.CharacterId] = e;

            // MYSTIC 19/10 : interrupteur gardé en base (commande .modeheroique on|off), prioritaire sur world_config.xml
            Database.Execute("CREATE TABLE IF NOT EXISTS mystic_reglages (Cle VARCHAR(64) NOT NULL PRIMARY KEY, Valeur VARCHAR(255) NOT NULL) DEFAULT CHARSET=utf8");
            var reglage = Database.ExecuteScalar<string>("SELECT Valeur FROM mystic_reglages WHERE Cle = 'heroique_actif'");
            if (reglage != null)
                Enabled = reglage == "1";

            m_typesMortels = ParseIds(DeathFightTypes);
            m_zonesProtegees = ParseIds(ProtectedAreas);
            m_sousZonesProtegees = ParseIds(ProtectedSubAreas);

            logger.Info("[Héroïque] mode {0} : {1} personnage(s) mort(s), {2} zone(s) et {3} sous-zone(s) protégée(s)",
                Enabled ? "activé" : "désactivé", m_etats.Values.Count(x => x.DeathState == (sbyte)HardcoreOrEpicDeathStateEnum.DEATH_STATE_DEAD),
                m_zonesProtegees.Count, m_sousZonesProtegees.Count);
        }

        static HashSet<int> ParseIds(string csv) =>
            new HashSet<int>((csv ?? "").Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => int.TryParse(x.Trim(), out var v) ? v : int.MinValue).Where(x => x != int.MinValue));

        #region État

        public Etat GetEtat(int characterId) => m_etats.TryGetValue(characterId, out var e) ? e : null;

        public bool IsDead(int characterId) =>
            Enabled && m_etats.TryGetValue(characterId, out var e) && e.DeathState == (sbyte)HardcoreOrEpicDeathStateEnum.DEATH_STATE_DEAD;

        void Sauver(Etat e)
        {
            WorldServer.Instance.IOTaskPool.AddMessage(() =>
                Database.Execute(@"REPLACE INTO mystic_heroique (CharacterId, DeathState, DeathCount, DeathMaxLevel, DerniereMort, MortPvP)
                    VALUES (@0, @1, @2, @3, @4, @5)", e.CharacterId, e.DeathState, e.DeathCount, e.DeathMaxLevel, e.DerniereMort, e.MortPvP ? 1 : 0));
        }

        /// <summary>Informations envoyées dans l'écran de sélection (tombe, nombre de morts, record de niveau).</summary>
        public CharacterBaseInformations BuildListInformations(CharacterRecord record, ushort level, EntityLook look)
        {
            // MYSTIC 19/10 : TOUJOURS la fiche héroïque : le monde 22 est un serveur héroïque pour le client, qui plante sur une fiche
            // normale (« cannot convert CharacterBaseInformations to CharacterHardcoreOrEpicInformations ») et reste bloqué à la
            // sélection des personnages. Mode désactivé : tout le monde apparaît vivant.
            var e = GetEtat(record.Id);
            return new CharacterHardcoreOrEpicInformations((ulong)record.Id, record.Name, level, look, (sbyte)record.Breed,
                record.Sex == SexTypeEnum.SEX_MALE,
                Enabled ? (e?.DeathState ?? (sbyte)HardcoreOrEpicDeathStateEnum.DEATH_STATE_ALIVE) : (sbyte)HardcoreOrEpicDeathStateEnum.DEATH_STATE_ALIVE,
                (ushort)Math.Min(ushort.MaxValue, e?.DeathCount ?? 0),
                (ushort)Math.Max(level, e?.DeathMaxLevel ?? 0));
        }

        #endregion

        #region Mort

        /// <summary>Vrai si perdre ce combat est mortel.</summary>
        /// <summary>MYSTIC 19/10 : active / désactive la mort définitive (enregistré en base, sans redémarrage).</summary>
        public void Activer(bool actif)
        {
            Enabled = actif;
            Database.Execute("REPLACE INTO mystic_reglages (Cle, Valeur) VALUES ('heroique_actif', @0)", actif ? "1" : "0");
            logger.Info("[Héroïque] mode {0} par commande", actif ? "activé" : "désactivé");
        }

        public bool IsDeadlyFight(IFight fight) => Enabled && fight != null && m_typesMortels.Contains((int)fight.FightType);

        /// <summary>
        /// Écran de fin de combat : les perdants (et ceux qui ont abandonné) voient dans la colonne « Objets » tout ce qu'ils vont
        /// perdre (affichage seul : IgnoreGeneration, rien n'est créé). La perte réelle a lieu ensuite dans OnLostFight.
        /// </summary>
        // agressions déjà pillées pendant le calcul de l'écran de fin de combat (OnLostFight ne refait pas la répartition)
        readonly ConcurrentDictionary<int, string> m_pillagesFaits = new ConcurrentDictionary<int, string>();

        public void AfficherPertes(IFight fight, List<Stump.Server.WorldServer.Game.Fights.Results.IFightResult> results)
        {
            if (!IsDeadlyFight(fight))
                return;

            var pvp = fight.FightType == FightTypeEnum.FIGHT_TYPE_AGRESSION;
            var perdants = results.OfType<Stump.Server.WorldServer.Game.Fights.Results.FightPlayerResult>()
                .Where(r => r.Character != null && !r.Character.GodMode && (r.Fighter.Team == fight.Losers || r.Fighter.HasLeft())).ToList();
            if (perdants.Count == 0)
                return;

            if (pvp)
            {
                // agression : la répartition réelle se fait ici, chaque vainqueur voit sur sa ligne ce qu'il reçoit
                var vainqueurs = results.OfType<Stump.Server.WorldServer.Game.Fights.Results.FightPlayerResult>()
                    .Where(r => r.Character != null && r.Fighter.Team == fight.Winners && !r.Fighter.HasLeft()).ToList();
                foreach (var p in perdants)
                    m_pillagesFaits[p.Character.Id] = DistribuerButin(p.Character, fight, vainqueurs);
                return;
            }

            // PvM : les objets du mort s'affichent sur les lignes des monstres vainqueurs (qui les emportent dans la zone)
            var monstres = results.OfType<Stump.Server.WorldServer.Game.Fights.Results.FightResult>()
                .Where(r => r.Fighter is MonsterFighter && r.Fighter.Team == fight.Winners).ToList();
            var i = 0;
            foreach (var p in perdants)
                foreach (var objet in p.Character.Inventory.ToArray())
                {
                    var ligne = monstres.Count > 0 ? monstres[i++ % monstres.Count].Loot : p.Loot;
                    ligne.AddItem(new DroppedItem(objet.Template.Id, objet.Stack) { IgnoreGeneration = true });
                }
        }

        /// <summary>
        /// Appelé par Character.RejoinMap quand le personnage a perdu (ou abandonné) un combat.
        /// </summary>
        public void OnLostFight(Character character, IFight fight)
        {
            if (!IsDeadlyFight(fight) || character == null || character.GodMode)
                return;

            var pvp = fight.FightType == FightTypeEnum.FIGHT_TYPE_AGRESSION;
            var objetsPerdus = character.Inventory.Count;
            var kamasPerdus = character.Inventory.Kamas;
            var etat = m_etats.GetOrAdd(character.Id, id => new Etat { CharacterId = id });
            etat.DeathState = (sbyte)HardcoreOrEpicDeathStateEnum.DEATH_STATE_DEAD;
            etat.DeathCount++;
            etat.DeathMaxLevel = Math.Max(etat.DeathMaxLevel, character.Level);
            etat.DerniereMort = DateTime.Now;
            etat.MortPvP = pvp;
            Sauver(etat);

            var adversaires = fight.Winners?.Fighters.Select(x => x is CharacterFighter cf ? cf.Character.Name : x.ToString()).Distinct().Take(10).ToArray()
                              ?? new string[0];

            // agression : butin aux vainqueurs ; PvM : butin réparti entre les groupes de monstres de la sous-zone (coffres)
            string butin;
            if (pvp)
                butin = m_pillagesFaits.TryRemove(character.Id, out var dejaFait) ? dejaFait : DistribuerButin(character, fight, null);
            else
                butin = ButinHeroique.Instance.DonnerAuxMonstres(character, fight);

            var nom = character.Name;
            var niveau = character.Level;
            var carte = character.Map?.Id ?? 0;
            var type = fight.FightType.ToString().Replace("FIGHT_TYPE_", "");
            var listeAdv = string.Join(", ", adversaires);
            WorldServer.Instance.IOTaskPool.AddMessage(() =>
                Database.Execute(@"INSERT INTO mystic_morts (CharacterId, Nom, Niveau, Date, MapId, TypeCombat, Adversaires, Butin)
                    VALUES (@0, @1, @2, NOW(), @3, @4, @5, @6)", character.Id, nom, niveau, carte, type, listeAdv, butin));

            logger.Info("[Héroïque] {0} (niveau {1}) est mort en combat {2} contre {3}", nom, niveau, type, listeAdv);

            // annonce à tous les joueurs connectés
            World.Instance.SendAnnounce($"☠ {nom} (niveau {niveau}) est tombé au combat. Sa mort est définitive.", System.Drawing.Color.DarkRed);

            if (character.IsLoggedIn && character.Client != null)
            {
                character.SendServerMessage(
                    "☠ Vous êtes mort. En mode héroïque, la mort est définitive : votre tombe reste ici et vous ne pouvez plus agir." +
                    $" Vous perdez {objetsPerdus} objet(s) et {kamasPerdus} kamas :" +
                    (pvp ? " ils ont été pris par vos adversaires." : " ils ont été emportés par les monstres de la zone, battez-les pour les récupérer (coffre au-dessus des groupes).") +
                    " Retournez à l'écran de sélection pour « Recommencer » ou jouer un autre personnage.",
                    System.Drawing.Color.Red);

                if (KickDelaySeconds > 0)
                {
                    var client = character.Client;
                    WorldServer.Instance.IOTaskPool.CallDelayed(KickDelaySeconds * 1000, () =>
                    {
                        if (client.Connected)
                            client.Disconnect();
                    });
                }
            }
        }

        /// <summary>Agression perdue : inventaire, équipement et kamas répartis au hasard entre les vainqueurs.</summary>
        /// <param name="lignes">lignes des vainqueurs dans l'écran de fin de combat (null : appel tardif, sans affichage)</param>
        string DistribuerButin(Character mort, IFight fight, List<Stump.Server.WorldServer.Game.Fights.Results.FightPlayerResult> lignes)
        {
            var vainqueurs = (lignes != null ? lignes.Select(x => x.Character) : fight.Winners?.Fighters.OfType<CharacterFighter>().Select(x => x.Character))
                ?.Where(x => x != null && x != mort).Distinct().ToList();
            var ligneDe = lignes?.Where(x => x.Character != null).GroupBy(x => x.Character).ToDictionary(x => x.Key, x => x.First());
            if (vainqueurs == null || vainqueurs.Count == 0)
                return "";

            var rnd = new Random();
            var parVainqueur = vainqueurs.ToDictionary(x => x, x => new List<string>());

            foreach (var objet in mort.Inventory.ToArray())
            {
                try
                {
                    var gagnant = vainqueurs[rnd.Next(vainqueurs.Count)];
                    var copie = ItemManager.Instance.CreatePlayerItem(gagnant, objet);
                    gagnant.Inventory.AddItem(copie);
                    mort.Inventory.RemoveItem(objet, true, false);
                    // affichage dans l'écran de fin de combat, sur la ligne du vainqueur (l'objet exact est déjà dans son inventaire)
                    if (ligneDe != null && ligneDe.TryGetValue(gagnant, out var ligne))
                        ligne.Loot.AddItem(new DroppedItem(copie.Template.Id, copie.Stack) { IgnoreGeneration = true });
                    parVainqueur[gagnant].Add($"{objet.Template.Name} x{objet.Stack}");
                }
                catch (Exception ex)
                {
                    logger.Error("[Héroïque] butin de {0} : objet {1} : {2}", mort.Name, objet.Guid, ex.Message);
                }
            }

            var kamas = mort.Inventory.Kamas;
            if (kamas > 0)
            {
                var part = kamas / (ulong)vainqueurs.Count;
                mort.Inventory.SetKamas(0);
                foreach (var v in vainqueurs)
                {
                    // avec l'écran de fin de combat : colonne « Kamas » (ajoutés à l'application des résultats)
                    if (ligneDe != null && ligneDe.TryGetValue(v, out var ligneK))
                        ligneK.Loot.Kamas += (long)part;
                    else
                        v.Inventory.AddKamas(part);
                    parVainqueur[v].Add($"{part} kamas");
                }
            }

            mort.RefreshActor();

            foreach (var v in vainqueurs)
            {
                var liste = parVainqueur[v];
                v.SendServerMessage(liste.Count == 0
                        ? $"Vous fouillez la dépouille de {mort.Name} : elle était vide."
                        : $"Vous récupérez sur la dépouille de {mort.Name} : {string.Join(", ", liste.Take(15))}{(liste.Count > 15 ? $" et {liste.Count - 15} autre(s) objet(s)" : "")}.",
                    System.Drawing.Color.Orange);
            }

            return string.Join(" | ", parVainqueur.Select(x => x.Key.Name + " : " + string.Join(", ", x.Value)));
        }

        #endregion

        #region Résurrection et « Recommencer »

        /// <summary>Résurrection par un admin (mort par bug) : le personnage revient tel quel, inventaire compris s'il est mort en PvM.</summary>
        public string Resurrect(CharacterRecord record)
        {
            var e = GetEtat(record.Id);
            if (e == null || e.DeathState != (sbyte)HardcoreOrEpicDeathStateEnum.DEATH_STATE_DEAD)
                return $"{record.Name} n'est pas mort.";

            e.DeathState = (sbyte)HardcoreOrEpicDeathStateEnum.DEATH_STATE_ALIVE;
            e.DeathCount = Math.Max(0, e.DeathCount - 1);
            Sauver(e);
            var id = record.Id;
            WorldServer.Instance.IOTaskPool.AddMessage(() =>
                Database.Execute("UPDATE mystic_morts SET Ressuscite = 1 WHERE CharacterId = @0 ORDER BY Id DESC LIMIT 1", id));

            // butin encore porté par les monstres : rendu au ressuscité
            var rendu = ButinHeroique.Instance.RendreAuMort(record);

            logger.Info("[Héroïque] {0} ressuscité par un admin ({1} objet(s), {2} kamas rendus)", record.Name, rendu.Item1, rendu.Item2);
            return e.MortPvP
                ? $"{record.Name} est ressuscité. Attention : mort en agression, ses objets et ses kamas ont été donnés aux vainqueurs et ne reviennent pas."
                : $"{record.Name} est ressuscité : {rendu.Item1} objet(s) et {rendu.Item2} kamas rendus (ce que des joueurs ont déjà récupéré sur les monstres ne revient pas).";
        }

        /// <summary>
        /// « Recommencer » depuis l'écran de sélection : le personnage mort est supprimé et recréé au niveau 1 avec le même nom,
        /// la même classe, le même sexe, la même tête et les mêmes couleurs. Il garde son nombre de morts et son record de niveau.
        /// </summary>
        public void Replay(WorldClient client, int characterId)
        {
            var record = client.Characters?.FirstOrDefault(x => x.Id == characterId && !x.IsDeleted);
            if (record == null || !IsDead(characterId))
            {
                client.Send(new CharacterSelectedErrorMessage());
                return;
            }

            var ancien = GetEtat(characterId);
            var nom = record.Name;
            var breed = (sbyte)record.Breed;
            var femme = record.Sex == SexTypeEnum.SEX_FEMALE;
            var tete = record.Head;
            var look = record.DefaultLook;
            var couleurs = Enumerable.Range(1, 5).Select(i => look.Colors.TryGetValue(i, out var c) ? c.ToArgb() : -1).ToArray();

            // le nom est libéré par la suppression (DeletedDate), puis réutilisé par la création
            record.DeletedDate = DateTime.Now;
            CharacterManager.Instance.Database.Update(record);

            CharacterManager.Instance.CreateCharacter(client, nom, breed, femme, couleurs, tete,
                () =>
                {
                    var nouveau = client.Characters.FirstOrDefault(x => x.Name == nom && !x.IsDeleted);
                    if (nouveau != null)
                    {
                        var e = new Etat
                        {
                            CharacterId = nouveau.Id,
                            DeathState = (sbyte)HardcoreOrEpicDeathStateEnum.DEATH_STATE_ALIVE,
                            DeathCount = ancien?.DeathCount ?? 1,
                            DeathMaxLevel = ancien?.DeathMaxLevel ?? 1
                        };
                        m_etats[e.CharacterId] = e;
                        Sauver(e);
                    }

                    logger.Info("[Héroïque] {0} recommence au niveau 1", nom);

                    // le client attend une entrée en jeu (ou une erreur) après « Recommencer » : sans elle il reste verrouillé
                    // et ignore toutes les sélections suivantes. On fait donc entrer directement le nouveau personnage.
                    WorldServer.Instance.IOTaskPool.AddMessage(() =>
                    {
                        if (nouveau != null && client.Connected)
                            CharacterHandler.CommonCharacterSelection(client, nouveau);
                        else
                        {
                            client.Send(new CharacterSelectedErrorMessage());
                            CharacterHandler.SendCharactersListWithRemodelingMessage(client);
                        }
                    });
                },
                erreur =>
                {
                    // échec : on rend le personnage mort tel qu'il était
                    record.DeletedDate = null;
                    CharacterManager.Instance.Database.Update(record);
                    logger.Warn("[Héroïque] « Recommencer » impossible pour {0} : {1}", nom, erreur);
                    // l'erreur de sélection déverrouille le client (sinon il ignore les clics suivants)
                    client.Send(new CharacterSelectedErrorMessage());
                    CharacterHandler.SendCharactersListWithRemodelingMessage(client);
                });
        }

        #endregion

        #region Agressions

        /// <summary>Règles d'agression du mode héroïque (remplacent celles de l'alignement).</summary>
        public FighterRefusedReasonEnum CanAgress(Character attaquant, Character cible, bool bypassCheck)
        {
            if (!cible.IsInWorld || cible.IsFighting() || cible.IsSpectator() || cible.IsBusy())
                return FighterRefusedReasonEnum.OPPONENT_OCCUPIED;

            if (!bypassCheck && (!attaquant.IsInWorld || attaquant.IsFighting() || attaquant.IsSpectator() || attaquant.IsBusy()))
                return FighterRefusedReasonEnum.IM_OCCUPIED;

            if (!bypassCheck && (cible.Map != attaquant.Map || !attaquant.Map.AllowAggression))
                return FighterRefusedReasonEnum.WRONG_MAP;

            if (EstProtegee(attaquant.Map))
                return FighterRefusedReasonEnum.WRONG_MAP;

            if (string.Equals(cible.Client?.IP, attaquant.Client?.IP) && !attaquant.IsGameMaster())
                return FighterRefusedReasonEnum.MULTIACCOUNT_NOT_ALLOWED;

            if (attaquant.IsGhost() || cible.IsGhost())
                return FighterRefusedReasonEnum.GHOST_REFUSED;

            if (AggressionRequiresGuild && attaquant.GuildMember == null)
                return FighterRefusedReasonEnum.WRONG_GUILD;

            var guildeA = attaquant.GuildMember?.Guild;
            var guildeC = cible.GuildMember?.Guild;
            if (guildeA != null && guildeA == guildeC)
                return FighterRefusedReasonEnum.WRONG_GUILD;

            if (guildeA?.Alliance != null && guildeA.Alliance == guildeC?.Alliance)
                return FighterRefusedReasonEnum.WRONG_ALLIANCE;

            int niveauA = Math.Min(200, (int)attaquant.Level), niveauC = Math.Min(200, (int)cible.Level);
            if (niveauC < AggressionMinTargetLevel)
                return FighterRefusedReasonEnum.INSUFFICIENT_RIGHTS;
            // un plus fort peut agresser n'importe qui de plus faible ; un plus faible pas un joueur de plus de N niveaux au-dessus
            if (niveauC - niveauA > AggressionMaxLevelAbove)
                return FighterRefusedReasonEnum.INSUFFICIENT_RIGHTS;

            if (attaquant.AgressionPenality >= DateTime.Now || cible.AgressionPenality >= DateTime.Now)
                return FighterRefusedReasonEnum.INSUFFICIENT_RIGHTS;

            return FighterRefusedReasonEnum.FIGHTER_ACCEPTED;
        }

        public bool EstProtegee(Maps.Map carte)
        {
            if (carte == null)
                return true;
            if (carte.SubArea != null && (m_sousZonesProtegees.Contains(carte.SubArea.Id) ||
                                          carte.SubArea.Area != null && m_zonesProtegees.Contains(carte.SubArea.Area.Id)))
                return true;
            if (HavenBagManager.Instance.IsHavenBagMap(carte.Id))
                return true;
            return carte.IsDungeon();
        }

        #endregion
    }
}
