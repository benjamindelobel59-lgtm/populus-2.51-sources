using System;
using System.Collections.Generic;
using NLog;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Messages;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Results;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Handlers.Context;

namespace Stump.Server.WorldServer.Game.Jobs
{
    /// <summary>
    /// MYSTIC 19/10 : collecteurs des métiers de récolte : à la fin d'une récolte, 1 chance sur 100 de devoir affronter le protecteur
    /// officiel de LA RESSOURCE récoltée (Épouvantails, Craqueboules, arbres Ensorcelés, Dévoreurs, Bulb… ; CollecteursTable),
    /// grade 1 ramené à 100 PV. Victoire = le sac de cette ressource en butin. Aucun message n'annonce la chance.
    /// </summary>
    public static class Collecteurs
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();
        const int Chance = 100;          // 1 sur 100
        const int PointsDeVie = 100;
        static readonly Random m_random = new Random();
        static readonly HashSet<int> m_enCombat = new HashSet<int>();

        public static void Tenter(Character c, int ressource)
        {
            try
            {
                if (!CollecteursTable.Ressources.TryGetValue(ressource, out var info))
                    return;
                lock (m_random)
                    if (m_random.Next(Chance) != 0)
                        return;
                if (c.IsFighting() || c.IsBusy() || c.Map == null)
                    return;
                var grade = MonsterManager.Instance.GetMonsterGrade(info.Monstre, 1);
                if (grade == null)
                    return;
                lock (m_enCombat)
                    if (!m_enCombat.Add(c.Id))
                        return;

                var fight = FightManager.Instance.CreatePvMFight(c.Map);
                fight.ChallengersTeam.AddFighter(c.CreateFighter(fight.ChallengersTeam));
                var groupe = new MonsterGroup(0, new ObjectPosition(c.Map, c.Cell, (DirectionsEnum)3));
                var monstre = new MonsterFighter(fight.DefendersTeam, new Monster(grade, groupe));
                monstre.Stats.Health.Base = PointsDeVie;
                monstre.Stats.Health.DamageTaken = 0;
                fight.DefendersTeam.AddFighter(monstre);

                fight.ResultsGenerated += (f, resultats) =>
                {
                    if (info.Sac <= 0 || f.Winners == null || f.Winners != f.ChallengersTeam)
                        return;
                    foreach (var r in resultats)
                        if (r is FightPlayerResult p && p.Character == c)
                            p.Loot.AddItem(new DroppedItem(info.Sac, 1));
                };
                fight.FightEnded += f =>
                {
                    lock (m_enCombat)
                        m_enCombat.Remove(c.Id);
                };

                fight.StartPlacement();
                ContextHandler.HandleGameFightJoinRequestMessage(c.Client, new GameFightJoinRequestMessage(c.Fighter.Id, (ushort)fight.Id));
                logger.Info($"[Collecteurs] {c.Name} affronte le protecteur {info.Monstre} (ressource {ressource}, sac {info.Sac})");
            }
            catch (Exception ex)
            {
                lock (m_enCombat)
                    m_enCombat.Remove(c.Id);
                logger.Error($"[Collecteurs] {c?.Name} : {ex}");
            }
        }
    }
}
