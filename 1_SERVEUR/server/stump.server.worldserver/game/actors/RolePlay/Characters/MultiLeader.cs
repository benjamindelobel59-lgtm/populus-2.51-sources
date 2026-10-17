using System.Collections.Generic;
using System.Linq;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Maps;

namespace Stump.Server.WorldServer.Game.Actors.RolePlay.Characters
{
    /// <summary>
    /// Leader of the characters connected from the same IP (.master / .onewindow).
    /// Out of fight the other characters follow the leader map by map (not cell by cell),
    /// in fight they join the leader's fight and copy its ready status.
    /// </summary>
    public static class MultiLeader
    {
        // NEKFEU : map que le meneur vient de quitter (seuls les personnages qui y etaient le suivent)
        static readonly Dictionary<int, int> m_previousMap = new Dictionary<int, int>();

        public static bool IsLeader(Character character) => character.isMultiLeadder;

        public static void Enable(Character leader)
        {
            if (leader.isMultiLeadder)
                return;

            // a single leader per IP, otherwise two characters would pull each other
            foreach (var other in GetFollowers(leader))
                Disable(other);

            leader.isMultiLeadder = true;
            leader.EnterMap += OnEnterMap;
            leader.LeaveMap += OnLeaveMap;
            leader.EnterFight += OnEnterFight;
            leader.ReadyStatusChanged += OnReadyStatusChanged;

            // MYSTIC 17/10 : plus de teleportation a l'activation (avant : les personnages de la map etaient poses sur la case du
            // meneur, meme de l'autre cote d'un obstacle qu'il fallait contourner). Ils suivent au prochain changement de map.
        }

        public static void Disable(Character leader)
        {
            if (!leader.isMultiLeadder)
                return;

            leader.isMultiLeadder = false;
            leader.EnterMap -= OnEnterMap;
            leader.LeaveMap -= OnLeaveMap;
            leader.EnterFight -= OnEnterFight;
            m_previousMap.Remove(leader.Id);
            leader.ReadyStatusChanged -= OnReadyStatusChanged;
        }

        static IEnumerable<Character> GetFollowers(Character leader) =>
            WorldServer.Instance.FindClients(x => x.IP == leader.Client.IP && x.Character != null && x.Character != leader)
                .Select(x => x.Character);

        static void BringFollowers(Character leader, Map map)
        {
            if (map == null)
                return;

            // NEKFEU : seuls suivent les personnages qui etaient sur la map que le meneur vient de quitter
            if (!m_previousMap.TryGetValue(leader.Id, out var previousMapId))
                return;

            // Donjon : on ne fait jamais entrer quelqu'un sans clé ; on ne suit de salle en salle
            // que les personnages déjà dans le même donjon (même sous-zone). Correctif Nekfeu.
            var dungeon = map.IsDungeon();

            foreach (var follower in GetFollowers(leader).Where(x => !x.IsFighting() && x.Map != map && x.Map != null && x.Map.Id == previousMapId))
            {
                if (dungeon && (follower.Map == null || follower.Map.SubArea != map.SubArea))
                    continue;

                follower.Teleport(map, leader.Cell);
            }
        }

        static void OnLeaveMap(RolePlayActor actor, Map map)
        {
            if (actor is Character leader && map != null)
                m_previousMap[leader.Id] = map.Id;
        }

        static void OnEnterMap(RolePlayActor actor, Map map)
        {
            var leader = actor as Character;
            if (leader == null || leader.IsFighting())
                return;

            BringFollowers(leader, map);
        }

        static void OnEnterFight(CharacterFighter fighter)
        {
            // Correctif Nekfeu : join auto aussi en donjon (seulement les persos déjà sur la même map)
            if (fighter == null)
                return;

            foreach (var follower in GetFollowers(fighter.Character).Where(x => x.Map.Id == fighter.Map.Id && !x.IsFighting()))
            {
                // plus de place de placement (salles de donjon) : on n'essaie pas, sinon le perso resterait à moitié en combat
                if (fighter.Team.IsFull())
                    break;

                var followerFighter = follower.CreateFighter(fighter.Team);
                fighter.Team.AddFighter(followerFighter);
            }
        }

        static void OnReadyStatusChanged(CharacterFighter fighter)
        {
            if (fighter == null)
                return;

            foreach (var follower in GetFollowers(fighter.Character).Where(x => x.IsInFight() && x.Map.Id == fighter.Map.Id))
                follower.Fighter.ToggleReady(fighter.IsReady);
        }
    }
}
