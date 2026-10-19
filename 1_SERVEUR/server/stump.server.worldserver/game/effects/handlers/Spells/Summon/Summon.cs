using NLog;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Actors.Fight.Customs;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Fights.Triggers;
using Stump.Server.WorldServer.Handlers.Actions;
using Stump.Server.WorldServer.Game.Spells.Casts;
using Stump.Server.WorldServer.Game.Spells;
using Stump.Server.WorldServer.Game.Fights.Buffs.Customs;
using Stump.Server.WorldServer.Game.Fights.Buffs;
using Stump.Server.WorldServer.Game.Effects.Handlers.Spells.States;
using Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Buffs;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Summon
{
    [EffectHandler(EffectsEnum.Effect_SummonSlave)]
    [EffectHandler(EffectsEnum.Effect_Summon)]
    public class Summon : SpellEffectHandler
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public Summon(EffectDice effect, FightActor caster, SpellCastHandler castHandler, Cell targetedCell, bool critical)
            : base(effect, caster, castHandler, targetedCell, critical)
        {
        }

        public const int FouxDamourSpellId = 2683;

        bool InvoquerTonneaux()
        {
            var alea = new System.Random();
            var centre = new Maps.Cells.MapPoint(TargetedCell);
            var cases = Map.Cells.Where(c => c.Walkable && !c.NonWalkableDuringFight && Fight.GetOneFighter(c) == null)
                .Select(c => new { Cell = c, Distance = centre.ManhattanDistanceTo(new Maps.Cells.MapPoint(c)) })
                .Where(x => x.Distance <= 3).OrderBy(x => x.Distance).ThenBy(x => alea.Next()).Take(3).Select(x => x.Cell).ToList();

            var invoques = 0;
            foreach (var cell in cases)
            {
                var grade = MonsterManager.Instance.GetMonsterGrade((int)MonsterIdEnum.TONNEAU_PIRATE, alea.Next(1, 6));
                if (grade == null)
                    continue;

                var tonneau = new SummonedMonster(Fight.GetNextContextualId(), Caster.Team, Caster, grade, cell) { SummoningEffect = this };
                ActionsHandler.SendGameActionFightSummonMessage(Fight.Clients, tonneau);
                Caster.AddSummon(tonneau);
                Caster.Team.AddFighter(tonneau);
                Fight.TriggerMarks(tonneau.Cell, tonneau, TriggerType.MOVE);
                invoques++;
            }

            return invoques > 0;
        }

        protected override bool InternalApply()
        {
            // MYSTIC 18/10 (audit Dantinéa / Baleine) : les conditions d'état sur le lanceur du masque (*E390…*E397, *e214) n'étaient jamais
            // vérifiées : 8 Grokillages en solo, plusieurs Malamibes
            if (Targets.OfType<Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Targets.StateCriterion>().Any(c => c.Caster && !c.IsTargetValid(Caster, this)))
                return false;

            var monster = MonsterManager.Instance.GetMonsterGrade(Dice.DiceNum, Dice.DiceFace);

            if (monster == null)
            {
                logger.Error("Cannot summon monster {0} grade {1} (not found)", Dice.DiceNum, Dice.DiceFace);
                return false;
            }

            // MYSTIC 17/10 : Tengu Givrefoux, « Foux d'amour » : jamais au hasard, toujours 1 Yokaï puis 1 Yomi (2 invocations max)
            if (Spell.Id == FouxDamourSpellId)
            {
                var vivants = Caster.Summons.Where(x => x.IsAlive()).OfType<SummonedMonster>().Select(x => x.Monster.Template.Id).ToList();
                var voulu = !vivants.Contains((int)MonsterIdEnum.YOKAI_GIVREFOUX) ? (int)MonsterIdEnum.YOKAI_GIVREFOUX :
                            !vivants.Contains((int)MonsterIdEnum.YOMI_GIVREFOUX) ? (int)MonsterIdEnum.YOMI_GIVREFOUX : 0;
                if (voulu == 0)
                    return false;
                monster = MonsterManager.Instance.GetMonsterGrade(voulu, Dice.DiceFace) ?? monster;
            }

            // MYSTIC 18/10 : journal de diagnostic : Wa Wobot (Mékattwaction 3789) qui ne pose pas ses Tamponneurs
            if (Spell.Id == 3789)
                logger.Info("[WaWobot] Mekattwaction : case {0}, occupee par {1}, cases de zone {2}, peut invoquer {3}",
                    TargetedCell?.Id, Fight.GetOneFighter(TargetedCell), AffectedCells.Count, Caster.CanSummon());

            if (monster.Template.UseSummonSlot && !Caster.CanSummon())
                return false;

            // MYSTIC 17/10 : Bonbombe (Péché Mignon de la Reine des Voleurs) : « invoquée malgré lui à 2 PO », toujours sur la case à
            // DROITE du personnage à l'écran (X+1, Y+1 : X+1 descend à droite, Y+1 monte à droite), décalée si la case est prise
            // (capture de l'utilisateur) ; avant : 1re case libre trouvée, placement au hasard
            if (monster.Template.Id == (int)MonsterIdEnum.BONBOMBE)
            {
                var centre = Caster.Position.Point;
                var voulu = new Maps.Cells.MapPoint(centre.X + 1, centre.Y + 1);
                bool Libre(Maps.Cells.MapPoint p) => p.IsInMap() && Map.Cells[p.CellId].Walkable && Fight.GetOneFighter(Map.Cells[p.CellId]) == null;
                Maps.Cells.MapPoint choisi = Libre(voulu) ? voulu : null;
                for (var distance = 2; choisi == null && distance <= 4; distance++)
                    choisi = Map.Cells.Select(c => new Maps.Cells.MapPoint(c))
                        .Where(p => p.ManhattanDistanceTo(centre) == distance && Libre(p))
                        .OrderBy(p => p.ManhattanDistanceTo(voulu)).FirstOrDefault();
                if (choisi == null)
                    return false;
                TargetedCell = Map.Cells[choisi.CellId];
            }

            // MYSTIC 18/10 (retour de test) : Gourlo invoque 1 tonneau par lancer (1 lancer par tour, grade tiré au hasard par les effets à
            // 20 %), au plus 3 en vie (GourloBrain). Avant : 3 tonneaux d'un coup puis plus rien (InvoquerTonneaux n'est plus appelée).

            if (Fight.GetOneFighter(TargetedCell) != null)
            {
                if (AffectedCells.Count <= 1)
                    return false;

                var Cell = AffectedCells.Where(x => Fight.GetOneFighter(x) == null && x.Walkable && x.Id != Caster.Cell.Id).OrderBy(y => Caster.Position.Point.ManhattanDistanceTo(new Maps.Cells.MapPoint(y))).FirstOrDefault();

                if (Cell == null)
                    return false;

                TargetedCell = Cell;
            }

            if (!Caster.HasState(600) && (Spell.Template.Id == 6192 || Spell.Template.Id == 6193 || Spell.Template.Id == 6194 || Spell.Template.Id == 6195))
                return false;

            SummonedFighter summon;
            if (monster.Template.Id == (int)MonsterIdEnum.EPEE_VOLANTE && Caster.HasState(489))
            {
                monster = MonsterManager.Instance.GetMonsterGrade((int)MonsterIdEnum.EPEE_ECORCHEUSE, Dice.DiceFace);
            }
            else if (monster.Template.Id == (int)MonsterIdEnum.EPEE_VOLANTE && Caster.HasState(490))
            {
                monster = MonsterManager.Instance.GetMonsterGrade((int)MonsterIdEnum.EPEE_GARDIENNE, Dice.DiceFace);
            }
            else if (monster.Template.Id == (int)MonsterIdEnum.EPEE_VOLANTE && Caster.HasState(488))
            {
                monster = MonsterManager.Instance.GetMonsterGrade((int)MonsterIdEnum.EPEE_VELOCE, Dice.DiceFace);
            }
            if (monster.Template.Id == (int)MonsterIdEnum.HARPONNEUSE || monster.Template.Id == (int)MonsterIdEnum.GARDIENNE || monster.Template.Id == (int)MonsterIdEnum.TACTIRELLE || monster.Template.Id == (int)MonsterIdEnum.CHALUTIER || monster.Template.Id == (int)MonsterIdEnum.FOREUSE || monster.Template.Id == (int)MonsterIdEnum.BATHYSCAPHE)
                summon = new SummonedTurret(Fight.GetNextContextualId(), Caster, monster, Spell, TargetedCell) {SummoningEffect = this};
            else if (monster.Template.Id == (int)MonsterIdEnum.COFFRE_ANIME)
                summon = new LivingChest(Fight.GetNextContextualId(), Caster.Team, Caster, monster, TargetedCell) { SummoningEffect = this };             
            else
                summon = new SummonedMonster(Fight.GetNextContextualId(), Caster.Team, Caster, monster, TargetedCell) {SummoningEffect = this};

            if (Effect.Id == (short)EffectsEnum.Effect_SummonSlave && Caster is CharacterFighter)
                summon.SetController(Caster as CharacterFighter);

            // MYSTIC : contrôle des invocations (par défaut pour tous, .invo on/off) : le joueur joue ses invocations qui peuvent jouer
            if (!summon.IsControlled() && Caster is CharacterFighter controleur && controleur.Character.ControleInvocations &&
                !(summon is SummonedTurret) && !(summon is LivingChest) && summon.CanPlay() &&
                monster.Template.Id != (int)MonsterIdEnum.BONBOMBE && monster.Template.Id != (int)MonsterIdEnum.MINIKRONE_4812) // MYSTIC : Bonbombe, Minikrone du Razof : jamais jouées par le joueur
                summon.SetController(controleur);


            ActionsHandler.SendGameActionFightSummonMessage(Fight.Clients, summon);

            Caster.AddSummon(summon);
            Caster.Team.AddFighter(summon);

            Fight.TriggerMarks(summon.Cell, summon, TriggerType.MOVE);

            //Control Osa
            switch (Spell.Id)
            {

                case 34: //tofu negro
                    TakeControlSummon(summon);
                    break;
                case 9653: //tofu albino
                    TakeControlSummon(summon);
                    break;
                case 35: // jalato
                    TakeControlSummon(summon);
                    break;
                case 38: //tofuleche
                    TakeControlSummon(summon);
                    break;
                case 40: //jalato negro
                    TakeControlSummon(summon);
                    break;
                case 9661: //jalatin
                    TakeControlSummon(summon);
                    break;
                case 39: // dragonito negro
                    TakeControlSummon(summon);
                    break;
                case 9662: //dragonito albino
                    TakeControlSummon(summon);
                    break;
                case 9664: //sapo negro
                    TakeControlSummon(summon);
                    break;
                case 9658: //sapo albino
                    TakeControlSummon(summon);
                    break;
                case 9667: //sapo baboso
                    TakeControlSummon(summon);
                    break;
                case 31: //dragonito rojo
                    TakeControlSummon(summon);
                    break;
                default:
                    break;
            }

            return true;
        }

        private void TakeControlSummon(SummonedFighter summon)
        {
            TakeControlBuff controlBuff = null;
            StateBuff stateBuff = null;
            SpellImmunityBuff immun = null;
            StatBuff apbuff = null;

            foreach (var item in Caster.GetBuffs())
            {
                if (item.Dice.Value == 432 || item.Dice.Value == 433 || item.Dice.Value == 434 || item.Dice.Value == 599)
                {
                    var id = Caster.PopNextBuffId();
                    controlBuff = new TakeControlBuff(id, Caster, Caster, new TakeControl(item.Spell.CurrentSpellLevel.Effects.Find(x => x.EffectId == EffectsEnum.Effect_TakeControl), Caster, null, summon.Cell, false), item.Spell, FightDispellableEnum.DISPELLABLE_BY_DEATH, summon as SummonedMonster) { Duration = (short)item.Duration };
                    stateBuff = new StateBuff(id + 1, summon, Caster, new AddState(item.Spell.CurrentSpellLevel.Effects.Find(x => x.EffectId == EffectsEnum.Effect_AddState && x.Value != 447), Caster, null, summon.Cell, false), item.Spell, FightDispellableEnum.DISPELLABLE_BY_DEATH, SpellManager.Instance.GetSpellState((uint)item.Dice.Value)) { Duration = (short)item.Duration };
                    immun = new SpellImmunityBuff(id + 2, summon, Caster, new SpellImmunity(item.Spell.CurrentSpellLevel.Effects.Find(x => x.EffectId == EffectsEnum.Effect_SpellImmunity), Caster, null, summon.Cell, false), item.Spell, item.Spell.CurrentSpellLevel.Effects.Find(x => x.EffectId == EffectsEnum.Effect_SpellImmunity).DiceNum, false, FightDispellableEnum.DISPELLABLE_BY_DEATH) { Duration = (short)item.Duration };
                    apbuff = new StatBuff(id + 3, summon, Caster, new APBuff(item.Spell.CurrentSpellLevel.Effects.Find(x => x.EffectId == EffectsEnum.Effect_AddAP_111), Caster, null, summon.Cell, false), item.Spell, 2, PlayerFields.AP, false, FightDispellableEnum.DISPELLABLE_BY_DEATH) { Duration = (short)item.Duration };
                }
            }

            if (controlBuff == null || stateBuff == null)
                return;


            if ((summon as SummonedMonster).Monster.Template.Id == (int)MonsterIdEnum.TOFU_NOIR_4561 ||
                (summon as SummonedMonster).Monster.Template.Id == (int)MonsterIdEnum.TOFU_DODU_4562 ||
                (summon as SummonedMonster).Monster.Template.Id == (int)MonsterIdEnum.TOFU_ALBINOS)

            {
                if (Caster.HasState(432))
                {
                    Caster.AddBuff(controlBuff);
                    summon.AddBuff(stateBuff);
                    summon.AddBuff(immun);
                    summon.AddBuff(apbuff);
                }
            }
            else if ((summon as SummonedMonster).Monster.Template.Id == (int)MonsterIdEnum.BOUFTOU ||
              (summon as SummonedMonster).Monster.Template.Id == (int)MonsterIdEnum.BOUFTOU_NOIR_4564 ||
              (summon as SummonedMonster).Monster.Template.Id == (int)MonsterIdEnum.BOUFTON)

            {
                if (Caster.HasState(433))
                {
                    Caster.AddBuff(controlBuff);
                    summon.AddBuff(stateBuff);
                    summon.AddBuff(immun);
                    summon.AddBuff(apbuff);
                }
            }
            else if ((summon as SummonedMonster).Monster.Template.Id == (int)MonsterIdEnum.DRAGONNET_ROUGE_4565 ||
              (summon as SummonedMonster).Monster.Template.Id == (int)MonsterIdEnum.DRAGONNET_NOIR ||
              (summon as SummonedMonster).Monster.Template.Id == (int)MonsterIdEnum.DRAGONNET_ALBINOS)
            {
                if (Caster.HasState(434))
                {
                    Caster.AddBuff(controlBuff);
                    summon.AddBuff(stateBuff);
                    summon.AddBuff(immun);
                    summon.AddBuff(apbuff);
                }
            }
            else if ((summon as SummonedMonster).Monster.Template.Id == (int)MonsterIdEnum.CRAPAUD_NOIR ||
              (summon as SummonedMonster).Monster.Template.Id == (int)MonsterIdEnum.CRAPAUD_ALBINOS ||
              (summon as SummonedMonster).Monster.Template.Id == (int)MonsterIdEnum.CRAPAUD_BAVEUX)
            {
                if (Caster.HasState(599))
                {
                    Caster.AddBuff(controlBuff);
                    summon.AddBuff(stateBuff);
                    summon.AddBuff(immun);
                    summon.AddBuff(apbuff);
                }
            }
        }
    }
}