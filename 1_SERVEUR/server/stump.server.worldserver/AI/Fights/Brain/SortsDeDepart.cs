using Stump.Server.WorldServer.AI.Fights.Brain.Custom.Boss;
using Stump.Server.WorldServer.Game.Actors.Fight;
using System.Collections.Generic;

namespace Stump.Server.WorldServer.AI.Fights.Brain
{
    /// <summary>
    /// PADGREF 23/10 (audit DPLN de l'Eliocalypse) : SORTS DE DÉPART des monstres de l'Eliocalypse. Dans le client 2.74, chaque grade
    /// a un « startingSpellId » (niveau de sort) qui porte toute la mécanique du monstre : Arsenal (Guerre : invulnérable tant qu'un
    /// monstre vit, armes aux seuils), Sentence de la Balance (Misère), Esclavagisme (Servitude), script « Corruption » (tout l'Arbre
    /// de Mort), Miséreux, Fanatisme, Armutation, Escarmouche, Charogne, Coup d'arme… Importés mais jamais lancés : aucun passif ne
    /// marchait. Posés au début du combat (ou à l'arrivée d'une invocation) par le constructeur de Brain, quelle que soit l'IA.
    /// Généré depuis Monsters.d2o / SpellLevels.d2o 2.74 : (sort, niveau) par grade.
    /// </summary>
    public static class SortsDeDepart
    {
        static readonly Dictionary<int, int[,]> Sorts = new Dictionary<int, int[,]>
        {
            { 5955, new[,] { { 15104, 1 }, { 15104, 1 }, { 15104, 1 }, { 15104, 1 }, { 15104, 1 }, { 15104, 1 }, { 15104, 1 }, { 15104, 1 }, { 15104, 1 }, { 15104, 1 } } }, // Servitude : Esclavagisme
            { 5979, new[,] { { 15142, 1 }, { 15142, 1 }, { 15142, 1 }, { 15142, 1 }, { 15142, 1 } } }, // Armécréante : Escarmouche
            { 5983, new[,] { { 15165, 1 }, { 15165, 1 }, { 15165, 1 }, { 15165, 1 }, { 15165, 1 } } }, // Boularbin : Évasion
            { 5989, new[,] { { 15177, 1 }, { 15177, 1 }, { 15177, 1 }, { 15177, 1 }, { 15177, 1 } } }, // Ecaptif : Maltraitance
            { 5990, new[,] { { 15213, 1 }, { 15213, 1 }, { 15213, 1 }, { 15213, 1 }, { 15213, 1 }, { 15213, 1 }, { 15213, 1 }, { 15213, 1 }, { 15213, 1 }, { 15213, 1 } } }, // Misère : Sentence de la Balance
            { 5991, new[,] { { 15212, 2 }, { 15212, 2 }, { 15212, 2 }, { 15212, 2 }, { 15212, 2 } } }, // Ferrailleur : Miséreux
            { 5992, new[,] { { 15212, 2 }, { 15212, 2 }, { 15212, 2 }, { 15212, 2 }, { 15212, 2 } } }, // Krèvladal : Miséreux
            { 5993, new[,] { { 15212, 2 }, { 15212, 2 }, { 15212, 2 }, { 15212, 2 }, { 15212, 2 } } }, // Désosseur : Miséreux
            { 5994, new[,] { { 15212, 2 }, { 15212, 2 }, { 15212, 2 }, { 15212, 2 }, { 15212, 2 } } }, // Skentu : Miséreux
            { 5995, new[,] { { 15212, 2 }, { 15212, 2 }, { 15212, 2 }, { 15212, 2 }, { 15212, 2 } } }, // Dawaj : Miséreux
            { 5997, new[,] { { 15270, 1 }, { 15270, 1 }, { 15270, 1 }, { 15270, 1 }, { 15270, 1 } } }, // Charogne : Charogne
            { 6010, new[,] { { 15262, 1 }, { 15262, 1 }, { 15262, 1 }, { 15262, 1 }, { 15262, 1 } } }, // Épée Géante : Coup d'épée
            { 6011, new[,] { { 15263, 1 }, { 15263, 1 }, { 15263, 1 }, { 15263, 1 }, { 15263, 1 } } }, // Marteau Géant : Coup de marteau
            { 6012, new[,] { { 15264, 1 }, { 15264, 1 }, { 15264, 1 }, { 15264, 1 }, { 15264, 1 } } }, // Hache Géante : Coup de hache
            { 6013, new[,] { { 15265, 1 }, { 15265, 1 }, { 15265, 1 }, { 15265, 1 }, { 15265, 1 } } }, // Morgenstern Géant : Coup de morgenstern
            { 6014, new[,] { { 15247, 1 }, { 15247, 1 }, { 15247, 1 }, { 15247, 1 }, { 15247, 1 }, { 15247, 1 }, { 15247, 1 }, { 15247, 1 }, { 15247, 1 }, { 15247, 1 } } }, // Guerre : Arsenal
            { 6021, new[,] { { 15341, 1 }, { 15341, 1 }, { 15341, 1 }, { 15341, 1 }, { 15341, 1 } } }, // Ravalame : Fanatisme
            { 6023, new[,] { { 15352, 1 }, { 15352, 1 }, { 15352, 1 }, { 15352, 1 }, { 15352, 1 } } }, // Olgoth : Armutation
            { 6025, new[,] { { 15595, 1 }, { 15595, 1 }, { 15595, 1 }, { 15595, 1 }, { 15595, 1 } } }, // Trancharnier : Intenable
            { 6026, new[,] { { 15396, 1 }, { 15396, 1 }, { 15396, 1 }, { 15396, 1 }, { 15396, 1 }, { 15753, 1 }, { 15753, 1 }, { 15753, 1 }, { 15753, 1 }, { 15753, 1 } } }, // Corruption : Corruption
            { 6027, new[,] { { 15396, 1 }, { 15396, 1 }, { 15396, 1 }, { 15396, 1 }, { 15396, 1 } } }, // Pistilangue : Corruption
            { 6028, new[,] { { 15396, 1 }, { 15396, 1 }, { 15396, 1 }, { 15396, 1 }, { 15396, 1 } } }, // Dolid : Corruption
            { 6029, new[,] { { 15396, 1 }, { 15396, 1 }, { 15396, 1 }, { 15396, 1 }, { 15396, 1 } } }, // Nheur'Gueule : Corruption
            { 6030, new[,] { { 15396, 1 }, { 15396, 1 }, { 15396, 1 }, { 15396, 1 }, { 15396, 1 } } }, // Tentaclaque : Corruption
            { 6031, new[,] { { 15396, 1 }, { 15396, 1 }, { 15396, 1 }, { 15396, 1 }, { 15396, 1 } } }, // Gangredogue : Corruption
            { 6120, new[,] { { 15754, 1 }, { 15754, 1 }, { 15754, 1 }, { 15754, 1 }, { 15754, 1 } } }, // Feu primordial : Feu Primordial
        };

        public static void Poser(AIFighter fighter)
        {
            var template = (fighter as MonsterFighter)?.Monster?.Template?.Id ?? (fighter as Game.Actors.Fight.SummonedMonster)?.MonsterGrade?.MonsterId ?? 0;
            if (!Sorts.TryGetValue(template, out var parGrade))
                return;
            var grade = (int)((fighter as MonsterFighter)?.MonsterGrade?.GradeId ?? (fighter as Game.Actors.Fight.SummonedMonster)?.MonsterGrade?.GradeId ?? 1);
            var i = System.Math.Max(0, System.Math.Min(parGrade.GetLength(0) - 1, grade - 1));
            if (parGrade[i, 0] > 0)
                SortPassif.Poser(fighter, parGrade[i, 0], parGrade[i, 1]);
        }
    }
}
