using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Stump.Core.Mathematics;
using Stump.DofusProtocol.Enums;
using Stump.ORM.SubSonic.SQLGeneration.Schema;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.Items;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Results;
using Stump.Server.WorldServer.Game.Maps.Cells;

namespace Stump.Server.WorldServer.Game.Items.Player.Custom
{
    /// <summary>PADGREF 21/10 : une ligne de padgref_box_boss (SQL 87, généré par _OUTILS/client/client-build/box_boss/generer.py).</summary>
    [TableName("padgref_box_boss")]
    public class BoxDeBossRecord
    {
        [PrimaryKey("ItemId", false)]
        public int ItemId { get; set; }
        public string BossIdsCSV { get; set; }
        public string RaresCSV { get; set; }
        public string PanoplieCSV { get; set; }
    }

    /// <summary>
    /// PADGREF 21/10 : box de boss (une par boss de donjon, icône = coffre + portrait du boss).
    ///   - Fin d'un combat GAGNÉ contre un boss, sur une carte de donjon : UNE box par combat, donnée à un joueur de l'équipe au hasard
    ///     (FightPvM.GetResults -> DonnerBox). Rien en arène (combat d'un autre type) ni hors donjon.
    ///   - Ouverture (clic dans l'inventaire, hors combat) : la box est CONSOMMÉE.
    ///       50 % : 1 ressource rare du boss, 25 % : 2, 20 % : 3, 5 % : 1 objet au hasard de la panoplie du boss
    ///       (sans panoplie : 5 ressources rares) ; jackpot 0,0001 % : la panoplie entière.
    /// </summary>
    public class BoxDeBoss : BasePlayerItem
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();
        static readonly CryptoRandom s_hasard = new CryptoRandom();
        public const double Jackpot = 0.000001;     // 0,0001 %

        class Contenu
        {
            public int Box;
            public int[] Rares, Panoplie;
        }

        static Dictionary<int, Contenu> s_parBox = new Dictionary<int, Contenu>();
        static Dictionary<int, Contenu> s_parBoss = new Dictionary<int, Contenu>();

        public BoxDeBoss(Character owner, PlayerItemRecord record) : base(owner, record) { }

        /// <summary>PADGREF 22/10 : vrai pour un objet box de boss (sert à le mettre en tête du butin de fin de combat).</summary>
        public static bool EstBox(int itemId) => s_parBox.ContainsKey(itemId);

        static int[] Ids(string csv) => (csv ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => int.TryParse(x.Trim(), out var v) ? v : 0).Where(v => v > 0).ToArray();

        [Initialization(typeof(ItemManager))]
        public static void Charger()
        {
            try
            {
                var db = WorldServer.Instance.DBAccessor.Database;
                if (db.ExecuteScalar<int>("SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'padgref_box_boss'") == 0)
                {
                    logger.Warn("[Box de boss] table padgref_box_boss absente (SQL 87) : pas de box");
                    return;
                }
                var parBox = new Dictionary<int, Contenu>();
                var parBoss = new Dictionary<int, Contenu>();
                foreach (var r in db.Query<BoxDeBossRecord>("SELECT ItemId, BossIdsCSV, RaresCSV, PanoplieCSV FROM padgref_box_boss"))
                {
                    var c = new Contenu
                    {
                        Box = r.ItemId,
                        Rares = Ids(r.RaresCSV).Where(i => ItemManager.Instance.TryGetTemplate(i) != null).ToArray(),
                        Panoplie = Ids(r.PanoplieCSV).Where(i => ItemManager.Instance.TryGetTemplate(i) != null).ToArray(),
                    };
                    if (ItemManager.Instance.TryGetTemplate(c.Box) == null || (c.Rares.Length == 0 && c.Panoplie.Length == 0))
                        continue;
                    parBox[c.Box] = c;
                    foreach (var b in Ids(r.BossIdsCSV))
                        parBoss[b] = c;
                    try { ItemManager.Instance.AddItemIdConstructor(typeof(BoxDeBoss), (ItemIdEnum)c.Box); }
                    catch (Exception ex) { logger.Warn($"[Box de boss] objet {c.Box} déjà géré ailleurs : {ex.Message}"); }
                }
                s_parBox = parBox;
                s_parBoss = parBoss;
                logger.Info($"[Box de boss] {parBox.Count} box, {parBoss.Count} boss");
            }
            catch (Exception ex)
            {
                logger.Error($"[Box de boss] chargement impossible : {ex}");
            }
        }

        /// <summary>Fin de combat (vainqueurs = joueurs, carte de donjon) : une box du boss tué, à un joueur au hasard.</summary>
        public static void DonnerBox(FightPvM fight, IEnumerable<IFightResult> results)
        {
            try
            {
                if (s_parBoss.Count == 0 || fight.Winners == null || fight.Winners != fight.ChallengersTeam || fight.Map == null || !fight.Map.IsDungeon())
                    return;
                var boites = fight.DefendersTeam.GetAllFighters<MonsterFighter>()
                    .Select(m => s_parBoss.TryGetValue(m.Monster.Template.Id, out var c) ? c : null)
                    .Where(c => c != null).Distinct().ToList();
                if (boites.Count == 0)
                    return;
                var joueurs = results.OfType<FightPlayerResult>()
                    .Where(r => r.Fighter != null && r.Fighter.Team == fight.Winners && r.Character != null).ToList();
                if (joueurs.Count == 0)
                    return;
                var box = boites[s_hasard.Next(boites.Count)];
                var gagnant = joueurs[s_hasard.Next(joueurs.Count)];
                gagnant.Loot.AddItem(new DroppedItem(box.Box, 1));
            }
            catch (Exception ex)
            {
                logger.Error($"[Box de boss] fin de combat : {ex}");
            }
        }

        public override uint UseItem(int amount = 1, Cell targetCell = null, Character target = null)
        {
            if (Owner.IsInFight() || !s_parBox.TryGetValue(Template.Id, out var c))
                return 0;

            var gains = new List<int>();
            var tirage = s_hasard.NextDouble();
            if (tirage < Jackpot && c.Panoplie.Length > 0)
            {
                gains.AddRange(c.Panoplie);
                World.Instance.SendAnnounce($"<b>{Owner.Name}</b> a ouvert une <b>{Template.Name}</b> et remporte le JACKPOT : la panoplie entière !");
            }
            else if (tirage < 0.50) gains.AddRange(Rares(c, 1));
            else if (tirage < 0.75) gains.AddRange(Rares(c, 2));
            else if (tirage < 0.95) gains.AddRange(Rares(c, 3));
            else if (c.Panoplie.Length > 0) gains.Add(c.Panoplie[s_hasard.Next(c.Panoplie.Length)]);
            else gains.AddRange(Rares(c, 5));

            foreach (var g in gains.GroupBy(x => x))
            {
                var template = ItemManager.Instance.TryGetTemplate(g.Key);
                if (template == null)
                    continue;
                Owner.Inventory.AddItem(template, g.Count());
                Owner.SendInformationMessage(TextInformationTypeEnum.TEXT_INFORMATION_MESSAGE, 21, g.Count(), g.Key);   // « Vous avez obtenu … »
            }
            return 1;   // la box est consommée
        }

        static IEnumerable<int> Rares(Contenu c, int n)
        {
            if (c.Rares.Length == 0)
                return c.Panoplie.Length > 0 ? new[] { c.Panoplie[s_hasard.Next(c.Panoplie.Length)] } : new int[0];
            return Enumerable.Range(0, n).Select(_ => c.Rares[s_hasard.Next(c.Rares.Length)]).ToList();
        }
    }
}
