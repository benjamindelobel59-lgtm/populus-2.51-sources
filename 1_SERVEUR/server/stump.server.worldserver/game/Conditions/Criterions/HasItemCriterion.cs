using System;
using System.Linq;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Conditions.Criterions
{
    public class HasItemCriterion : Criterion
    {
        public const string Identifier = "PO";

        public int Item
        {
            get;
            set;
        }

        /// <summary>PADGREF 21/10 : quantité (« PO<2269,10 » = moins de 10 exemplaires de l'objet 2269), -1 = forme sans quantité.</summary>
        public int Quantite { get; set; } = -1;

        public override bool Eval(Character character)
        {
            if (Quantite >= 0)
            {
                long n = character.Inventory.Where(entry => entry.Template.Id == Item).Sum(entry => (long)entry.Stack);
                switch (Operator)
                {
                    case ComparaisonOperatorEnum.INFERIOR: return n < Quantite;
                    case ComparaisonOperatorEnum.SUPERIOR: return n > Quantite;
                    case ComparaisonOperatorEnum.EQUALS: return n == Quantite;
                    case ComparaisonOperatorEnum.INEQUALS: return n != Quantite;
                }
            }
            if (Operator == ComparaisonOperatorEnum.EQUALS)
                return character.Inventory.Any(entry => entry.Template.Id == Item);

             return Operator == ComparaisonOperatorEnum.INEQUALS && !character.Inventory.Any(entry => entry.Template.Id == Item && entry.IsEquiped());
        }

        public override void Build()
        {
            int itemId;
            var parts = Literal.Split(',');
            if (parts.Length == 2 && int.TryParse(parts[1], out var quantite))
                Quantite = quantite;
            if (!int.TryParse(parts[0], out itemId))
                throw new Exception(string.Format("Cannot build HasItemCriterion, {0} is not a valid item id", Literal));

            Item = itemId;
        }

        public override string ToString()
        {
            return FormatToString(Identifier);
        }
    }
}