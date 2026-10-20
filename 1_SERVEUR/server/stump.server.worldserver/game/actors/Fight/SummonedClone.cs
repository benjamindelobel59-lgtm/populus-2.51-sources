using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Stats;
using Stump.Server.WorldServer.Game.Maps.Cells;
using System.Collections.Generic;
using System.Linq;
using Spell = Stump.Server.WorldServer.Game.Spells.Spell;

namespace Stump.Server.WorldServer.Game.Actors.Fight
{
    public class SummonedClone : SummonedFighter
    {
        protected readonly StatsFields m_stats;

        public SummonedClone(int id, FightActor caster, Cell cell)
            : base(id, caster.Team, new List<Spell>(), caster, cell)
        {
            Caster = caster;
            Look = caster.Look.Clone();
            m_stats = new StatsFields(this);
            m_stats.InitializeFromStats(caster.Stats);
            if (Caster is CharacterFighter && (Caster as CharacterFighter).Character.BreedId == PlayableBreedEnum.Sram) Stats.Health.DamageTaken = 0;
            ResetUsedPoints();
        }

        public FightActor Caster
        {
            get;
        }

        public override ObjectPosition MapPosition => Position;

        public override string GetMapRunningFighterName() => Name;

        public override ushort Level => Caster.Level;

        public override string Name => NomDouble ?? ((Caster is NamedFighter) ? ((NamedFighter)Caster).Name : "(no name)");

        /// <summary>PADGREF 20/10 : nom affiché du double d'un MONSTRE (ex. « Double de Doublure ») ; null = double d'un joueur.</summary>
        string NomDouble => Caster is MonsterFighter m ? (m.Monster.Template.Id == 3748 ? "Double de Doublure" : "Double") : null;

        public override StatsFields Stats => m_stats;

        public override GameFightFighterInformations GetGameFightFighterInformations(WorldClient client = null)
        {
            var casterInfos = Caster.GetGameFightFighterInformations();

            if (casterInfos is GameFightCharacterInformations)
            {
                var characterInfos = casterInfos as GameFightCharacterInformations;

                return new GameFightCharacterInformations(Id, casterInfos.Look, GetEntityDispositionInformations(), casterInfos.TeamId,
                    0, IsAlive(), GetGameFightMinimalStats(), MovementHistory.GetEntries(2).Select(x => x.Cell.Id).Select(x => (ushort)x).ToArray(),
                    characterInfos.Name, characterInfos.Status, characterInfos.LeagueId, characterInfos.LadderPosition, false, characterInfos.Level, characterInfos.AlignmentInfos, characterInfos.Breed, characterInfos.Sex);
            }

            // PADGREF 20/10 (Reine des Voleurs) : double d'un monstre (Doublage de la Doublure) : avant, informations « nues » que le
            // client affichait « UnknownFighter » comme un combattant à part ; maintenant une invocation nommée (« Double de Doublure »)
            if (NomDouble != null)
                return new GameFightMutantInformations(Id, casterInfos.Look, GetEntityDispositionInformations(), casterInfos.TeamId,
                    0, IsAlive(), GetGameFightMinimalStats(), MovementHistory.GetEntries(2).Select(x => x.Cell.Id).Select(x => (ushort)x).ToArray(),
                    NomDouble, new PlayerStatus(0), 0, 0, false, 0);

            return new GameFightFighterInformations(Id, casterInfos.Look, GetEntityDispositionInformations(), casterInfos.TeamId,
                0, IsAlive(), GetGameFightMinimalStats(), MovementHistory.GetEntries(2).Select(x => x.Cell.Id).Select(x => (ushort)x).ToArray());
        }

        /// <summary>PADGREF 20/10 : le client doit savoir que c'est une INVOCATION (lanceur + « invoqué ») : sinon il la traite
        /// comme un combattant à part entière dans la timeline.</summary>
        public override GameFightMinimalStats GetGameFightMinimalStats(WorldClient client = null)
        {
            var stats = base.GetGameFightMinimalStats(client);
            stats.Summoner = Caster.Id;
            stats.Summoned = true;
            return stats;
        }

        public override FightTeamMemberInformations GetFightTeamMemberInformations() => new FightTeamMemberInformations(Id);
    }
}