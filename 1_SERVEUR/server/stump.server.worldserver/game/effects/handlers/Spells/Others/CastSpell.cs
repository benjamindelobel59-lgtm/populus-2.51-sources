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
                    // PADGREF 23/10 : PAS de dédoublonnage des déclencheurs passifs identiques posés par plusieurs monstres (essayé pour
                    // Aquazomb 9451 chez Bethel) : chaque monstre doit garder le sien, c'est lui qui se lance son propre bonus (« les mobs
                    // ne font plus leur boost »).
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
                        // PADGREF 22/10 : Défense des Rois d'Ilyzaelle : la case 437 est d'abord libérée (occupant poussé à côté)
                        if (spell.Id == AI.Fights.Brain.Custom.Boss.IlyzaelleBrain.DefenseDesRois)
                            AI.Fights.Brain.Custom.Boss.IlyzaelleBrain.LibererCase(affectedActor);
                        affectedActor.CastAutoSpell(spell, affectedActor.Cell);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// MYSTIC 19/10 (audit Antre du Mantiscore) : frappé à distance, le Mantiscore se téléporte sur une de ses cases adjacentes (la
        /// plus loin de l'attaquant, PADGREF 23/10) ; s'il ne peut pas, il prend le niveau 3 de Garde-bouclier (-50 % résistances, +200 % dommages,
        /// Déréglé 2 tours). Déréglé (état 237) : plus de téléportation. Avant : le sort visait la propre case du Mantiscore, il ne se
        /// téléportait jamais et se mettait à -50 % / Déréglé à chaque coup à distance.
        /// </summary>
        static void GardeBouclier(MonsterFighter manti, FightActor attaquant)
        {
            if (attaquant == null || !manti.IsAlive() || !attaquant.IsAlive() || attaquant.Team == manti.Team || manti.HasState(237))
                return;
            var fight = manti.Fight;
            // PADGREF 23/10 (retour utilisateur : « quand on le frappe il se rapproche du lanceur au lieu de s'éloigner ») : règle officielle,
            // il se téléporte sur UNE DE SES cases adjacentes (avant : au contact de l'attaquant) : la case libre la plus loin de l'attaquant
            var cible = manti.Position.Point.GetAdjacentCells()
                .Select(p => fight.Map.Cells[p.CellId])
                .Where(c => c.Walkable && !c.NonWalkableDuringFight && fight.IsCellFree(c))
                .OrderByDescending(c => new Maps.Cells.MapPoint(c).ManhattanDistanceTo(attaquant.Position.Point))
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
        const int Fuseau = 3649, Gousset = 3657, Interets = 4504;

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
            // PADGREF 23/10 (retour utilisateur : « les tours impairs doivent téléporter le lanceur, ça ne le fait pas ») : Fuseau vise la
            // case de l'ATTAQUANT (zone 1 case, masque ennemi) ; lancé sur la case du Comte, il ne touchait personne
            comte.CastSpell(new SpellCastInformations(impair ? comte : attaquant, sort, impair ? attaquant.Cell : comte.Cell)
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

            // PADGREF 21/10 : Poison incurable du Scoliopode (4736) : vise l'attaquant via CibleDeclencheur (LastAttackerCriterion.cs)

            // PADGREF 21/10 : Carapace gardée de la Lucrane (4746 -> 4745 -> 4744) : codée dans LucraneBrain (le renvoi touchait la Lucrane)
            if (buff.Spell.Id == 4745 || buff.Spell.Id == 4744)
                return;

            var damages = token as Fights.Damage;

            // MYSTIC 20/10 : Protozorreur (Ventre de la Baleine) : voir la classe Protozorreur
            if (buff.Spell.Id == Protozorreur.Attraction && buff.Target is MonsterFighter proto && proto.Monster.Template.Id == Protozorreur.Monstre)
            {
                Protozorreur.Attirer(proto, triggerrer);
                return;
            }
            if (buff.Spell.Id == Protozorreur.Glyphe && !Protozorreur.PeutPoserGlyphe(buff.Target, damages))
                return;
            if (buff.Spell.Id == Dislocation.Sort)
            {
                Dislocation.Intercepter(buff.Target, damages);
                return;
            }

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

            // PADGREF 21/10 (retour utilisateur, Missiz Frizz) : Monogel (11943, Congère, déclencheur « P » = poussé) part seulement
            // quand un JOUEUR (ou son piège / son invocation) pousse ; les poussées des monstres (Braguette / Bralong du Verglasseur)
            // ne lancent plus Monogel
            if (buff.Spell.Id == 11943 && trigger == BuffTriggerType.OnPushed && triggerrer != null && buff.Caster != null &&
                triggerrer.Team == buff.Caster.Team)
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

            // PADGREF 22/10 (audit Belvédère d'Ilyzaelle) : Brûle-âme (9079, déclencheurs M|MPA|EON8) : pour un retrait de PM tenté ou un
            // monstre porté, le moteur donne le monstre lui-même comme déclencheur : c'est le joueur qui joue qui prend la Brûlure
            if (buff.Spell.Id == 9079 && (triggerrer == null || triggerrer == buff.Target))
            {
                var joueur = buff.Target.Fight.FighterPlaying;
                if (joueur == null || !joueur.IsAlive() || joueur.Team == buff.Target.Team)
                    return;
                triggerrer = joueur;
            }

            // PADGREF 22/10 : Éradication d'Ilyzaelle (2e palier) codée : les monstres puis elle (voir IlyzaelleBrain.Eradiquer)
            if (buff.Spell.Id == AI.Fights.Brain.Custom.Boss.IlyzaelleBrain.Eradication && buff.Target is MonsterFighter ily &&
                ily.Monster.Template.Id == (int)MonsterIdEnum.ILYZAELLE)
            {
                AI.Fights.Brain.Custom.Boss.IlyzaelleBrain.Eradiquer(ily);
                return;
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

            // MYSTIC 20/10 (Palais du Roi Nidas) : changement de camp de la Boubourse et Cadeau Empoisonné codés (BoubourseNidas, NidasBrains.cs) :
            // les chaînes 2.51 (Changement de camp 4512 lancé par la Boubourse déjà morte, retour 4483, Cadeau 4481 du passif) sont ignorées
            if (buff.Spell.Id == 4512 || buff.Spell.Id == 4483 || (buff.Spell.Id == 4481 && buff.ParentSpell?.Id == 4482))
                return;

            // MYSTIC 20/10 (Palais du Roi Nidas, décision utilisateur) : Reflets placides (Lévitrof) : seul le joueur qui porte l'état vert
            // (Reflets placides 4521 sur lui) échange sa place avec le Lévitrof en le frappant (et perd l'état) ; les autres coups ne font rien
            if (buff.Spell.Id == 4520 && (triggerrer == null || !triggerrer.GetBuffs(x => x.Spell.Id == ChangementCouleur.RefletsPlacides).Any()))
                return;

            // MYSTIC 20/10 (Fabrique de Malléfisk) : Intérêts 4504 (bouclier 500 si Malléfisk ≤ 50 % PV) part quand il est frappé, mais le
            // moteur déclenche AVANT de retirer les PV : le coup qui le faisait passer sous 50 % ne donnait jamais le bouclier. Il est
            // maintenant lancé juste après l'application des dégâts.
            if (buff.Spell.Id == Interets && damages != null && trigger == BuffTriggerType.OnDamaged)
            {
                var cible = buff.Target;
                var sort = buff.Spell;
                System.Action<FightActor, Fights.Damage> apres = null;
                apres = (f, d) =>
                {
                    cible.DamageInflicted -= apres;
                    // seulement pour CE coup (un coup bloqué par l'invulnérabilité ne passe jamais ici : rien n'est relancé plus tard)
                    if (d != damages || !cible.IsAlive() || cible.Fight.State != FightState.Fighting)
                        return;
                    cible.CastSpell(new SpellCastInformations(cible, sort, cible.Cell)
                    {
                        Force = true,
                        Silent = true,
                        ApFree = true,
                        TriggerEffect = this,
                        Triggerer = triggerrer
                    });
                };
                cible.DamageInflicted += apres;
                return;
            }

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

    /// <summary>
    /// MYSTIC 20/10 (Ventre de la Baleine, décision utilisateur) : Protozorreur (3828), passif Restauration infectieuse (4972).
    /// - Attraction cellulaire (4967) : à la première frappe du tour d'un joueur, si celui-ci est ALIGNÉ avec lui, il est attiré en ligne droite jusqu'au contact (il s'arrête
    ///   devant un obstacle, et SUR un de ses Protoglyphes s'il en croise un), même s'il est Indéplaçable (comme le Totem du Feu du
    ///   Darkli Moon). Avant : l'effet 1022 visait les ennemis du Protozorreur, personne n'était attiré.
    /// - Protoglyphe infectieux (4968, posé sous un joueur qui subit des dommages) : un seul glyphe par case, et pas de glyphe pour les
    ///   dommages du glyphe lui-même. Avant : chaque dommage (y compris les 60 du glyphe) en reposait un autre sur la même case,
    ///   en boucle : énormément de dommages.
    /// </summary>
    public static class Protozorreur
    {
        public const int Monstre = 3828, Attraction = 4967, Glyphe = 4968, SortDuGlyphe = 4966;

        static bool EstProtoglyphe(MarkTrigger m) => m is Glyph g && g.GlyphSpell != null && g.GlyphSpell.Id == SortDuGlyphe;

        /// <summary>false : ne pas poser de Protoglyphe pour ce dommage.</summary>
        public static bool PeutPoserGlyphe(FightActor cible, Fights.Damage damages)
        {
            if (cible == null || !cible.IsAlive() || cible.Cell == null)
                return false;
            if (damages != null && damages.Spell != null && damages.Spell.Id == SortDuGlyphe)
                return false;
            return !cible.Fight.GetTriggersByCell(cible.Cell).Any(EstProtoglyphe);
        }

        static readonly System.Runtime.CompilerServices.ConditionalWeakTable<FightActor, int[]> s_derniereFrappe =
            new System.Runtime.CompilerServices.ConditionalWeakTable<FightActor, int[]>();

        public static void Attirer(FightActor proto, FightActor attaquant)
        {
            if (attaquant == null || proto == null || !proto.IsAlive() || !attaquant.IsAlive() || attaquant.Team == proto.Team)
                return;
            var fight = proto.Fight;
            // seulement la première frappe du tour de chaque joueur (alignée ou non : une frappe non alignée « consomme » l'attraction)
            var tour = fight.TimeLine.RoundNumber * 10000 + (fight.FighterPlaying?.Id ?? 0);
            var dernier = s_derniereFrappe.GetValue(proto, _ => new[] { -1 });
            if (dernier[0] == tour)
                return;
            dernier[0] = tour;
            var but = attaquant.Position.Point;
            var depart = proto.Position.Point;
            // MYSTIC 20/10 (retour de test) : attiré seulement si l'attaquant est aligné avec lui (même ligne), en ligne droite
            if (depart.X != but.X && depart.Y != but.Y)
                return;
            var dx = System.Math.Sign(but.X - depart.X);
            var dy = System.Math.Sign(but.Y - depart.Y);
            var pos = depart;
            for (var i = 0; i < 64 && pos.ManhattanDistanceTo(but) > 1; i++)
            {
                var p = new Maps.Cells.MapPoint(pos.X + dx, pos.Y + dy);
                if (!Maps.Cells.MapPoint.IsInMap(p.X, p.Y))
                    break;
                var c = fight.Map.Cells[p.CellId];
                if (!c.Walkable || c.NonWalkableDuringFight || !fight.IsCellFree(c))
                    break;
                pos = p;
                if (fight.GetTriggersByCell(c).Any(EstProtoglyphe))
                    break;
            }
            if (pos.CellId == depart.CellId)
                return;
            var arrivee = fight.Map.Cells[pos.CellId];
            foreach (var perso in fight.GetCharactersAndSpectators().Where(proto.IsVisibleFor))
                Stump.Server.WorldServer.Handlers.Actions.ActionsHandler.SendGameActionFightSlideMessage(perso.Client, attaquant, proto, depart.CellId, arrivee.Id);
            proto.Position.Cell = arrivee;
            fight.TriggerMarks(arrivee, proto, TriggerType.MOVE);
        }
    }

    /// <summary>
    /// MYSTIC 20/10 (Ventre de la Baleine, décision utilisateur) : Dislocation (4935) des Krobes Disloqués (état 214, donné par la
    /// Malamibe) : les dommages qu'un ennemi inflige au Krobe sont interceptés par la Malamibe et DOUBLÉS ; la Malamibe réduisant de
    /// 50 % les dommages qu'elle subit (Décellularisation), taper un Krobe revient à la taper normalement (200 % x 50 % = 100 %).
    /// Avant : renvoi calculé sur le jet brut sans les caractéristiques de l'attaquant, marqué « renvoyé » (donc sans la réduction de
    /// 50 %), et une fois par buff Dislocation présent sur le Krobe.
    /// </summary>
    public static class Dislocation
    {
        public const int Sort = 4935, EtatDisloque = 214, Malamibe = 3822;

        static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Fights.Damage, object> s_traites =
            new System.Runtime.CompilerServices.ConditionalWeakTable<Fights.Damage, object>();

        static int Modele(FightActor f) => (f as MonsterFighter)?.Monster.Template.Id ?? (f as SummonedMonster)?.Monster.Template.Id ?? 0;

        public static void Intercepter(FightActor krobe, Fights.Damage damage)
        {
            if (krobe == null || damage == null || damage.ReflectedDamages || damage.Source == null || damage.Source.Team == krobe.Team ||
                damage.School == EffectSchoolEnum.Pushback || damage.School == EffectSchoolEnum.Healing || !krobe.HasState(EtatDisloque))
                return;
            if (s_traites.TryGetValue(damage, out _))
                return;
            s_traites.Add(damage, null);
            // Krobe invulnérable : le moteur déclenche avant les bonus de l'attaquant (jet brut) ; sinon les bonus sont déjà comptés
            var brut = krobe.IsInvulnerable(damage.Source.Position.Point.ManhattanDistanceTo(krobe.Position.Point) <= 1);
            foreach (var mala in krobe.Team.Fighters.Where(x => x.IsAlive() && Modele(x) == Malamibe).ToArray())
            {
                mala.InflictDamage(new Fights.Damage(damage.Amount * 2)
                {
                    Source = damage.Source,
                    School = damage.School,
                    IsCritical = damage.IsCritical,
                    IgnoreDamageBoost = !brut,
                    Spell = null,
                });
            }
        }
    }
}
