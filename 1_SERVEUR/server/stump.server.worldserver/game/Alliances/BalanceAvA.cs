using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Stump.Core.Reflection;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;

namespace Stump.Server.WorldServer.Game.Alliances
{
    /// <summary>
    /// MYSTIC 19/10 : « Balance » des zones AvA (onglet Balance du panneau d'alliance, à la place du Bulletin).
    /// Toutes les 2 minutes : dans chaque sous-zone qui a un prisme, nombre de membres connectés de chaque alliance. Le joueur ne voit
    /// que des POURCENTAGES : la part de son alliance (le reste en gris, sans nom ni nombre). Lecture seule : ne touche ni aux prismes
    /// ni à la capture de territoire.
    /// Échange avec l'onglet (comme l'onglet Village) : demande = message du jour « §BAL§ » (intercepté), réponse = message
    /// d'information « §BAL§secondes;zone actuelle;% dans la zone actuelle (-1 = hors zone AvA / pas de membre);autres zones
    /// (nom^% séparés par |) » caché par le chat.
    /// </summary>
    public class BalanceAvA : Singleton<BalanceAvA>
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public const string Marqueur = "§BAL§";
        const int Periode = 2 * 60 * 1000;

        readonly object m_lock = new object();
        Dictionary<int, Dictionary<int, int>> m_zones = new Dictionary<int, Dictionary<int, int>>();   // sous-zone -> (alliance -> membres)
        DateTime m_calcul = DateTime.MinValue;

        [Initialization(typeof(PrismManager))]
        public void Initialize()
        {
            WorldServer.Instance.IOTaskPool.CallPeriodically(Periode, Calculer);
        }

        void Calculer()
        {
            try
            {
                var zones = new Dictionary<int, Dictionary<int, int>>();
                foreach (var c in World.Instance.GetCharacters(x => x.Guild?.Alliance != null && x.SubArea != null && x.Map != null))
                {
                    var sz = c.SubArea.Id;
                    // zones AvA = sous-zones capturables (prisme posé ou non) ; pas Astrub ni Incarnam (interdites aux prismes)
                    if (!PrismManager.EstCapturable(sz) || c.SubArea.Record.AreaId == 18 || c.SubArea.Record.AreaId == 45)
                        continue;
                    if (!zones.TryGetValue(sz, out var parAlliance))
                        zones[sz] = parAlliance = new Dictionary<int, int>();
                    var a = c.Guild.Alliance.Id;
                    parAlliance[a] = (parAlliance.TryGetValue(a, out var n) ? n : 0) + 1;
                }
                lock (m_lock)
                {
                    m_zones = zones;
                    m_calcul = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"[Balance] calcul : {ex.Message}");
            }
        }

        static string Propre(string s) => (s ?? "").Replace("~", "-").Replace(";", ",").Replace("|", "/").Replace("^", " ");

        static int Pourcent(Dictionary<int, int> parAlliance, int alliance)
        {
            var total = parAlliance.Values.Sum();
            if (total <= 0 || !parAlliance.TryGetValue(alliance, out var n))
                return -1;
            return (int)Math.Round(100.0 * n / total);
        }

        public string Donnees(Character c)
        {
            var alliance = c.Guild?.Alliance;
            Dictionary<int, Dictionary<int, int>> zones;
            DateTime calcul;
            lock (m_lock)
            {
                zones = m_zones;
                calcul = m_calcul;
            }
            // données absentes ou plus vieilles que 2 minutes (le minuteur ne tourne pas forcément) : on recalcule maintenant
            if (calcul == DateTime.MinValue || (DateTime.Now - calcul).TotalMilliseconds >= Periode)
            {
                Calculer();
                lock (m_lock) { zones = m_zones; calcul = m_calcul; }
            }
            var restant = Math.Max(0, (int)Math.Ceiling((calcul.AddMilliseconds(Periode) - DateTime.Now).TotalSeconds));
            var ici = c.SubArea;
            var nomIci = Propre(ici?.Record?.Name ?? "");
            if (alliance == null)
                return Marqueur + $"{restant};{nomIci};-1;";

            var pctIci = ici != null && zones.TryGetValue(ici.Id, out var z) ? Pourcent(z, alliance.Id) : -1;
            var autres = zones.Where(kv => kv.Key != ici?.Id && kv.Value.ContainsKey(alliance.Id))
                .Select(kv => (nom: Propre(World.Instance.GetSubArea(kv.Key)?.Record?.Name ?? kv.Key.ToString()), pct: Pourcent(kv.Value, alliance.Id)))
                .OrderBy(x => x.nom)
                .Select(x => $"{x.nom}^{x.pct}");
            return Marqueur + $"{restant};{nomIci};{pctIci};{string.Join("|", autres)}";
        }
    }
}
