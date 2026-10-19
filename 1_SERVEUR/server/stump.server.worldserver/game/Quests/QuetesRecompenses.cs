using System;
using System.Collections.Generic;
using NLog;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Items;

namespace Stump.Server.WorldServer.Game.Quests
{
    /// <summary>
    /// MYSTIC 19/10 : récompense donnée dès qu'un OBJECTIF de quête est validé (table mystic_quetes_recompenses, SQL 68) :
    /// « Le tour du monde » (1/2) et (2/2) : chaque boss vaincu rapporte de l'XP + 1 ressource rare de ce boss.
    /// </summary>
    public static class QuetesRecompenses
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();
        static readonly System.Drawing.Color Couleur = System.Drawing.Color.FromArgb(0xE8, 0xC5, 0x47);

        public class Recompense
        {
            public int ObjectiveId { get; set; }
            public int ItemId { get; set; }
            public int Quantite { get; set; }
            public long Xp { get; set; }
        }

        static Dictionary<int, Recompense> m_recompenses;

        static Dictionary<int, Recompense> Toutes()
        {
            if (m_recompenses != null)
                return m_recompenses;
            var d = new Dictionary<int, Recompense>();
            try
            {
                foreach (var r in WorldServer.Instance.DBAccessor.Database.Fetch<Recompense>("SELECT ObjectiveId, ItemId, Quantite, Xp FROM mystic_quetes_recompenses"))
                    d[r.ObjectiveId] = r;
            }
            catch (Exception ex)
            {
                logger.Warn("[Quêtes] table mystic_quetes_recompenses illisible (SQL 68 passé ?) : {0}", ex.Message);
            }
            return m_recompenses = d;
        }

        public static void Donner(Character c, int objectiveId)
        {
            if (c == null || !Toutes().TryGetValue(objectiveId, out var r))
                return;

            try
            {
                var textes = new List<string>();
                if (r.Xp > 0)
                {
                    c.AddExperience(r.Xp);
                    textes.Add($"{r.Xp:N0} points d'expérience".Replace('\u00A0', ' '));
                }

                var template = r.ItemId > 0 ? ItemManager.Instance.TryGetTemplate(r.ItemId) : null;
                if (template != null && r.Quantite > 0)
                {
                    c.Inventory.AddItem(template, r.Quantite);
                    textes.Add($"{r.Quantite} × {template.Name}");
                }

                c.RefreshStats();
                if (textes.Count > 0)
                    c.SendServerMessage("<b>Le tour du monde : boss vaincu !</b> Récompense : " + string.Join(" et ", textes) + ".", Couleur);
            }
            catch (Exception ex)
            {
                logger.Error("[Quêtes] récompense de l'objectif {0} pour {1} impossible : {2}", objectiveId, c.Name, ex);
            }
        }
    }
}
