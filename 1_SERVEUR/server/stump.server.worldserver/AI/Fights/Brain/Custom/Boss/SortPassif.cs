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
            // MYSTIC 18/10 : sort absent de la base (ex. nouveau sort pas encore importé) : on ne pose rien au lieu de planter le combat
            if (Stump.Server.WorldServer.Game.Spells.SpellManager.Instance.GetSpellTemplate(spellId) == null)
                return;

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

        /// <summary>MYSTIC 19/10 : le combattant passe tous ses tours jusqu'à la fin du combat (comme les monstres corrompus du Vortex).</summary>
        public static void AjouterPasseTour(FightActor cible, FightActor lanceur, int spellId)
        {
            if (cible.GetBuffs(x => x is SkipTurnBuff && x.Spell.Id == spellId).Any())
                return;

            var handler = new AddState(new EffectDice(EffectsEnum.Effect_AddState, 157, 0, 0) { Duration = -1 }, cible, null, cible.Cell, false);
            var buff = new SkipTurnBuff(cible.PopNextBuffId(), cible, lanceur, handler, new Spell(spellId, 1), false,
                FightDispellableEnum.REALLY_NOT_DISPELLABLE) { Duration = -1000 };
            cible.AddBuff(buff, true);
        }

        /// <summary>MYSTIC 17/10 : état pour un nombre de tours donné (ex. prisme invulnérable 2 tours).</summary>
        public static void AjouterPourTours(FightActor cible, FightActor lanceur, int etat, int spellId, int tours)
        {
            var state = Game.Spells.SpellManager.Instance.GetSpellState((uint)etat);
            if (state == null)
                return;

            var handler = new AddState(new EffectDice(EffectsEnum.Effect_AddState, etat, 0, 0) { Duration = tours }, cible, null, cible.Cell, false);
            var buff = new StateBuff(cible.PopNextBuffId(), cible, lanceur, handler, new Spell(spellId, 1), FightDispellableEnum.REALLY_NOT_DISPELLABLE, state) { Duration = (short)tours };
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

        // MYSTIC 18/10 : true = une invocation encore en vie garde la main (Klime : poussé par une invocation pendant SON tour, il reste
        // vulnérable jusqu'au tour suivant de cette invocation, donc pendant le tour du joueur)
        readonly bool m_garderInvocation;

        public VulnerableJusquAuTourDuDeclencheur(FightActor boss, bool garderInvocation = false)
        {
            m_garderInvocation = garderInvocation;
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
                var declencheur = buff.BuffTriggered && buff.DecrementReference is CharacterFighter ? buff.DecrementReference : m_fight.FighterPlaying;
                // MYSTIC 18/10 : déclenché par une invocation (Kolosso) : c'est le tour de son invocateur qui compte (l'invocation peut
                // mourir avant de rejouer : le boss restait vulnérable tout le combat)
                while (declencheur != null && declencheur.IsSummoned() && declencheur.Summoner != null && declencheur.Summoner != declencheur
                       && !(m_garderInvocation && declencheur.IsAlive() && declencheur.CanPlay()))
                    declencheur = declencheur.Summoner;
                m_declencheur = declencheur != boss ? declencheur : m_declencheur;
                // MYSTIC 18/10 : 1 tour, décompté au début du tour du déclencheur (l'affichage vient de spells_effects_fix, SQL 54) ; ce suivi
                // la retire aussi au même moment
                if (m_declencheur != null)
                    buff.DecrementReference = m_declencheur;
                if (buff.Duration == -1000 || buff.Duration > 1)
                    buff.Duration = 1;
            }
        }

        void OnTurnStarted(IFight fight, FightActor joueur)
        {
            // MYSTIC 18/10 : déclencheur mort ou parti : la vulnérabilité s'arrête au premier début de tour suivant
            if (m_declencheur == null || (joueur != m_declencheur && m_declencheur.IsAlive() && m_declencheur.Fight == fight))
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
