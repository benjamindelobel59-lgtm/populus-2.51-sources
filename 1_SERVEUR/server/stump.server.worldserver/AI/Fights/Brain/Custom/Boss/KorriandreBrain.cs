using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Game.Actors.Fight;
using Stump.Server.WorldServer.Game.Effects.Handlers.Spells.Marks;
using Stump.Server.WorldServer.Game.Fights;
using System.Linq;

namespace Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss
{
    /// <summary>Korriandre : passif « Kète » (glyphes Daipipay à chaque début de tour ennemi), 3 invocations max.</summary>
    [BrainIdentifier((int)MonsterIdEnum.KORRIANDRE)]
    public class KorriandreBrain : Brain
    {
        public KorriandreBrain(AIFighter fighter)
            : base(fighter)
        {
            fighter.Fight.FightStarted += Fight_FightStarted;
            SortPassif.Poser(fighter, (int)SpellIdEnum.KWEST);
        }

        private void Fight_FightStarted(IFight obj)
        {
            obj.FightStarted -= Fight_FightStarted;
            Fighter.Stats.Fields.FirstOrDefault(x => x.Key == PlayerFields.SummonLimit).Value.Base = 3;
            AnnoncerCouleursGlyphes();
        }

        /// <summary>MYSTIC 17/10 : chaque joueur pose des glyphes de sa couleur ; on dit dans le chat qui a quelle couleur.</summary>
        private void AnnoncerCouleursGlyphes()
        {
            var joueurs = Fighter.OpposedTeam.Fighters.OfType<CharacterFighter>().ToArray();
            if (joueurs.Length == 0)
                return;

            foreach (var destinataire in joueurs)
            {
                destinataire.Character.SendServerMessage("Glyphes du Korriandre : chacun pose les siennes, mortelles pour son équipe.");
                foreach (var joueur in joueurs)
                {
                    var index = GlyphSpawn.GetKorriandreColorIndex(joueur);
                    destinataire.Character.SendServerMessage(string.Format("{0} : glyphes {1}", joueur.Name, GlyphSpawn.KorriandreColorNames[index]),
                        GlyphSpawn.KorriandreColors[index]);
                }
            }
        }
    }

    /// <summary>
    /// MYSTIC : Sporakne (invoquée par le Korriandre) : passif « Haimji » (déplacée → pousse sa ligne de 1).
    /// Avant le 17/10 il n'était posé qu'au début du combat, donc jamais (elle arrive toujours en cours de combat).
    /// </summary>
    [BrainIdentifier((int)MonsterIdEnum.SPORAKNE)]
    public class SporakneBrain : Brain
    {
        public SporakneBrain(AIFighter fighter)
            : base(fighter)
        {
            SortPassif.Poser(fighter, (int)SpellIdEnum.JIEM);
        }
    }

    /// <summary>Mérulette : passif « Mérule Traçon » (frappée à distance → riposte), aussi si elle est invoquée.</summary>
    [BrainIdentifier((int)MonsterIdEnum.MERULETTE)]
    public class MeruletteBrain : Brain
    {
        public MeruletteBrain(AIFighter fighter)
            : base(fighter)
        {
            SortPassif.Poser(fighter, (int)SpellIdEnum.SERPULVERISE);
        }
    }
}
