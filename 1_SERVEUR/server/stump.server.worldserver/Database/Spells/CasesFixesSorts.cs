using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using Stump.Server.WorldServer.Game.Effects.Instances;

namespace Stump.Server.WorldServer.Database.Spells
{
    /// <summary>
    /// MYSTIC 17/10 : zones « ;NNN » (une case précise de la carte) des sorts. L'import de spells_levels ne savait pas lire les cases
    /// supérieures à 255 et les a perdues (2267 effets : Chalœil, Nidas, Meno, Koutoulou, Dantinéa, XLII…). Elles sont relues
    /// depuis cases_fixes.tsv (généré depuis le client par _OUTILS/serveur/donjons/generer_cases_fixes.py) :
    /// levelId TAB critique(0/1) TAB index de l'effet TAB effectId TAB case.
    /// </summary>
    public static class CasesFixesSorts
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();
        static Dictionary<(uint, int, int), (int effet, int cellule)> m_cases;
        static readonly object m_verrou = new object();

        static Dictionary<(uint, int, int), (int, int)> Cases
        {
            get
            {
                if (m_cases != null)
                    return m_cases;

                lock (m_verrou)
                {
                    if (m_cases != null)
                        return m_cases;

                    var cases = new Dictionary<(uint, int, int), (int, int)>();
                    try
                    {
                        var chemin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cases_fixes.tsv");
                        if (File.Exists(chemin))
                        {
                            foreach (var ligne in File.ReadAllLines(chemin))
                            {
                                var p = ligne.Split('\t');
                                if (p.Length < 5)
                                    continue;
                                cases[(uint.Parse(p[0]), int.Parse(p[1]), int.Parse(p[2]))] = (int.Parse(p[3]), int.Parse(p[4]));
                            }
                        }
                        else
                            logger.Warn("[Sorts] cases_fixes.tsv introuvable : les zones « case fixe » ne marcheront pas");
                    }
                    catch (Exception ex)
                    {
                        logger.Error("[Sorts] lecture de cases_fixes.tsv impossible : {0}", ex.Message);
                    }

                    return m_cases = cases;
                }
            }
        }

        public static List<EffectDice> Appliquer(uint levelId, bool critique, List<EffectDice> effets)
        {
            if (effets == null)
                return null;

            var cases = Cases;
            for (var i = 0; i < effets.Count; i++)
            {
                if (cases.TryGetValue((levelId, critique ? 1 : 0, i), out var c) && (int)effets[i].EffectId == c.Item1)
                    effets[i].DefinirCaseFixe(c.Item2);
            }

            return effets;
        }
    }
}
