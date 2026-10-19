using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Stump.Server.WorldServer.Database.Quests;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Maps;

namespace Stump.Server.WorldServer.Game.Quests
{
    /// <summary>
    /// MYSTIC 19/10 : les quêtes de donjon (quests.IsDungeonQuest : « Le tour du monde » 2005, 2006, 153… et les quêtes des
    /// donjons de Frigost 619 à 626, 1309, 1310…) démarrent toutes seules quand le personnage arrive sur la carte d'un objectif
    /// de leur première étape (colonne quests_objectives.MapId), s'il remplit leur condition de départ (StartCriterion).
    /// Elles sont enregistrées sur le personnage et avancent ensuite seules (objectifs validés au fil du jeu).
    /// </summary>
    public static class QuetesAuto
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();
        static Dictionary<int, QuestTemplate[]> m_parCarte;
        static readonly object m_verrou = new object();

        static Dictionary<int, QuestTemplate[]> ParCarte()
        {
            if (m_parCarte != null)
                return m_parCarte;

            lock (m_verrou)
            {
                if (m_parCarte != null)
                    return m_parCarte;

                var liste = new List<KeyValuePair<int, QuestTemplate>>();
                foreach (var quete in QuestManager.Instance.GetQuestTemplates().Where(x => x.IsDungeonQuest && x.Steps != null && x.Steps.Length > 0))
                {
                    var premiere = quete.Steps[0];
                    if (premiere.Objectives == null)
                        continue;
                    foreach (var carte in premiere.Objectives.Select(o => o.MapId).Where(m => m != 0).Distinct())
                        liste.Add(new KeyValuePair<int, QuestTemplate>(carte, quete));
                }

                m_parCarte = liste.GroupBy(x => x.Key).ToDictionary(g => g.Key, g => g.Select(x => x.Value).Distinct().ToArray());
                logger.Info($"[Quêtes] {liste.Select(x => x.Value).Distinct().Count()} quêtes de donjon démarrées automatiquement, sur {m_parCarte.Count} cartes");
                return m_parCarte;
            }
        }

        public static void SurArrivee(Character c, Map map)
        {
            try
            {
                if (map == null || !ParCarte().TryGetValue(map.Id, out var quetes))
                    return;

                foreach (var quete in quetes)
                {
                    if (c.Quests.Any(x => x.Template != null && x.Template.Id == quete.Id))
                        continue;

                    if (!string.IsNullOrWhiteSpace(quete.StartCriterionStr) && !quete.StartCriterion.Eval(c))
                        continue;

                    c.StartQuest(quete.Steps[0]);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"[Quêtes] démarrage automatique sur la carte {map?.Id} : {ex.Message}");
            }
        }
    }
}
