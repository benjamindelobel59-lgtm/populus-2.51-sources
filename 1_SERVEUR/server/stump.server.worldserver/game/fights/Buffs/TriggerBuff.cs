using System;
using System.Collections.Generic;
using System.Linq;
using Stump.DofusProtocol.Types;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Spells;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Effects.Handlers.Spells;
using Stump.Server.WorldServer.Game.Fights.Sequences;

namespace Stump.Server.WorldServer.Game.Fights.Buffs
{
    public delegate void TriggerBuffApplyHandler(TriggerBuff buff, FightActor trigerrer, BuffTriggerType trigger, object token);
    public delegate void TriggerBuffRemoveHandler(TriggerBuff buff);

    public class TriggerBuff : Buff
    {
        public static readonly Dictionary<string, BuffTriggerType> m_triggersMapping = new Dictionary<string, BuffTriggerType>()
        {
            {"I", BuffTriggerType.Instant},
            {"D", BuffTriggerType.OnDamaged},
            {"DA", BuffTriggerType.OnDamagedAir},
            {"DE", BuffTriggerType.OnDamagedEarth},
            {"DF", BuffTriggerType.OnDamagedFire},
            {"DW", BuffTriggerType.OnDamagedWater},
            {"DN", BuffTriggerType.OnDamagedNeutral},
            {"DBA", BuffTriggerType.OnDamagedByAlly},
            {"DBE", BuffTriggerType.OnDamagedByEnemy},
            {"DI", BuffTriggerType.OnDamagedBySummon},
            {"DC", BuffTriggerType.OnDamagedByWeapon},
            {"DS", BuffTriggerType.OnDamagedBySpell},
            {"DG", BuffTriggerType.OnDamagedByGlyph},
            {"DP", BuffTriggerType.OnDamagedByTrap},
            {"DM", BuffTriggerType.OnDamagedInCloseRange},
            {"DR", BuffTriggerType.OnDamagedInLongRange},
            {"MD", BuffTriggerType.OnDamagedByPush},
            {"MDP", BuffTriggerType.OnDamagedByEnemyPush},
            {"MDM", BuffTriggerType.OnDamageEnemyByPush},
            {"Dr", BuffTriggerType.OnDamagedUnknown_2},
            {"DTB", BuffTriggerType.OnDamagedUnknown_3},
            {"DTE", BuffTriggerType.OnDamagedUnknown_4},
            {"TB", BuffTriggerType.OnTurnBegin},
            {"TE", BuffTriggerType.OnTurnEnd},
            {"m", BuffTriggerType.OnMPLost},
            {"A", BuffTriggerType.OnAPLost},
            {"H", BuffTriggerType.OnHealed},
            {"EO", BuffTriggerType.OnStateAdded},
            {"Eo", BuffTriggerType.OnStateRemoved},
            {"CC", BuffTriggerType.OnCriticalHit},
            {"d", BuffTriggerType.OnDispelled},
            {"M", BuffTriggerType.OnMoved},
            {"mA", BuffTriggerType.Unknown_3},
            {"ML", BuffTriggerType.Unknown_4},
            {"MP", BuffTriggerType.OnPushed},
            {"MA", BuffTriggerType.OnPulled}, // MYSTIC 17/10 : attiree (Ombre, Silhouette, Phossile...)
            {"MS", BuffTriggerType.Unknown_6},
            {"PT", BuffTriggerType.UsedPortal},
            {"R", BuffTriggerType.OnRangeLost},
            {"tF", BuffTriggerType.OnTackled},
            {"tS", BuffTriggerType.OnTackle},
            {"X", BuffTriggerType.OnDeath},
            {"MPA", BuffTriggerType.OnMPAttack},
            {"APA", BuffTriggerType.OnAPAttack},
            {"PD", BuffTriggerType.OnPushDamaged},
            {"CDM", BuffTriggerType.OnMakeMeleeDamage},
            {"CDR", BuffTriggerType.OnMakeDistanceDamage},
            // PADGREF 23/10 (Eliocalypse, Brutalyte) : 2.74 « inflige des dommages au contact » / « par sort »
            {"CDCAC", BuffTriggerType.OnMakeMeleeDamage},
            {"CDS", BuffTriggerType.OnMakeDistanceDamage},
            // PADGREF 23/10 (audit DPLN Eliocalypse, Arbre de Mort) : « TP » = téléporté (équivalent déplacé), « LPU » = soigné (même à PV
            // pleins) : maladies de Corruption (Terrain fertile, Tétanyte, Aura du Destrier Purulent, Cauchemar de Corruption)
            {"TP", BuffTriggerType.OnMoved},
            {"LPU", BuffTriggerType.OnHealed},
            {"CMP", BuffTriggerType.HaveMoveDuringTurn},
            {"PPD", BuffTriggerType.OnInderctlyPush},
            {"PMD", BuffTriggerType.OnPushDamagedInMelee},
            // MYSTIC : declencheurs des sorts 2.51 manquants (ex. Gloursons) : DCAC = frappe au corps a corps, CT = tacle quelqu'un
            {"DCAC", BuffTriggerType.OnDamagedInCloseRange},
            {"CT", BuffTriggerType.OnTackle},
        };

        private FightSequence m_lastTriggeredSequence;
        private Damage m_lastDamageTrigger;
        
        public TriggerBuff(int id, FightActor target, FightActor caster, SpellEffectHandler effect, Spell spell, Spell parentSpell, bool critical, FightDispellableEnum dispelable, int priority,
            TriggerBuffApplyHandler applyTrigger, TriggerBuffRemoveHandler removeTrigger = null)
            : base(id, target, caster, effect, spell, critical, dispelable)
        {
            ParentSpell = parentSpell;
            ApplyTrigger = applyTrigger;
            RemoveTrigger = removeTrigger;

            Triggers = Effect.Triggers.Split('|').Select(GetTriggerFromString).ToList();
        }

        public object Token
        {
            get;
            set;
        }

        public Spell ParentSpell
        {
            get;
        }

        public TriggerBuffApplyHandler ApplyTrigger
        {
            get;
        }

        public TriggerBuffRemoveHandler RemoveTrigger
        {
            get;
        }

        public List<BuffTrigger> Triggers
        {
            get;
            protected set;
        }

        public void SetTrigger(BuffTriggerType trigger)
        {
            Triggers = new List<BuffTrigger> { new BuffTrigger(trigger) };
        }

        public bool ShouldTrigger(BuffTriggerType type, object token = null)
            => Delay == 0 && Triggers.Any(x => x.Type == type && (x.Parameter == null || x.Parameter.Equals(token)));

        public override void Apply()
        {
            if (ShouldTrigger(BuffTriggerType.Instant))
                Apply(Caster, BuffTriggerType.Instant);
        }

        public void Apply(FightActor fighterTrigger, BuffTriggerType trigger, object token)
        {
            // to avoid recursion cannot be triggered twice in the same sequence (spell cast, move, turn end/begin...)

            if (m_lastTriggeredSequence != null && m_lastTriggeredSequence.IsChild(fighterTrigger.Fight.CurrentSequence))
                return;

            // PADGREF 23/10 (crash client, Corruption / Arbre de Mort) : un déclencheur qui se relance lui-même (ex. aura « Bouillon de
            // culture » : état 4074 retiré -> sort relancé -> glyphe -> état 4074 posé puis retiré…) bouclait des centaines de fois
            // hors séquence de sort (garde ci-dessus inopérante) -> flot de messages, client planté. Pas de relance pendant sa propre
            // exécution, et au plus MaxDeclenchementsParTour déclenchements par tour de jeu.
            if (m_enCours)
                return;

            // PADGREF 23/10 : Carapace gardée de la Lucrane (passif 4746, posé pour l'affichage) : l'Indéplaçable « dégâts de poussée »
            // du passif dure tout le combat ; c'est LucraneBrain qui le pose pour 1 tour (et coupe la carapace pendant ce tour).
            if (Spell?.Id == 4746 && EffectHandler?.Effect.EffectId == EffectsEnum.Effect_AddState)
                return;

            var tour = fighterTrigger.Fight.TimeLine?.RoundNumber ?? 0;
            if (tour != m_tourDeclenchements)
            {
                m_tourDeclenchements = tour;
                m_declenchementsDuTour = 0;
            }
            if (++m_declenchementsDuTour > MaxDeclenchementsParTour)
                return;

            // PADGREF 23/10 (règle officielle « Aquazomb », Tour de Bethel / Solar) : seulement à partir du tour 2, et UNE fois par fin
            // de tour du joueur : chaque monstre pose son propre déclencheur sur le joueur, mais un seul lancer applique déjà tout
            // (+2 PM / +1 PO à tous les monstres et l'effet spécial de chacun) ; avant : 3-4 lancers -> boosts en double.
            if (Spell?.Id == SortAquazomb)
            {
                if (tour < 2)
                    return;
                // (celui de Bethel compte aussi : log VPS 23/10, il donne les mêmes boosts -> en l'excluant, boosts en double)
                var dernier = s_aquazombParCible.GetOrCreateValue(Target);
                if (dernier.Value == tour && dernier.Fight == Target.Fight)
                    return;
                dernier.Value = tour;
                dernier.Fight = Target.Fight;
            }

            m_lastTriggeredSequence = fighterTrigger.Fight.CurrentSequence;
            m_enCours = true;
            try
            {
                base.Apply();
                ApplyTrigger?.Invoke(this, fighterTrigger, trigger, token);
            }
            finally
            {
                m_enCours = false;
            }
        }

        const int MaxDeclenchementsParTour = 30, SortAquazomb = 9451;

        sealed class DernierAquazomb { public int Value = -1; public IFight Fight; }
        static readonly System.Runtime.CompilerServices.ConditionalWeakTable<FightActor, DernierAquazomb> s_aquazombParCible =
            new System.Runtime.CompilerServices.ConditionalWeakTable<FightActor, DernierAquazomb>();
        bool m_enCours;
        int m_tourDeclenchements = -1, m_declenchementsDuTour;

        public void Apply(FightActor fighterTrigger, BuffTriggerType trigger)
        {
            Apply(fighterTrigger, trigger, Token);
        }
        
        public override void Dispell()
        {
            base.Dispell();
            RemoveTrigger?.Invoke(this);
        }

        public override AbstractFightDispellableEffect GetAbstractFightDispellableEffect()
        {
            var turnDuration = Delay == 0 ? Duration : Delay;

            var values = ValeursPourClient(Effect);

            return new FightTriggeredEffect((uint)Id, Target.Id, turnDuration,
                (sbyte)Dispellable,
                (ushort)ParentSpell.Id, (uint)(EffectFix?.ClientEffectId ?? EffetUidClient), 0,
                (values.Length > 0 ? Convert.ToInt32(values[0]) : 0),
                (values.Length > 1 ? Convert.ToInt32(values[1]) : 0),
                (values.Length > 2 ? Convert.ToInt32(values[2]) : 0),
                Delay);
        }

        /// <summary>
        /// MYSTIC 20/10 (Griffure Ondulante : « 76 à 6080 ») : pour les effets « forceMinMax » (dégâts...), le client 2.51 lit un effet
        /// déclenché comme un lancer de dés : min = p3 + p1, max = p1 × p2 + p3. On envoie donc (1, max - min + 1, min - 1).
        /// PADGREF 23/10 (« 190 à 50350 » sur les maladies de Corruption) : commun à tous les buffs envoyés comme effet déclenché
        /// (SpellBuff, StatBuff, StateBuff n'avaient pas la correction).
        /// </summary>
        public static object[] ValeursPourClient(Effects.Instances.EffectBase effet)
        {
            var values = effet.GetValues();
            if (effet is Effects.Instances.EffectDice de && de.Template != null && de.Template.ForceMinMax && de.Max > 0)
            {
                var min = de.DiceFace == 0 ? de.DiceNum : de.Min;
                var max = de.Max;
                values = new object[] { 1, max - min + 1, min - 1 };
            }
            return values;
        }

        public static BuffTrigger GetTriggerFromString(string str)
        {
            if (m_triggersMapping.ContainsKey(str))
                return new BuffTrigger(m_triggersMapping[str]);

            if (str.StartsWith("EON"))
            {
                var test = int.Parse(str.Remove(0, "EON".Length));

                if(test == 826)
                    return new BuffTrigger(BuffTriggerType.AttackWithASpecificState, test);

                return new BuffTrigger(BuffTriggerType.OnSpecificStateAdded, test);
            }

            if (str.StartsWith("Eon"))
            {
                var test = int.Parse(str.Remove(0, "Eon".Length));
                return new BuffTrigger(BuffTriggerType.OnSpecificStateAdded, test);
            }

            if (str == "T")
                return new BuffTrigger(BuffTriggerType.OnTackled);

            // PADGREF 23/10 (Eliocalypse) : « TR<sort> » = le seuil de PV posé par ce sort (effet 2872) vient d'être atteint
            if (str.StartsWith("TR") && int.TryParse(str.Substring(2), out var sortSeuil))
                return new BuffTrigger(BuffTriggerType.OnThresholdReached, sortSeuil);

            if (str == "P")
                return new BuffTrigger(BuffTriggerType.OnPushed);

            if (str.StartsWith("EOFF"))
                return new BuffTrigger(BuffTriggerType.OnSpecificStateRemoved, int.Parse(str.Remove(0, "EOFF".Length)));

            if (str.StartsWith("EO"))
                return new BuffTrigger(BuffTriggerType.OnSpecificStateAdded, int.Parse(str.Remove(0, "EO".Length)));

            if (str.StartsWith("Eo"))
                return new BuffTrigger(BuffTriggerType.OnSpecificStateRemoved, int.Parse(str.Remove(0, "Eo".Length)));

            return new BuffTrigger(BuffTriggerType.Unknown);
        }
    }
}