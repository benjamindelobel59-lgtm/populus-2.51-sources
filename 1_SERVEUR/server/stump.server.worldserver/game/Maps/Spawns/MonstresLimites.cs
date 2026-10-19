using System.Collections.Generic;
using System.Linq;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Monsters;

namespace Stump.Server.WorldServer.Game.Maps.Spawns
{
    /// <summary>
    /// MYSTIC 19/10 : monstres rares limités à N exemplaires vivants par sous-zone (demande utilisateur : « 1 max par zone ») :
    /// Leprechaun, Hell Mina, Muldos sauvages et Volkornes sauvages (une seule famille = une seule limite).
    /// Vérifié au tirage d'un nouveau groupe (Map.GenerateRandomMonsterGroup) et à la restauration des groupes sauvegardés.
    /// </summary>
    public static class MonstresLimites
    {
        /// <summary>Familles limitées : tous les monstres d'une famille comptent ensemble (ex. 1 Muldo sauvage par zone, toutes couleurs).</summary>
        static readonly (int[] Monstres, int Max)[] Familles =
        {
            (new[] { 1247 }, 1),                               // Leprechaun
            (new[] { 107 }, 1),                                // Hell Mina
            (new[] { 4434, 4435, 4436, 4437, 4438 }, 1),       // Muldos sauvages (Indigo, Ebène, Orchidée, Pourpre, Doré)
            (new[] { 5308, 5309, 5311, 5313 }, 1),             // Volkornes sauvages (Orchidée, Indigo, Ebène, Pourpre)
        };

        static readonly Dictionary<int, (int[] Monstres, int Max)> ParMonstre =
            Familles.SelectMany(f => f.Monstres.Select(m => new KeyValuePair<int, (int[], int)>(m, f))).ToDictionary(x => x.Key, x => x.Value);

        public static bool EstLimite(int monsterId) => ParMonstre.ContainsKey(monsterId);

        /// <summary>Vrai si la sous-zone de la map a déjà son maximum de ce monstre (en comptant aussi le groupe en cours de création).</summary>
        public static bool Atteinte(Map map, int monsterId, IEnumerable<int> dejaDansLeGroupe = null)
        {
            if (!ParMonstre.TryGetValue(monsterId, out var famille))
                return false;

            var ids = new HashSet<int>(famille.Monstres);
            var n = dejaDansLeGroupe?.Count(x => ids.Contains(x)) ?? 0;
            var maps = map.SubArea != null ? map.SubArea.Maps : new[] { map };
            foreach (var m in maps)
                foreach (var g in m.GetActors<MonsterGroup>())
                    n += g.GetMonsters().Count(x => ids.Contains(x.Grade.MonsterId));

            return n >= famille.Max;
        }
    }
}
