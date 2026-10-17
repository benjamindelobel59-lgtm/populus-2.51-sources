using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stump.Core.IO;
using Stump.DofusProtocol.D2oClasses;
using Stump.DofusProtocol.Enums;
using Stump.Server.WorldServer.Database.Items.Templates;
using Stump.Server.WorldServer.Database.Spells;
using Stump.Server.WorldServer.Game.Effects;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Game.Spells;

namespace Stump.Server.WorldServer.Game.Admin
{
    /// <summary>
    /// Panel admin : lecture / écriture des effets binaires des objets (PossibleEffectsBin) et des niveaux de sorts
    /// (EffectsBin, CriticalEffectsBin), et rechargement à chaud d'un objet ou d'un sort déjà chargé.
    /// Le panel manipule une liste simple { i (index d'origine, -1 = nouvel effet), effectId, min, max, ... }.
    /// </summary>
    public static class AdminPanelData
    {
        private static Stump.ORM.Database Database => WorldServer.Instance.DBAccessor.Database;

        /* ------------------------------------------------------------------ objets */

        public static string ItemEffects(int itemId)
        {
            var template = ItemManager.Instance.TryGetTemplate(itemId);
            if (template == null)
                throw new Exception("objet introuvable");
            var list = (template.PossibleEffects ?? new List<EffectInstance>()).Select((e, i) => ItemEffectDto(e, i)).ToList();
            return JsonConvert.SerializeObject(list);
        }

        private static object ItemEffectDto(EffectInstance e, int index)
        {
            var dice = e as EffectInstanceDice;
            var integer = e as EffectInstanceInteger;
            var minmax = e as EffectInstanceMinMax;
            return new
            {
                i = index,
                kind = e.GetType().Name.Replace("EffectInstance", ""),
                effectId = e.effectId,
                min = dice != null ? (long?)dice.diceNum : minmax != null ? (long?)minmax.min : integer?.value,
                max = dice != null ? (long)dice.diceSide : minmax != null ? (long?)minmax.max : null,
                value = integer?.value,
                text = (e as EffectInstanceString)?.text,
                duration = e.duration,
                random = e.random,
                group = e.group,
                targetMask = e.targetMask,
                triggers = e.triggers,
                visible = e.visibleInTooltip,
            };
        }

        public static void SetItemEffects(int itemId, string json)
        {
            var template = ItemManager.Instance.TryGetTemplate(itemId);
            if (template == null)
                throw new Exception("objet introuvable");
            var old = template.PossibleEffects ?? new List<EffectInstance>();
            var result = new List<EffectInstance>();
            foreach (var dto in JArray.Parse(json).Cast<JObject>())
            {
                var index = (int?)dto["i"] ?? -1;
                EffectInstance effect;
                if (index >= 0 && index < old.Count)
                    effect = old[index];
                else
                    effect = new EffectInstanceDice { targetMask = "", triggers = "I", visibleInTooltip = true, visibleInBuffUi = true, visibleInFightLog = true, dispellable = 1 };

                effect.effectId = (uint)(int)dto["effectId"];
                var min = (int?)dto["min"] ?? 0;
                var max = (int?)dto["max"] ?? 0;
                switch (effect)
                {
                    case EffectInstanceDice d:
                        d.diceNum = (uint)Math.Max(0, min);
                        d.diceSide = (uint)Math.Max(0, max > min ? max : 0);
                        d.value = (int?)dto["value"] ?? d.value;
                        break;
                    case EffectInstanceMinMax mm:
                        mm.min = (uint)Math.Max(0, min);
                        mm.max = (uint)Math.Max(0, max);
                        break;
                    case EffectInstanceInteger n:
                        n.value = (int?)dto["value"] ?? min;
                        break;
                    case EffectInstanceString s:
                        s.text = (string)dto["text"] ?? s.text;
                        break;
                }
                if (dto["visible"] != null)
                    effect.visibleInTooltip = (bool)dto["visible"];
                result.Add(effect);
            }

            template.PossibleEffects = result;   // met aussi à jour PossibleEffectsBin et les effets convertis
            var table = template is WeaponTemplate ? "items_templates_weapons" : "items_templates";
            Database.Execute($"UPDATE {table} SET PossibleEffectsBin=@0 WHERE Id=@1", template.PossibleEffectsBin, itemId);
        }

        /// <summary>Relit l'objet en base (niveau, prix, poids, conditions, effets...) et met à jour le modèle déjà chargé.</summary>
        public static void ReloadItem(int itemId)
        {
            var template = ItemManager.Instance.TryGetTemplate(itemId);
            if (template == null)
                throw new Exception("objet introuvable (les nouveaux objets demandent un redémarrage)");
            if (template is WeaponTemplate weapon)
                Copy(Database.Query<WeaponTemplate>(WeaponTemplateRelator.FetchQuery + " WHERE Id=@0", itemId).FirstOrDefault(), weapon);
            else
                Copy(Database.Query<ItemTemplate>(ItemTemplateRelator.FetchQuery + " WHERE Id=@0", itemId).FirstOrDefault(), template);
            // PossibleEffectsBin ne rafraîchit pas les effets convertis gardés en cache : on repasse par PossibleEffects
            template.PossibleEffects = template.PossibleEffects;
        }

        /* ------------------------------------------------------------------ sorts */

        private static object SpellEffectDto(EffectDice e, int index) => new
        {
            i = index,
            effectId = (int)e.EffectId,
            min = e.DiceNum,
            max = e.DiceFace,
            value = e.Value,
            duration = e.Duration,
            delay = e.Delay,
            random = e.Random,
            group = e.Group,
            targetMask = e.TargetMask,
            triggers = e.Triggers,
            zoneShape = (int)e.ZoneShape,
            zoneSize = e.ZoneSize,
            zoneMinSize = e.ZoneMinSize,
            zoneEfficiency = e.ZoneEfficiencyPercent,
            zoneMaxEfficiency = e.ZoneMaxEfficiency,
            order = e.Order,
            hidden = e.Hidden,
        };

        public static string SpellLevelEffects(int levelId)
        {
            var level = SpellManager.Instance.GetSpellLevel(levelId);
            if (level == null)
                throw new Exception("niveau de sort introuvable");
            return JsonConvert.SerializeObject(new
            {
                effects = level.Effects.Select((e, i) => SpellEffectDto(e, i)).ToList(),
                critical = level.CriticalEffects.Select((e, i) => SpellEffectDto(e, i)).ToList(),
            });
        }

        public static void SetSpellLevelEffects(int levelId, bool critical, string json)
        {
            var level = SpellManager.Instance.GetSpellLevel(levelId);
            if (level == null)
                throw new Exception("niveau de sort introuvable");
            var old = critical ? level.CriticalEffects : level.Effects;
            var result = new List<EffectDice>();
            foreach (var dto in JArray.Parse(json).Cast<JObject>())
            {
                var index = (int?)dto["i"] ?? -1;
                var effect = index >= 0 && index < old.Count
                    ? new EffectDice(old[index])
                    : old.Count > 0 ? new EffectDice(old[0]) : new EffectDice(EffectsEnum.Effect_DamageNeutral, 0, 0, 0) { TargetMask = "A,a", Triggers = "I" };

                effect.EffectId = (EffectsEnum)(int)dto["effectId"];
                effect.DiceNum = (int?)dto["min"] ?? 0;
                effect.DiceFace = (int?)dto["max"] ?? 0;
                effect.Value = (int?)dto["value"] ?? 0;
                effect.Duration = (int?)dto["duration"] ?? 0;
                effect.Delay = (int?)dto["delay"] ?? 0;
                effect.Random = (int?)dto["random"] ?? 0;
                effect.Group = (int?)dto["group"] ?? 0;
                effect.TargetMask = (string)dto["targetMask"] ?? effect.TargetMask;
                effect.Triggers = (string)dto["triggers"] ?? effect.Triggers;
                effect.ZoneShape = (SpellShapeEnum)((int?)dto["zoneShape"] ?? (int)effect.ZoneShape);
                effect.ZoneSize = (uint)((int?)dto["zoneSize"] ?? (int)effect.ZoneSize);
                effect.ZoneMinSize = (uint)((int?)dto["zoneMinSize"] ?? (int)effect.ZoneMinSize);
                effect.ZoneEfficiencyPercent = (int?)dto["zoneEfficiency"] ?? effect.ZoneEfficiencyPercent;
                effect.ZoneMaxEfficiency = (int?)dto["zoneMaxEfficiency"] ?? effect.ZoneMaxEfficiency;
                effect.Order = (int?)dto["order"] ?? effect.Order;
                effect.Hidden = (bool?)dto["hidden"] ?? effect.Hidden;
                result.Add(effect);
            }

            var bin = EffectManager.Instance.SerializeEffects(result);
            Database.Execute($"UPDATE spells_levels SET {(critical ? "CriticalEffectsBin" : "EffectsBin")}=@0 WHERE Id=@1", bin, levelId);
            // relit les octets : les cibles (TargetMask) et les zones sont analysées comme au chargement du serveur
            if (critical)
                level.CriticalEffectsBin = bin;
            else
                level.EffectsBin = bin;
        }

        /// <summary>Relit les niveaux d'un sort en base (PA, portée, relances...) et met à jour les niveaux déjà chargés.</summary>
        public static void ReloadSpell(int spellId)
        {
            foreach (var fresh in Database.Query<SpellLevelTemplate>(SpellLevelTemplateRelator.FetchQuery + " WHERE SpellId=@0", spellId))
            {
                var level = SpellManager.Instance.GetSpellLevel((int)fresh.Id);
                if (level != null)
                    Copy(fresh, level);
            }
        }

        /* ------------------------------------------------------------------ support : objets des joueurs */

        /// <summary>Jets d'objets de joueurs (SerializedEffects en base64) -> [[ [effet, valeurs...], ... ], ...].</summary>
        public static string DecodeItemEffects(string jsonBase64List)
        {
            var result = new List<object>();
            foreach (var b64 in JArray.Parse(jsonBase64List).Select(x => (string)x))
            {
                try
                {
                    var effects = string.IsNullOrEmpty(b64) ? new List<EffectBase>() : EffectManager.Instance.DeserializeEffects(Convert.FromBase64String(b64));
                    result.Add(effects.Select(e => new { id = (int)e.EffectId, v = e.GetValues() }).ToList());
                }
                catch
                {
                    result.Add(null);
                }
            }
            return JsonConvert.SerializeObject(result);
        }

        /// <summary>Rend à un personnage connecté un objet d'une copie (mêmes jets exacts).</summary>
        public static string GiveItemFromCopy(int characterId, string table, int rowId)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(table, "^admin_copy_[0-9]{12}_[ib]$"))
                throw new Exception("copie invalide");
            var character = World.Instance.GetCharacter(characterId);
            if (character == null)
                throw new Exception("le personnage doit être connecté");
            var row = Database.Fetch<CopyItemRow>($"SELECT ItemId, Stack, SerializedEffects FROM {table} WHERE Id=@0", rowId).FirstOrDefault();
            if (row == null)
                throw new Exception("objet introuvable dans la copie");
            var template = ItemManager.Instance.TryGetTemplate(row.ItemId);
            if (template == null)
                throw new Exception("modèle d'objet introuvable");
            var effects = row.SerializedEffects == null ? new List<EffectBase>() : EffectManager.Instance.DeserializeEffects(row.SerializedEffects);
            character.Area.ExecuteInContext(() => character.Inventory.AddItem(template, effects, (int)row.Stack));
            return $"{row.Stack} x {template.Name} rendu(s) à {character.Name}";
        }

        private class CopyItemRow
        {
            public int ItemId { get; set; }
            public uint Stack { get; set; }
            public byte[] SerializedEffects { get; set; }
        }

        /// <summary>Copie des personnages, objets, banques (une par jour, gardées 14 jours) pour l'onglet Support.</summary>
        /* ------------------------------------------------------------------ panoplies */

        /// <summary>Bonus d'une panoplie : une liste par nombre d'objets portés (index 0 = 1 objet), chaque bonus { effectId, value }.</summary>
        public static string ItemSetEffects(int setId)
        {
            var set = ItemManager.Instance.TryGetItemSetTemplate((uint)setId);
            if (set == null)
                throw new Exception("panoplie introuvable");
            var list = (set.Effects ?? new List<List<EffectBase>>()).Select(step => (step ?? new List<EffectBase>()).Select(e => new
            {
                effectId = (int)e.Id,
                value = e is EffectDice d ? (d.DiceNum != 0 ? Math.Max(d.DiceNum, d.DiceFace) : d.Value) : e is EffectInteger n ? n.Value : 0,   // bonus du d2o = dés (diceNum)
            }).ToList()).ToList();
            return JsonConvert.SerializeObject(new { secret = set.BonusIsSecret, items = set.Items?.Where(x => x != null).Select(x => x.Id).ToArray() ?? new int[0], effects = list });
        }

        /// <summary>Relit les bonus d'une panoplie en base (après une annulation depuis l'historique).</summary>
        public static void ReloadItemSet(int setId)
        {
            var set = ItemManager.Instance.TryGetItemSetTemplate((uint)setId);
            if (set == null)
                throw new Exception("panoplie introuvable");
            set.EffectsBin = Database.Query<ItemSetTemplate>(ItemSetTemplateRelator.FetchQuery + " WHERE Id=@0", setId).First().EffectsBin;
        }

        public static void SetItemSetEffects(int setId, string json)
        {
            var set = ItemManager.Instance.TryGetItemSetTemplate((uint)setId);
            if (set == null)
                throw new Exception("panoplie introuvable");
            var result = new List<List<EffectBase>>();
            foreach (var step in JArray.Parse(json))
                result.Add(step.Cast<JObject>().Select(dto =>
                {
                    var value = (int?)dto["value"] ?? 0;
                    return (EffectBase)new EffectDice((EffectsEnum)(int)dto["effectId"], value, value, 0);   // même forme que les bonus d'origine
                }).ToList());
            while (result.Count > 0 && result[result.Count - 1].Count == 0)
                result.RemoveAt(result.Count - 1);
            set.Effects = result;
            set.EffectsBin = result.ToBinary();   // relit aussi Effects depuis le binaire
            Database.Execute("UPDATE items_sets SET EffectsBin=@0 WHERE Id=@1", set.EffectsBin, setId);
        }

        public static string MakeCopy()
        {
            var stamp = DateTime.Now.ToString("yyyyMMddHHmm");
            Database.Execute($"CREATE TABLE admin_copy_{stamp}_c AS SELECT Id, Name, AccountId, Experience, Kamas, MapId FROM characters");
            Database.Execute($"CREATE TABLE admin_copy_{stamp}_i AS SELECT Id, OwnerId, ItemId, Stack, SerializedEffects FROM characters_items");
            Database.Execute($"CREATE TABLE admin_copy_{stamp}_b AS SELECT Id, OwnerAccountId, ItemId, Stack, SerializedEffects FROM accounts_items_bank");
            Database.Execute($"CREATE TABLE admin_copy_{stamp}_a AS SELECT Id, BankKamas FROM accounts");
            var copies = Database.Fetch<string>("SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME LIKE 'admin\\_copy\\_%\\_c' ORDER BY TABLE_NAME DESC");
            foreach (var old in copies.Skip(14))
            {
                var prefix = old.Substring(0, old.Length - 2);
                foreach (var suffix in new[] { "_c", "_i", "_b", "_a" })
                    Database.Execute($"DROP TABLE IF EXISTS {prefix}{suffix}");
            }
            return stamp;
        }

        /* ------------------------------------------------------------------ outils */

        private static void Copy<T>(T from, T to)
        {
            if (from == null)
                return;
            foreach (var prop in from.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!prop.CanRead || !prop.CanWrite || prop.GetIndexParameters().Length > 0 || prop.GetSetMethod() == null)
                    continue;
                if (prop.GetCustomAttributes(true).Any(a => a.GetType().Name == "IgnoreAttribute"))
                    continue;
                try
                {
                    prop.SetValue(to, prop.GetValue(from));
                }
                catch
                {
                    // propriété calculée : ignorée
                }
            }
        }
    }
}
