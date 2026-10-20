using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    /// <summary>
    /// MYSTIC 20/10 : Fabrique de Malléfisk (dofusdb donjon 77). Sorts identiques à dofusdb ; IA refaite pour lancer tous les sorts.
    /// Malléfisk : passif Ofranor 4505 (Intérêts 4504 quand il est frappé : +100 % de dommages pour lui et l'attaquant, bouclier 500
    /// sous 50 % PV, voir CastSpell) ; Banqueroute 4508 (retire les Intérêts, dégâts selon ses PV érodés, +2 PM, bouclier 1000 pendant
    /// 2 tours, relance 5 tours) lancée dès qu'il est à 50 % PV ou moins (avant : jamais lancée par l'IA normale).
    /// </summary>
    [BrainIdentifier(3652)]
    public class MallefiskBrain : BrainPhorreur
    {
        const int Ofranor = 4505, Banqueroute = 4508;

        public MallefiskBrain(AIFighter f) : base(f)
        {
            SortPassif.Poser(f, Ofranor);
        }

        public override void Play()
        {
            LancerBanqueroute();
            base.Play();
            LancerBanqueroute();
        }

        void LancerBanqueroute()
        {
            if (!Actif() || Fighter.LifePoints * 2 > Fighter.MaxLifePoints)
                return;
            if (Fighter.Spells.TryGetValue(Banqueroute, out var sort) && Fighter.CanCastSpell(sort, Fighter.Cell) == SpellCastResult.OK)
                Fighter.CastSpell(sort, Fighter.Cell);
        }
    }

    /// <summary>Boursoin : soins (4558, 4559) par l'IA normale ; 4561 sur lui (soigne autour de lui quand il est frappé).</summary>
    [BrainIdentifier(3663)]
    public class BoursoinBrain : BrainPhorreur
    {
        public BoursoinBrain(AIFighter f) : base(f) { }
        protected override int[] SortsSurSoi => new[] { 4561 };
    }

    /// <summary>Mimikado : 4562, 4563, 4564 (dégâts à distance) par l'IA normale ; ne passe plus son tour.</summary>
    [BrainIdentifier(3664)]
    public class MimikadoBrain : BrainPhorreur
    {
        public MimikadoBrain(AIFighter f) : base(f) { }
    }

    /// <summary>Trésantène : 4565, 4567 par l'IA normale ; 4566 sur lui (+100 % dommages et +1 PM quand il est frappé).</summary>
    [BrainIdentifier(3665)]
    public class TresanteneBrain : BrainPhorreur
    {
        public TresanteneBrain(AIFighter f) : base(f) { }
        protected override int[] SortsSurSoi => new[] { 4566 };
    }

    /// <summary>Berserkoffre : 4568 échange de place avec un ennemi (et devient invulnérable 1 tour) pour aller au contact ;
    /// 4569 (autour de lui), 4570 (contact) par l'IA normale.</summary>
    [BrainIdentifier(3666)]
    public class BerserkoffreBrain : BrainPhorreur
    {
        public BerserkoffreBrain(AIFighter f) : base(f) { }
        protected override int[] SortsSurEnnemi => new[] { 4568 };
    }

    /// <summary>Précieux : 4571, 4572, 4573 par l'IA normale ; ne passe plus son tour.</summary>
    [BrainIdentifier(3667)]
    public class PrecieuxBrain : BrainPhorreur
    {
        public PrecieuxBrain(AIFighter f) : base(f) { }
    }
}
