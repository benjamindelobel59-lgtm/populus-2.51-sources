using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Stump.Core.Mathematics;
using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.Items.Templates;
using Stump.Server.WorldServer.Game.Effects;
using Stump.Server.WorldServer.Game.Effects.Instances;

namespace Stump.Server.WorldServer.Game.Exchanges.Craft.Runes
{
    /// <summary>
    /// MYSTIC 17/10 : brisage (concasseur) façon officielle, choix de l'utilisateur.
    /// - Poids d'une ligne = (valeur × poids d'un point × niveau de l'objet × 0,015 + 1) × coefficient de l'objet.
    /// - Sans focus : chaque ligne donne ses runes de base (la plus petite rune de la caractéristique) = poids de ligne / poids de la rune ;
    ///   la partie décimale est la chance d'avoir une rune de plus.
    /// - Focus sur une caractéristique : on ne reçoit que des runes de cette caractéristique, avec 100 % du poids de sa ligne + 50 %
    ///   du poids des autres lignes.
    /// - Coefficient propre à chaque objet (table nekfeu_brisage_coef) : départ 4000 % ; chaque objet brisé retire 1 % de la valeur
    ///   actuelle (×0,99 : 4000 % → ~1500 % après 100 brisages, ~100 % après 370) ; il remonte de 1 % de l'écart avec 4000 % par
    ///   heure. Bornes 1 % à 4000 %. Objet sans recette : fixe à 50 % (règle affichée par le client). Choix utilisateur (idée 1).
    /// </summary>
    public class BrisageOfficiel : Singleton<BrisageOfficiel>
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public const double COEF_DEPART = 40;
        public const double COEF_MIN = 0.01;
        public const double COEF_MAX = 40;
        public const double FACTEUR_PAR_OBJET = 0.99;
        public const double REMONTEE_PAR_HEURE = 0.01; // part de l'écart avec COEF_MAX regagnée chaque heure
        public const double COEF_SANS_RECETTE = 0.5;
        public const double FACTEUR_NIVEAU = 0.015;
        public const double PART_FOCUS_AUTRES_LIGNES = 0.5;

        static Stump.ORM.Database Database => WorldServer.Instance.DBAccessor.Database;

        class Coef
        {
            public double Valeur;
            public DateTime MajAt;
        }

        public class LigneCoef
        {
            public int ItemId { get; set; }
            public double Coef { get; set; }
            public DateTime MajAt { get; set; }
        }

        readonly ConcurrentDictionary<int, Coef> m_coefs = new ConcurrentDictionary<int, Coef>();

        [Initialization(InitializationPass.Last)]
        public void Initialize()
        {
            Database.Execute(@"CREATE TABLE IF NOT EXISTS nekfeu_brisage_coef (ItemId INT NOT NULL PRIMARY KEY, Coef DOUBLE NOT NULL,
                MajAt DATETIME NOT NULL) DEFAULT CHARSET=utf8");

            foreach (var l in Database.Fetch<LigneCoef>("SELECT ItemId, Coef, MajAt FROM nekfeu_brisage_coef"))
                m_coefs[l.ItemId] = new Coef { Valeur = l.Coef, MajAt = l.MajAt };

            logger.Info("[Brisage] {0} coefficients d'objets", m_coefs.Count);
        }

        /// <summary>Coefficient actuel de l'objet (avec la remontée depuis la dernière mise à jour).</summary>
        public double Coefficient(ItemTemplate item)
        {
            if (!RuneManager.Instance.EstCraftable(item))
                return COEF_SANS_RECETTE;

            if (!m_coefs.TryGetValue(item.Id, out var c))
                return COEF_DEPART;

            var heures = Math.Max(0, (DateTime.Now - c.MajAt).TotalHours);
            var valeur = COEF_MAX - (COEF_MAX - c.Valeur) * Math.Pow(1 - REMONTEE_PAR_HEURE, heures);
            return Math.Min(COEF_MAX, Math.Max(COEF_MIN, valeur));
        }

        /// <summary>Un objet vient d'être brisé : son coefficient perd 1 % de sa valeur actuelle.</summary>
        public void Briser(ItemTemplate item)
        {
            if (!RuneManager.Instance.EstCraftable(item))
                return;

            var valeur = Math.Max(COEF_MIN, Coefficient(item) * FACTEUR_PAR_OBJET);
            var maintenant = DateTime.Now;
            m_coefs[item.Id] = new Coef { Valeur = valeur, MajAt = maintenant };

            var id = item.Id;
            WorldServer.Instance.IOTaskPool.AddMessage(() =>
                Database.Execute("REPLACE INTO nekfeu_brisage_coef (ItemId, Coef, MajAt) VALUES (@0, @1, @2)", id, valeur, maintenant));
        }

        static double PoidsPoint(EffectInteger effet) => EffectManager.Instance.GetEffectPower(effet.Template);

        /// <summary>
        /// Runes obtenues pour UN objet. focus = id d'effet focalisé (0 = aucun). Ajoute au dictionnaire runes (template -> quantité).
        /// </summary>
        public void Calculer(ItemTemplate template, IEnumerable<EffectBase> effets, double coef, int focus, Dictionary<ItemTemplate, int> runes)
        {
            var lignes = effets.OfType<EffectInteger>()
                .Where(x => x.EffectId != EffectsEnum.Effect_PowerSink && x.Value > 0 && x.Template != null && x.Template.BonusType >= 0)
                .ToArray();

            var rand = new CryptoRandom();

            double PoidsLigne(EffectInteger e) => (e.Value * PoidsPoint(e) * template.Level * FACTEUR_NIVEAU + 1) * coef;

            void Ajouter(EffectInteger ligne, double poids)
            {
                var runesEffet = RuneManager.Instance.GetEffectRunes(ligne.EffectId);
                if (runesEffet.Length == 0 || poids <= 0)
                    return;

                var rune = runesEffet.OrderBy(x => x.Amount).First();
                var poidsRune = PoidsPoint(ligne) * Math.Max(1, rune.Amount);
                if (poidsRune <= 0)
                    return;

                var nombre = poids / poidsRune;
                var quantite = (int)Math.Floor(nombre);
                if (rand.NextDouble() < nombre - quantite)
                    quantite++;

                if (quantite <= 0)
                    return;

                runes[rune.Item] = (runes.TryGetValue(rune.Item, out var deja) ? deja : 0) + quantite;
            }

            var ligneFocus = focus > 0 ? lignes.FirstOrDefault(x => (int)x.EffectId == focus) : null;
            if (ligneFocus != null)
            {
                var poids = PoidsLigne(ligneFocus) + PART_FOCUS_AUTRES_LIGNES * lignes.Where(x => x != ligneFocus).Sum(PoidsLigne);
                Ajouter(ligneFocus, poids);
                return;
            }

            foreach (var ligne in lignes)
                Ajouter(ligne, PoidsLigne(ligne));
        }
    }
}
