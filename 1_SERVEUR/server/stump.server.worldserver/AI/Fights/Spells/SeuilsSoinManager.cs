using System;
using System.Collections.Concurrent;
using NLog;
using Stump.Core.Reflection;
using Stump.Server.BaseServer.Initialization;

namespace Stump.Server.WorldServer.AI.Fights.Spells
{
    /// <summary>
    /// MYSTIC : seuil de soin d'urgence par monstre (table nekfeu_ia_soins : MonsterId, Seuil en %).
    /// Sous ce seuil de PV (le monstre ou un allié), un monstre qui a un sort de soin soigne avant d'attaquer.
    /// Monstre absent de la table = SpellSelector.HealUrgencyPercent (50 % par défaut) ; 0 = ne soigne jamais en priorité.
    /// Modifiable en jeu par le staff : .seuilsoin &lt;id monstre&gt; &lt;%&gt; (enregistré en base, effet immédiat).
    /// </summary>
    public class SeuilsSoinManager : Singleton<SeuilsSoinManager>
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static Stump.ORM.Database Database => WorldServer.Instance.DBAccessor.Database;

        private readonly ConcurrentDictionary<int, int> m_seuils = new ConcurrentDictionary<int, int>();

        private class SeuilRow
        {
            public int MonsterId { get; set; }
            public int Seuil { get; set; }
        }

        [Initialization(InitializationPass.Sixth)]
        public void Initialize()
        {
            try
            {
                Database.Execute("CREATE TABLE IF NOT EXISTS nekfeu_ia_soins (MonsterId INT NOT NULL PRIMARY KEY, Seuil INT NOT NULL) DEFAULT CHARSET=utf8");
                foreach (var row in Database.Fetch<SeuilRow>("SELECT MonsterId, Seuil FROM nekfeu_ia_soins"))
                    m_seuils[row.MonsterId] = row.Seuil;
                if (m_seuils.Count > 0)
                    logger.Info("[IA] {0} seuil(s) de soin par monstre (defaut {1} %)", m_seuils.Count, SpellSelector.HealUrgencyPercent);
            }
            catch (Exception ex)
            {
                logger.Error("[IA] table nekfeu_ia_soins illisible : {0}", ex.Message);
            }
        }

        /// <summary>Seuil (en %) du monstre, ou le seuil par défaut.</summary>
        public int GetSeuil(int monsterId) => m_seuils.TryGetValue(monsterId, out var seuil) ? seuil : SpellSelector.HealUrgencyPercent;

        public bool HasCustom(int monsterId) => m_seuils.ContainsKey(monsterId);

        /// <summary>seuil &lt; 0 : retour au seuil par défaut.</summary>
        public void SetSeuil(int monsterId, int seuil)
        {
            if (seuil < 0)
            {
                m_seuils.TryRemove(monsterId, out _);
                Database.Execute("DELETE FROM nekfeu_ia_soins WHERE MonsterId=@0", monsterId);
                return;
            }

            seuil = Math.Min(100, seuil);
            m_seuils[monsterId] = seuil;
            Database.Execute("REPLACE INTO nekfeu_ia_soins (MonsterId, Seuil) VALUES (@0, @1)", monsterId, seuil);
        }
    }
}
