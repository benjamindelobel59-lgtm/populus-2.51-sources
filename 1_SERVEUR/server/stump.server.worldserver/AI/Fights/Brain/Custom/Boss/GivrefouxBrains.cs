using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    // MYSTIC 17/10 : Tanière Givrefoux. Les passifs officiels (données du client 2.51) n'étaient jamais lancés.
    // Chacun est posé au début du combat, ou dès l'arrivée si le monstre est invoqué (« Foux d'amour » du Tengu).

    /// <summary>
    /// Tengu Givrefoux : « Croquette » (Invulnérable + Lourd). Déblocage : avec Floutage (Yokaï amené en diagonale) ET Fulguration
    /// (Yomi frappé à l'arme en ligne), il lance « Toison givrée » au début de son tour : il grossit et perd l'invulnérabilité 4 tours.
    /// MYSTIC 17/10 : l'IA jugeait ce sort « sans effet » (rien sur l'ennemi) et ne le lançait jamais → déblocage impossible.
    /// </summary>
    [BrainIdentifier((int)MonsterIdEnum.TENGU_GIVREFOUX)]
    public class TenguGivrefouxBrain : Brain
    {
        public const int CroquetteSpellId = 2677;
        public const int ToisonGivreeSpellId = 2684;

        public TenguGivrefouxBrain(AIFighter fighter)
            : base(fighter)
        {
            SortPassif.Poser(fighter, CroquetteSpellId);
            fighter.Fight.FightStarted += OnFightStarted;
        }

        // MYSTIC 17/10 : 2 invocations max (1 Yokaï + 1 Yomi, choix forcé dans Summon.cs) ; avant : limite à 1
        private void OnFightStarted(IFight fight)
        {
            fight.FightStarted -= OnFightStarted;
            Fighter.Stats[PlayerFields.SummonLimit].Base = 2;
        }

        public override void Play()
        {
            // MYSTIC 18/10 : journal pour vérifier le déblocage en jeu (Floutage = Yokaï déplacé en diagonale du Tengu, Fulguration =
            // Yomi frappé à l'arme en ligne avec le Tengu)
            NLog.LogManager.GetCurrentClassLogger().Info("[Tengu] debut de tour : Floutage {0}, Fulguration {1}, invocations [{2}]",
                Fighter.HasState((int)SpellStatesEnum.FLOUTAGE_87), Fighter.HasState((int)SpellStatesEnum.FULGURATION_86),
                string.Join(", ", System.Linq.Enumerable.Select(Fighter.Team.GetAllFighters(x => x.IsAlive() && x.Summoner == Fighter),
                    x => (x as SummonedMonster)?.Monster.Template.Id + "@" + x.Cell.Id)));

            if (Fighter.HasState((int)SpellStatesEnum.FLOUTAGE_87) && Fighter.HasState((int)SpellStatesEnum.FULGURATION_86))
                Fighter.CastAutoSpell(new Spell(ToisonGivreeSpellId, 1), Fighter.Cell);

            base.Play();
        }
    }

    /// <summary>Yokaï Givrefoux : « Fourrure » (déplacé → Floutage 2 tours, +10 dommages, +1 PM, Affaibli aux ennemis).</summary>
    [BrainIdentifier((int)MonsterIdEnum.YOKAI_GIVREFOUX)]
    public class YokaiGivrefouxBrain : Brain
    {
        public const int FourrureSpellId = 668;

        public YokaiGivrefouxBrain(AIFighter fighter)
            : base(fighter)
        {
            SortPassif.Poser(fighter, FourrureSpellId);
        }
    }

    /// <summary>Yomi Givrefoux : « Foudre » (frappé au corps à corps → Fulguration).</summary>
    [BrainIdentifier((int)MonsterIdEnum.YOMI_GIVREFOUX)]
    public class YomiGivrefouxBrain : Brain
    {
        public const int FoudreSpellId = 669;

        public YomiGivrefouxBrain(AIFighter fighter)
            : base(fighter)
        {
            SortPassif.Poser(fighter, FoudreSpellId);
            // MYSTIC 18/10 (demande utilisateur) : Fulguration (2670) seulement sur un COUP D'ARME, à toute distance (le déclencheur
            // DCAC partait sur tout coup au contact, y compris les dommages de poussée contre un obstacle). Filtre dans CastSpell.cs.
            fighter.BuffAdded += (f, b) =>
            {
                if (b is Game.Fights.Buffs.TriggerBuff t && t.Spell.Id == Fulguration)
                    t.SetTrigger(Game.Fights.Buffs.BuffTriggerType.OnDamagedByWeapon);
            };
        }

        public const int Fulguration = 2670;
    }

    /// <summary>Fuji Givrefoux : « Instinct maternel » (mort d'un allié → +1000 dommages à Fuji / Tengu, +1 PM aux alliés).</summary>
    [BrainIdentifier((int)MonsterIdEnum.FUJI_GIVREFOUX)]
    public class FujiGivrefouxBrain : Brain
    {
        public const int InstinctMaternelSpellId = 2676;

        public FujiGivrefouxBrain(AIFighter fighter)
            : base(fighter)
        {
            SortPassif.Poser(fighter, InstinctMaternelSpellId);
        }
    }

    /// <summary>Fuji Givrefoux Nourricière (salle Tengu + Fuji) : même passif que Fuji.</summary>
    [BrainIdentifier((int)MonsterIdEnum.FUJI_GIVREFOUX_NOURRICIERE)]
    public class FujiNourriciereBrain : Brain
    {
        public FujiNourriciereBrain(AIFighter fighter)
            : base(fighter)
        {
            SortPassif.Poser(fighter, FujiGivrefouxBrain.InstinctMaternelSpellId);
        }
    }
}
