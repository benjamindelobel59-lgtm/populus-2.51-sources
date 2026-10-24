using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Stump.Core.Attributes;
using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Game.Effects.Instances;

namespace Stump.Server.WorldServer.Game.Items.Player.Custom
{
    /// <summary>
    /// MYSTIC : trousseau de clefs (objet 10207, non échangeable, donné à tous les personnages).
    /// Il contient toutes les clefs : une entrée gratuite par donjon à clef (dungeons_entries.KeyItemId), utilisée avant la vraie clef.
    /// Réinitialisation tous les ResetJours jours, comptés depuis la première utilisation ; enregistré en base (nekfeu_trousseau),
    /// donc un redémarrage du serveur ne remet rien à zéro.
    /// </summary>
    public class TrousseauManager : Singleton<TrousseauManager>
    {
        public const int TrousseauItemId = 10207;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static Stump.ORM.Database Database => WorldServer.Instance.DBAccessor.Database;

        [Variable(true)]
        public static int ResetJours = 7;

        private readonly ConcurrentDictionary<int, Etat> m_etats = new ConcurrentDictionary<int, Etat>();

        public class Etat
        {
            public DateTime ResetAt;
            public HashSet<int> Utilises = new HashSet<int>();
        }

        private class EtatRow
        {
            public int CharacterId { get; set; }
            public DateTime ResetAt { get; set; }
            public string UtilisesCSV { get; set; }
        }

        [Initialization(InitializationPass.Sixth)]
        public void Initialize()
        {
            try
            {
                Database.Execute("CREATE TABLE IF NOT EXISTS nekfeu_trousseau (CharacterId INT NOT NULL PRIMARY KEY, ResetAt DATETIME NOT NULL, UtilisesCSV TEXT NOT NULL) DEFAULT CHARSET=utf8");
            }
            catch (Exception ex)
            {
                logger.Error("[Trousseau] table nekfeu_trousseau : {0}", ex.Message);
            }
        }

        /// <summary>État du trousseau du personnage (remis à zéro si la date est passée).</summary>
        public Etat GetEtat(Character character)
        {
            var etat = m_etats.GetOrAdd(character.Id, id =>
            {
                var row = Database.Fetch<EtatRow>("SELECT CharacterId, ResetAt, UtilisesCSV FROM nekfeu_trousseau WHERE CharacterId=@0", id).FirstOrDefault();
                var e = new Etat { ResetAt = row?.ResetAt ?? DateTime.MinValue };
                if (row?.UtilisesCSV != null)
                    foreach (var part in row.UtilisesCSV.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        if (int.TryParse(part, out var dj))
                            e.Utilises.Add(dj);
                return e;
            });

            lock (etat)
            {
                if (etat.ResetAt <= DateTime.Now && etat.Utilises.Count > 0)
                {
                    etat.Utilises.Clear();
                    Save(character.Id, etat);
                }
            }
            return etat;
        }

        /// <summary>Le trousseau a encore la clef de ce donjon (sans la consommer) ?</summary>
        public bool CanUse(Character character, int dungeonId)
        {
            if (!HasTrousseau(character))
                return false;
            var etat = GetEtat(character);
            lock (etat)
                return !etat.Utilises.Contains(dungeonId);
        }

        /// <summary>Entrée gratuite pour ce donjon ? Si oui elle est consommée (et enregistrée).</summary>
        public bool TryUse(Character character, int dungeonId)
        {
            if (!HasTrousseau(character))
                return false;

            var etat = GetEtat(character);
            lock (etat)
            {
                if (etat.Utilises.Contains(dungeonId))
                    return false;

                // la periode de 7 jours commence a la premiere entree utilisee
                if (etat.Utilises.Count == 0 || etat.ResetAt <= DateTime.Now)
                    etat.ResetAt = DateTime.Now.AddDays(ResetJours);

                etat.Utilises.Add(dungeonId);
                Save(character.Id, etat);
                return true;
            }
        }

        public bool HasTrousseau(Character character)
        {
            var template = ItemManager.Instance.TryGetTemplate(TrousseauItemId);
            return template != null && character.Inventory.HasItem(template);
        }

        static int[] s_clefs;

        /// <summary>PADGREF 23/10 : clefs de tous les donjons à clef (dungeons_entries), objets existants seulement, triées.</summary>
        static int[] Clefs
        {
            get
            {
                if (s_clefs == null)
                {
                    try
                    {
                        s_clefs = Database.Fetch<int>("SELECT DISTINCT KeyItemId FROM dungeons_entries WHERE KeyItemId > 0")
                            .Where(k => ItemManager.Instance.TryGetTemplate(k) != null).OrderBy(k => k).ToArray();
                    }
                    catch (Exception ex)
                    {
                        logger.Error("[Trousseau] liste des clefs : {0}", ex.Message);
                        s_clefs = new int[0];
                    }
                }
                return s_clefs;
            }
        }

        /// <summary>À la connexion : chaque personnage a son trousseau, non échangeable.</summary>
        public void EnsureTrousseau(Character character)
        {
            try
            {
                var template = ItemManager.Instance.TryGetTemplate(TrousseauItemId);
                if (template == null)
                    return;

                var item = character.Inventory.TryGetItem(template) ?? character.Inventory.AddItem(template, 1);
                if (item == null)
                    return;

                // etat lu en base sur le fil des E/S (evite la lecture dans le fil de la map au 1er gardien)
                WorldServer.Instance.IOTaskPool.AddMessage(() => GetEtat(character));

                var change = false;
                if (!item.Effects.Any(x => x.EffectId == EffectsEnum.Effect_NonExchangeable_981))
                {
                    item.Effects.Clear();
                    item.Effects.Add(new EffectInteger(EffectsEnum.Effect_NonExchangeable_981, 1));
                    change = true;
                }

                // PADGREF 23/10 (retour utilisateur : « dans les anciens trousseaux il n'y a pas les nouveaux donjons ») : une ligne
                // « clef » (effet 814) par clef de donjon (dungeons_entries), ajoutée si elle manque (nouveaux donjons : Eliocalypse...)
                foreach (var clef in Clefs)
                {
                    if (item.Effects.Any(x => (int)x.EffectId == 814 && x is EffectDice d && d.DiceNum == clef))
                        continue;
                    item.Effects.Add(new EffectDice((EffectsEnum)814, 0, clef, 0));
                    change = true;
                }

                if (change)
                {
                    item.Invalidate();
                    character.Inventory.RefreshItem(item);
                }
            }
            catch (Exception ex)
            {
                logger.Error("[Trousseau] {0} : {1}", character.Name, ex.Message);
            }
        }

        private void Save(int characterId, Etat etat)
        {
            var csv = string.Join(",", etat.Utilises);
            var resetAt = etat.ResetAt == DateTime.MinValue ? DateTime.Now : etat.ResetAt;
            WorldServer.Instance.IOTaskPool.AddMessage(() =>
            {
                try
                {
                    Database.Execute("REPLACE INTO nekfeu_trousseau (CharacterId, ResetAt, UtilisesCSV) VALUES (@0, @1, @2)", characterId, resetAt, csv);
                }
                catch (Exception ex)
                {
                    logger.Error("[Trousseau] sauvegarde {0} : {1}", characterId, ex.Message);
                }
            });
        }
    }
}
