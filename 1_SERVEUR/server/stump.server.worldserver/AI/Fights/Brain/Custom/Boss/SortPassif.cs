using System;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    /// <summary>
    /// MYSTIC : pose le sort passif officiel d'un monstre sur lui-même.
    /// Monstre présent au lancement : au début du combat. Monstre invoqué (ou arrivé en vague) en cours de combat : dès son
    /// arrivée — l'événement FightStarted ne lui parvient jamais (ex. Sporakne invoquée par le Korriandre, jamais passive avant).
    /// </summary>
    public static class SortPassif
    {
        public static void Poser(AIFighter fighter, int spellId, int level = 1)
        {
            if (fighter.Fight.State == FightState.Fighting)
            {
                Action<FightTeam, FightActor> arrivee = null;
                arrivee = (team, actor) =>
                {
                    if (actor != fighter)
                        return;
                    team.FighterAdded -= arrivee;
                    fighter.CastAutoSpell(new Spell(spellId, (short)level), fighter.Cell);
                };
                fighter.Team.FighterAdded += arrivee;
            }
            else
            {
                Action<IFight> debut = null;
                debut = fight =>
                {
                    fight.FightStarted -= debut;
                    if (fighter.IsAlive())
                        fighter.CastAutoSpell(new Spell(spellId, (short)level), fighter.Cell);
                };
                fighter.Fight.FightStarted += debut;
            }
        }
    }
}

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    using System.Linq;
    using Stump.DofusProtocol.Enums;
    using Stump.Server.WorldServer.Game.Effects.Handlers.Spells.States;
    using Stump.Server.WorldServer.Game.Effects.Instances;
    using Stump.Server.WorldServer.Game.Fights.Buffs;
    using Stump.Server.WorldServer.Game.Fights.Buffs.Customs;

    /// <summary>MYSTIC 17/10 : outils d'états pour les boss (pose d'un état permanent, retrait des désactivations).</summary>
    public static class EtatsBoss
    {
        public const int Invulnerable = 56;

        public static void Ajouter(FightActor cible, FightActor lanceur, int etat, int spellId,
            FightDispellableEnum dispellable = FightDispellableEnum.REALLY_NOT_DISPELLABLE)
        {
            var state = Game.Spells.SpellManager.Instance.GetSpellState((uint)etat);
            if (state == null)
                return;

            var handler = new AddState(new EffectDice(EffectsEnum.Effect_AddState, etat, 0, 0) { Duration = -1 }, cible, null, cible.Cell, false);
            var buff = new StateBuff(cible.PopNextBuffId(), cible, lanceur, handler, new Spell(spellId, 1), dispellable, state) { Duration = -1000 };
            cible.AddBuff(buff, true);
        }

        /// <summary>Retire les désactivations de l'état (l'état redevient actif, ex. invulnérable à nouveau).</summary>
        public static bool RetirerDesactivations(FightActor cible, int etat)
        {
            var buffs = cible.GetBuffs(x => x is DisableStateBuff d && d.StateBuff?.State?.Id == etat).ToArray();
            foreach (var buff in buffs)
                cible.RemoveBuff(buff);
            return buffs.Length > 0;
        }

        public static bool EstDesactive(FightActor cible, int etat) =>
            cible.GetBuffs(x => x is DisableStateBuff d && d.StateBuff?.State?.Id == etat).Any();
    }

    /// <summary>
    /// MYSTIC 17/10 : un boss rendu vulnérable (désactivation de son invulnérabilité) le reste jusqu'au début du tour suivant
    /// du joueur qui l'a déclenché, puis redevient invulnérable (Royalmouth, Grolloum).
    /// </summary>
    public class VulnerableJusquAuTourDuDeclencheur
    {
        readonly FightActor m_boss;
        readonly IFight m_fight;
        FightActor m_declencheur;

        public VulnerableJusquAuTourDuDeclencheur(FightActor boss)
        {
            m_boss = boss;
            m_fight = boss.Fight;
            boss.BuffAdded += OnBuffAdded;
            m_fight.TurnStarted += OnTurnStarted;
            m_fight.FightEnded += OnFightEnded;
        }

        public FightActor Declencheur => m_declencheur;

        void OnBuffAdded(FightActor boss, Buff buff)
        {
            if (buff is DisableStateBuff d && d.StateBuff?.State?.Id == EtatsBoss.Invulnerable)
            {
                buff.Duration = -1000; // c'est ce suivi qui la retire, pas la durée du sort
                var declencheur = buff.BuffTriggered && buff.DecrementReference is CharacterFighter ? buff.DecrementReference : m_fight.FighterPlaying;
                m_declencheur = declencheur != boss ? declencheur : m_declencheur;
            }
        }

        void OnTurnStarted(IFight fight, FightActor joueur)
        {
            if (m_declencheur == null || joueur != m_declencheur)
                return;

            m_declencheur = null;
            using (fight.StartSequence(SequenceTypeEnum.SEQUENCE_TRIGGERED))
                EtatsBoss.RetirerDesactivations(m_boss, EtatsBoss.Invulnerable);
        }

        void OnFightEnded(IFight fight)
        {
            m_boss.BuffAdded -= OnBuffAdded;
            fight.TurnStarted -= OnTurnStarted;
            fight.FightEnded -= OnFightEnded;
        }
    }
}
