using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Buffs;
using Stump.Server.WorldServer.Game.Fights.Triggers;
using Stump.Server.WorldServer.Game.Spells;
using Stump.Server.WorldServer.Game.Spells.Casts;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Others
{
    [EffectHandler(EffectsEnum.Effect_TriggerBuff)]
    [EffectHandler(EffectsEnum.Effect_TriggerBuff_793)]
    [EffectHandler(EffectsEnum.Effect_CastSpell_1160)]
    [EffectHandler(EffectsEnum.Effect_CastSpell_1017)]
    [EffectHandler(EffectsEnum.Effect_CastSpell_2160)]
    [EffectHandler(EffectsEnum.Effect_CastSpell_1175)]
    [EffectHandler(EffectsEnum.Effect_2792)]
    [EffectHandler(EffectsEnum.Effect_2794)]
    [EffectHandler(EffectsEnum.Effect_1018)]
    [EffectHandler(EffectsEnum.Effect_1019)] // MYSTIC 17/10 : lance un sort (Aquadôme, Comte…), comme 792
    public class CastSpellEffect : SpellEffectHandler
    {
        public CastSpellEffect(EffectDice effect, FightActor caster, SpellCastHandler castHandler, Cell targetedCell, bool critical)
            : base(effect, caster, castHandler, targetedCell, critical)
        {
        }

        protected override bool InternalApply()
        {
            // MYSTIC 18/10 : sous-sort inexistant (ex. Tourbillon mortel de l'Arapex → 5612, absent du 2.51) : le constructeur de Spell
            // plantait (NullReference) et coupait le sort entier
            if (SpellManager.Instance.GetSpellTemplate(Dice.DiceNum) == null)
                return false;

            var spelll = new Spell(Dice.DiceNum, (byte)Dice.DiceFace);

            if(spelll.Id == 4840)
            {
                this.Caster.CastAutoSpell(spelll, this.Caster.Cell);
                return true;
            }

            var cibles = GetAffectedActors();
            if (Effect.EffectId == EffectsEnum.Effect_CastSpell_2160 && SortsPlusProche.Contains(Spell.Id))
            {
                // MYSTIC 18/10 : Chouque. 2160 = « lance le sort sur les N cibles les plus proches de la case visée » (valeur N, 1 par défaut) ;
                // avant : toutes les cibles de la zone (Malédiction Pirate sur tous les joueurs, tous les pirates à +200 puissance).
                var n = Dice.Value > 0 ? Dice.Value : 1;
                if (Spell.Id == 6144)
                {
                    // MYSTIC 18/10 (retour de test) : Téléportation Spectrale = échange avec un ALLIÉ du Chouque (le plus proche de l'ennemi
                    // visé) ; impossible si le Chouque ou l'allié est en Pesanteur (avant : on testait la Pesanteur du joueur visé)
                    if (Caster.HasState((int)SpellStatesEnum.PESANTEUR_7))
                        return true;
                    cibles = cibles.Where(x => !x.HasState((int)SpellStatesEnum.PESANTEUR_7) && x.CanSwitchPos() && !x.IsCarried());
                }
                cibles = cibles.OrderBy(x => x.Position.Point.ManhattanDistanceTo(TargetedPoint)).ThenBy(x => AngleHoraire(TargetedPoint, x.Position.Point)).Take(n).ToArray();
            }

            foreach (var affectedActor in cibles)
            {
                if (Dice.Duration != 0 || Dice.Delay != 0)
                {
                    var buffId = affectedActor.PopNextBuffId();

                    var spell = new Spell(Dice.DiceNum, (byte)Dice.DiceFace);

                    // MYSTIC 17/10 : Kete du Korriandre : les joueurs continuent de poser leurs glyphes apres la mort du Korriandre (officiel)
                    var dispellable = Spell.Id == (int)SpellIdEnum.KWEST ? FightDispellableEnum.REALLY_NOT_DISPELLABLE : FightDispellableEnum.DISPELLABLE_BY_DEATH;
                    var buff = new TriggerBuff(buffId, affectedActor, Caster, this, spell, Spell, false, dispellable, Priority, DefaultBuffTrigger);
                    affectedActor.AddBuff(buff);
                }
                else
                {
                    var spell = new Spell(Dice.DiceNum, (byte)Dice.DiceFace);

                    if (Effect.EffectId == EffectsEnum.Effect_CastSpell_1160 || Effect.EffectId == EffectsEnum.Effect_CastSpell_2160)
                    {
                        if (Spell.Id == (int)SpellIdEnum.ABYSSAL_DOFUS)
                        {
                            var ignored = new[]
                                {
                                    SpellCastResult.CANNOT_PLAY,
                                    SpellCastResult.CELL_NOT_FREE,
                                    SpellCastResult.HAS_NOT_SPELL,
                                    SpellCastResult.HISTORY_ERROR,
                                    SpellCastResult.NOT_ENOUGH_AP,
                                    SpellCastResult.NOT_IN_ZONE,
                                    SpellCastResult.STATE_FORBIDDEN,
                                    SpellCastResult.STATE_REQUIRED,
                                    SpellCastResult.UNWALKABLE_CELL
                                };

                            Caster.CastSpell(new SpellCastInformations(Caster, spell, affectedActor.Cell)
                                {
                                    Silent = true,
                                    ApFree = true,
                                    BypassedConditions = ignored
                                });
                        }
                        else
                        {
                            Caster.CastAutoSpell(spell, affectedActor.Cell);
                        }
                    }
                    else if (Effect.EffectId == EffectsEnum.Effect_CastSpell_1017)
                    {
                        affectedActor.CastAutoSpell(spell, Caster.Cell);
                    }
                    else if(Effect.EffectId == EffectsEnum.Effect_2794)
                    {
                        // MYSTIC 18/10 : Garde-bouclier du Mantiscore : -50 % résistances + Dérèglement seulement s'il n'a pas pu se
                        // téléporter (s'il a bougé, il n'est plus sur la case du sort)
                        if (Spell.Id == 7223 && Caster.Cell != null && TargetedCell != null && Caster.Cell.Id != TargetedCell.Id)
                            continue;
                        affectedActor.CastAutoSpell(spell, TargetedCell);
                    }
                    else
                    {
                        affectedActor.CastAutoSpell(spell, affectedActor.Cell);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// MYSTIC 19/10 (audit Antre du Mantiscore) : frappé à distance, le Mantiscore se téléporte au contact de l'attaquant (case libre
        /// la plus proche de lui) ; s'il ne peut pas, il prend le niveau 3 de Garde-bouclier (-50 % résistances, +200 % dommages,
        /// Déréglé 2 tours). Déréglé (état 237) : plus de téléportation. Avant : le sort visait la propre case du Mantiscore, il ne se
        /// téléportait jamais et se mettait à -50 % / Déréglé à chaque coup à distance.
        /// </summary>
        static void GardeBouclier(MonsterFighter manti, FightActor attaquant)
        {
            if (attaquant == null || !manti.IsAlive() || !attaquant.IsAlive() || attaquant.Team == manti.Team || manti.HasState(237))
                return;
            var fight = manti.Fight;
            if (manti.Position.Point.IsAdjacentTo(attaquant.Position.Point))
                return;
            var cible = attaquant.Position.Point.GetAdjacentCells()
                .Select(p => fight.Map.Cells[p.CellId])
                .Where(c => c.Walkable && !c.NonWalkableDuringFight && fight.IsCellFree(c))
                .OrderBy(c => new Maps.Cells.MapPoint(c).ManhattanDistanceTo(manti.Position.Point))
                .FirstOrDefault();
            if (cible != null)
            {
                manti.Position.Cell = cible;
                Stump.Server.WorldServer.Handlers.Actions.ActionsHandler.SendGameActionFightTeleportOnSameMapMessage(fight.Clients, manti, manti, cible);
                fight.TriggerMarks(cible, manti, TriggerType.MOVE);
                return;
            }
            manti.CastSpell(new SpellCastInformations(manti, new Spell(7223, 3), manti.Cell)
            {
                Force = true,
                Silent = true,
                ApFree = true
            });
        }

        static void InvoquerMulou(FightActor mort)
        {
            var fight = mort.Fight;
            var meulou = fight.GetAllFighters<MonsterFighter>(x => x.IsAlive() && x.Monster.Template.Id == 232 && x.Team != mort.Team).FirstOrDefault();
            var grade = Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters.MonsterManager.Instance.GetMonsterGrade(102, 5);
            var cell = mort.Cell;
            if (meulou == null || grade == null || cell == null || fight.GetOneFighter(cell) != null)
                return;

            var mulou = new Stump.Server.WorldServer.Game.Actors.Fight.SummonedMonster(fight.GetNextContextualId(), meulou.Team, meulou, grade, cell);
            Stump.Server.WorldServer.Handlers.Actions.ActionsHandler.SendGameActionFightSummonMessage(fight.Clients, mulou);
            meulou.AddSummon(mulou);
            meulou.Team.AddFighter(mulou);
            fight.TriggerMarks(mulou.Cell, mulou, TriggerType.MOVE);
        }

        // MYSTIC 18/10 : Comte Harebourg (décision utilisateur) : tour de jeu IMPAIR = Fuseau (le lanceur qui frappe le Comte est
        // téléporté au symétrique), tour PAIR = Gousset (le Comte se téléporte au symétrique de l'attaquant ; échange avec un
        // allié / une invocation = Comte vulnérable). Avant : l'alternance des passifs était décalée d'un tour (Gousset au tour 1).
        // Une seule téléportation par sort lancé : un sort à plusieurs lignes de dégâts téléportait deux fois et ramenait tout le
        // monde à sa case de départ (« ne se téléporte pas tout le temps »).
        const int Fuseau = 3649, Gousset = 3657;

        // MYSTIC 18/10 : 2160 = « la ou les N cibles les plus proches » (Chouque ; Tal Kasha : Maudification 8049, Filature 7907,
        // Transe-Perse 7909) ; égalité : la première dans le sens horaire en partant de midi
        static readonly int[] SortsPlusProche = { 6144, 6145, 8049, 7907, 7909 };

        static double AngleHoraire(Maps.Cells.MapPoint c, Maps.Cells.MapPoint p)
        {
            double ex = (p.X - c.X) + (p.Y - c.Y), ey = (p.X - c.X) - (p.Y - c.Y);
            return (System.Math.Atan2(ex, -ey) + 2 * System.Math.PI) % (2 * System.Math.PI);
        }

        static readonly System.Runtime.CompilerServices.ConditionalWeakTable<FightActor, int[]> s_dernierSortComte =
            new System.Runtime.CompilerServices.ConditionalWeakTable<FightActor, int[]>();

        void DeclencherAlternanceComte(FightActor comte, FightActor attaquant)
        {
            if (attaquant == null || attaquant == comte || attaquant.Team == comte.Team || !comte.IsAlive())
                return;

            var dernier = s_dernierSortComte.GetValue(attaquant, _ => new[] { -1 });
            if (dernier[0] == attaquant.NumeroDernierSort)
                return;
            dernier[0] = attaquant.NumeroDernierSort;

            var impair = comte.Fight.TimeLine.RoundNumber % 2 == 1;
            var sort = new Spell(impair ? Fuseau : Gousset, 1);
            comte.CastSpell(new SpellCastInformations(impair ? comte : attaquant, sort, comte.Cell)
            {
                Force = true,
                Silent = true,
                ApFree = true,
                TriggerEffect = this,
                Triggerer = attaquant
            });
        }

        void DefaultBuffTrigger(TriggerBuff buff, FightActor triggerrer, BuffTriggerType trigger, object token)
        {
            // MYSTIC 18/10 : Mulanthropie (Milimeulou 8657 → 8658) : à la mort de la cible, le Meulou invoque à sa place un Mulou
            // (8658 niv. 2 = 405 Mulou 102 grade 5). La chaîne officielle passe par la cible morte, que le moteur exclut des effets :
            // le Mulou n'apparaissait jamais.
            if (buff.Spell.Id == 8658 && trigger == BuffTriggerType.OnDeath)
            {
                InvoquerMulou(buff.Target);
                return;
            }

            // MYSTIC 19/10 : Garde-bouclier du Mantiscore (7223 niv. 2, déclencheur DR) codé : voir GardeBouclier
            if (buff.Spell.Id == 7223 && buff.Target is MonsterFighter manti && manti.Monster.Template.Id == 4621)
            {
                GardeBouclier(manti, triggerrer);
                return;
            }

            var damages = token as Fights.Damage;

            if ((buff.Spell.Id == Fuseau || buff.Spell.Id == Gousset) && buff.Target is MonsterFighter comte && comte.Monster.Template.Id == 3416)
            {
                DeclencherAlternanceComte(comte, triggerrer);
                return;
            }

            // MYSTIC 18/10 (décision utilisateur) : Haute Truche : Truchage (4344, Struchthio) et Sdeurthio (4394 → 4375) seulement sur
            // un coup d'ARME au corps à corps (le déclencheur DCAC partait aussi sur les sorts ; Glourséleste inchangé)
            if ((buff.Spell.Id == 4344 || (buff.Spell.Id == 4375 && Spell.Id == 4394)) && !(damages != null && damages.IsWeaponAttack))
                return;

            if (damages != null && damages.Spell != null && damages.Spell.Id == buff.Spell.Id)
                return;

            // MYSTIC 18/10 (audit Anerice) : Marche des goules codée (la chaîne 2.51 ne déplaçait personne)
            if (buff.Spell.Id == MarcheDesGoules.Sort && trigger == BuffTriggerType.OnDamaged)
            {
                MarcheDesGoules.Declencher(buff.Target, triggerrer);
                return;
            }

            // MYSTIC 18/10 (décision utilisateur, comme le site) : pas de Mer Cenaire (mort des joueurs collés aux monstres, Friture)
            if (buff.Spell.Id == 4006)
                return;

            // MYSTIC 18/10 (retour de test, site) : Mer Veille (Merkator) : le déclencheur MPA est la cible ; c'est celui qui retire les
            // PM (le combattant qui joue) qui subit le vol de vie
            if (buff.Spell.Id == 4008)
            {
                var voleur = buff.Target.Fight.FighterPlaying;
                if (voleur == null || !voleur.IsAlive() || voleur.Team == buff.Target.Team)
                    return;
                triggerrer = voleur;
            }

            // MYSTIC 18/10 (site) : Mer Cure posée sur un joueur ne part que si un ALLIÉ le frappe avec une arme (côté monstres inchangé)
            if (buff.Spell.Id == 4007 && buff.Target.Team != buff.Caster.Team &&
                !(damages != null && damages.IsWeaponAttack && damages.Source != null && damages.Source != buff.Target && damages.Source.Team == buff.Target.Team))
                return;

            // MYSTIC 18/10 : journal du déblocage du Mansot Royal (joueur soigné, vol de vie compris : le Mansot doit être au contact)
            if (buff.Spell.Id == 2608)
                NLog.LogManager.GetCurrentClassLogger().Info("[Mansot] {0} soigne (source {1}) : Mansot au contact = {2}", buff.Target, triggerrer,
                    buff.Target.Fight.GetAllFighters(x => x.IsAlive() && x is MonsterFighter m && m.Monster.Template.Id == 2848 && x.Position.Point.IsAdjacentTo(buff.Target.Position.Point)).Any());

            // MYSTIC 18/10 : Fulguration du Yomi Givrefoux : vrai coup d'arme seulement (sort 0), jamais poussée / renvoi / dommages directs
            if (buff.Spell.Id == 2670 && !(damages != null && damages.Spell != null && damages.Spell.Id == 0 && damages.School != EffectSchoolEnum.Pushback))
                return;

            if (Effect.EffectId == EffectsEnum.Effect_CastSpell_1160)
            {
                if(buff.Spell.Id == 9917)
                {
                    buff.Target.CastSpell(new SpellCastInformations(buff.Target, buff.Spell, buff.Target.Cell)
                    {
                        Force = true,
                        Silent = true,
                        ApFree = true,
                        TriggerEffect = this,
                        Triggerer = triggerrer
                    });
                }
                else
                {
                    buff.Caster.CastSpell(new SpellCastInformations(buff.Caster, buff.Spell, buff.Target.Cell)
                    {
                        Force = true,
                        Silent = true,
                        ApFree = true,
                        TriggerEffect = this,
                        Triggerer = triggerrer
                    });
                }
            }
            else if (Effect.EffectId == EffectsEnum.Effect_CastSpell_1017 ||
                buff.Spell.Id == (int)SpellIdEnum.FRIKT)
            {
                buff.Target.CastSpell(new SpellCastInformations(buff.Target, buff.Spell, triggerrer.Cell)
                {
                    Force = true,
                    Silent = true,
                    ApFree = true,
                    TriggerEffect = this,
                    Triggerer = triggerrer
                });
            }
            else if (Effect.EffectId == EffectsEnum.Effect_1018)
            {
                buff.Target.CastSpell(new SpellCastInformations(triggerrer, buff.Spell, buff.Target.Cell)
                {
                    Force = true,
                    Silent = true,
                    ApFree = true,
                    TriggerEffect = this,
                    Triggerer = triggerrer
                });
            }
            else if (Effect.EffectId == EffectsEnum.Effect_2794 && (Spell.Id == 10027 || Spell.Id == 10020 || Spell.Id == 10023 || Spell.Id == 10035))
            {
                var cellToCast = buff.Target.Fight.GetTriggers().Where(x => x is Rune && x.Caster == buff.Target && x.OriginEffect.SpellId == Spell.Id).FirstOrDefault().CenterCell;
                if (cellToCast != null)
                {
                    buff.Target.CastSpell(new SpellCastInformations(buff.Target, buff.Spell, cellToCast)
                    {
                        Force = true,
                        Silent = true,
                        ApFree = true,
                        TriggerEffect = this,
                        Triggerer = triggerrer
                    });
                }
                else
                {
                    return;
                }
            }
            else
            {
                if (buff.Spell.Id == (int)SpellIdEnum.BEARBARKENTINE)
                {
                    buff.Target.CastSpell(new SpellCastInformations(buff.Target, buff.Spell, buff.Caster.Cell)
                    {
                        Force = true,
                        Silent = true,
                        ApFree = true,
                        TriggerEffect = this,
                        Triggerer = triggerrer
                    });
                }
                else
                {
                    buff.Target.CastSpell(new SpellCastInformations(buff.Target, buff.Spell, buff.Target.Cell)
                    {
                        Force = true,
                        Silent = true,
                        ApFree = true,
                        TriggerEffect = this,
                        Triggerer = triggerrer
                    });
                }
            }
        }
    }
    /// <summary>
    /// MYSTIC 18/10 (audit Anerice, site) : Marche des goules (8726) : une fois par tour et par monstre frappé, les autres monstres avancent
    /// d'une case vers l'attaquant ; le compteur repart au tour du monstre frappé. La chaîne 2.51 ne déplaçait personne.
    /// </summary>
    public static class MarcheDesGoules
    {
        public const int Sort = 8726, SortAvance = 8890;

        class Etat
        {
            public bool Declenche, Abonne;
        }

        static readonly System.Runtime.CompilerServices.ConditionalWeakTable<FightActor, Etat> s_etats =
            new System.Runtime.CompilerServices.ConditionalWeakTable<FightActor, Etat>();

        public static void Declencher(FightActor frappe, FightActor attaquant)
        {
            if (frappe == null || attaquant == null || !frappe.IsAlive() || !attaquant.IsAlive() || attaquant.Team == frappe.Team)
                return;
            var etat = s_etats.GetValue(frappe, _ => new Etat());
            if (!etat.Abonne)
            {
                etat.Abonne = true;
                frappe.Fight.TurnStarted += (f, joueur) => { if (joueur == frappe) etat.Declenche = false; };
            }
            if (etat.Declenche)
                return;
            etat.Declenche = true;
            foreach (var allie in frappe.Team.Fighters.Where(x => x != frappe && x.IsAlive() && x is MonsterFighter).ToArray())
            {
                if (allie.Position.Point.ManhattanDistanceTo(attaquant.Position.Point) <= 1)
                    continue;
                var cast = new DefaultSpellCastHandler(new SpellCastInformations(allie, new Spell(SortAvance, 1), attaquant.Cell));
                var h = EffectManager.Instance.GetSpellEffectHandler(new EffectDice(EffectsEnum.Effect_Advance, 0, 1, 0), allie, cast, attaquant.Cell, false);
                h.SetAffectedActors(new[] { attaquant });
                h.Apply();
            }
        }
    }
}
