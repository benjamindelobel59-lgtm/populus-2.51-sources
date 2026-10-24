using System.Collections.Generic;
using Stump.Server.WorldServer.Game.Actors.Fight;

namespace Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Targets
{
    public class LastAttackerCriterion : TargetCriterion
    {
        public LastAttackerCriterion(bool required)
        {
            Required = required;
        }

        public bool Required
        {
            get;
            set;
        }

        public override bool IsTargetValid(FightActor actor, SpellEffectHandler handler)
        {
            // MYSTIC 18/10 : sorts de donjons où « o » / « O » désignent celui qui a déclenché l'effet (voir CibleDeclencheur)
            if (CibleDeclencheur.Concerne(handler))
                return CibleDeclencheur.Reference(handler) == actor;

            return Required ? handler.Caster.LastAttacker == actor : handler.Caster.LastAttacker != actor;
        }
    }

    /// <summary>
    /// MYSTIC 18/10 (audit des donjons, décision utilisateur : correction limitée à ces sorts). Dans le client 2.51, « O » et « o »
    /// visent l'entité qui a déclenché l'effet (l'attaquant, le pousseur, le tueur), et « O » la vise même hors de la zone du sort.
    /// Le moteur lisait « o » comme « tout le monde SAUF le dernier attaquant » et ne regardait que la case du porteur pour « O » :
    /// - O : Bump 3787, Webond 3793, Inflexibilité 3805 (Terrier du Wa Wabbit), Intérêts 4504 (Malléfisk), Pendultimatum 3643 (Comte) ;
    /// - o : Totems 6163-6167 (Moon), Boursicotage 4483 / Changement de camp 4512 (Nidas), Encrage 4115 (Ombre).
    /// Les autres sorts gardent l'ancien comportement (sorts de classe, Kolosso, Korriandre, Glourséleste intacts).
    /// </summary>
    public static class CibleDeclencheur
    {
        static readonly HashSet<int> Sorts = new HashSet<int>
        {
            3787, 3793, 3805, 4504, 3643, 3649, // PADGREF 23/10 : 3649 Fuseau (Comte, tour impair) : vise l'attaquant
            4008, 5101, 5091, 5058, 5038, 5044, 5047, 4037,
            6163, 6164, 6165, 6166, 6167, 4483, 4512, 4115,
            4520, // MYSTIC 20/10 : Reflets placides du Lévitrof (échange avec le joueur vert qui le frappe)
            // PADGREF 21/10 (retours bêta-testeur, audit des sorts déclenchés « quand il est frappé » à une case) : l'effet vise
            // l'attaquant (avant : la case du monstre, donc rien) : Poison incurable (Scoliopode), Dard contemporain (Termystique),
            // Sporange mécanique (Biofiente, Drosérâle), Retour historique (Kanimate), Hiboum (Granduk), Indigestion (Cawotman),
            // Détentrave (Caznoar), Distanss (Hin), Crève-cœur (Dame du Hasard), Instabilité (Krosmoglob, Énergie du portail),
            // Ailurophilie (Chasquatch, Gardien de la Flamme)
            4736, 3540, 3544, 3590, 3625, 3812, 4130, 5086, 6311, 4418, 6391,
            // PADGREF 21/10 (retour utilisateur) : Ovagodra (Cohobation de la Nileza) : frappée à distance, elle échange sa place avec
            // l'attaquant (avant : sort lancé sur sa propre case, aucun échange), puis Molalité illéplochable frappe autour d'elle
            3566,
            // PADGREF 22/10 (audit Belvédère d'Ilyzaelle) : Brûle-âme : le joueur qui déplace un monstre / tente de lui retirer des PM
            // prend l'état Brûlure (avant : sort lancé sur la case du monstre, la Brûlure n'était jamais posée)
            9079,
        };

        public static bool Concerne(SpellEffectHandler handler) => handler?.CastHandler?.Spell != null && Sorts.Contains(handler.CastHandler.Spell.Id);

        /// <summary>Celui qui a déclenché le sort (déclencheur du buff), sinon le dernier attaquant du lanceur.</summary>
        public static FightActor Reference(SpellEffectHandler handler) =>
            handler.CastHandler?.Informations?.Triggerer ?? handler.Caster.LastAttacker;
    }
}
