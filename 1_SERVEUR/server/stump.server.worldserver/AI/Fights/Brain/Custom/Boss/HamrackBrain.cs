using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.AI.Fights.Actions;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights.Buffs;
using Stump.Server.WorldServer.Game.Fights.Buffs.Customs;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Maps;
using Stump.Server.WorldServer.Game.Spells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeSharp;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom
{
    /// <summary>
    /// Hamrack (scarabée de Ben le Ripate). MYSTIC 17/10 (tofus.fr + demande utilisateur) :
    ///  - priorité absolue : aller au contact de Ben, où qu'il soit, et le « déclencher » (Gouverne ail : Ben vulnérable 5 tours) ;
    ///  - à son 4e tour il « périme » : il se suicide (Gîte) en tuant ce qui est à son contact, sans déclencher Ben.
    /// Avant : il n'allait vers Ben que s'il était déjà à moins de 2 cases, sinon il attaquait les joueurs.
    /// </summary>
    [BrainIdentifier((int)MonsterIdEnum.HAMRACK)]
    public class HamrackBrain : Brain
    {
        public const int GiteSpellId = 682;
        public const int GouverneAilSpellId = 2618;
        public const int TourPeremption = 4;

        int m_tours;

        public HamrackBrain(AIFighter fighter) : base(fighter)
        {
            Fighter.GetAlive += OnGetAlive;
        }

        public override void Play()
        {
            m_tours++;
            var gite = Fighter.Spells.Values.FirstOrDefault(x => x.Id == GiteSpellId);
            var gouverne = Fighter.Spells.Values.FirstOrDefault(x => x.Id == GouverneAilSpellId);

            if (m_tours >= TourPeremption)
            {
                if (gite != null)
                    Fighter.CastAutoSpell(gite, Fighter.Cell);
                return;
            }

            var ben = Fighter.Team.Fighters.FirstOrDefault(x => x.IsAlive() && x is MonsterFighter m && m.Monster.Template.Id == (int)MonsterIdEnum.BEN_LE_RIPATE);
            if (ben == null || gouverne == null)
                return;

            if (!Fighter.Position.Point.GetAdjacentCells().Contains(ben.Position.Point))
            {
                foreach (var resultat in new MoveNearTo(Fighter, ben).Execute(this))
                {
                    if (resultat == RunStatus.Failure)
                        break;
                }
            }

            if (Fighter.Position.Point.GetAdjacentCells().Contains(ben.Position.Point) &&
                Fighter.CanCastSpell(gouverne, ben.Cell) == SpellCastResult.OK)
                Fighter.CastSpell(gouverne, ben.Cell);
        }

        private void OnGetAlive(FightActor fighter)
        {
            if (fighter != Fighter)
                return;

            if (fighter is SummonedMonster && (fighter as SummonedMonster).Monster.MonsterId == 2955) fighter.CastAutoSpell(new Spell(2617, 1), fighter.Cell);
        }
    }
}
