using System.Collections.Generic;
using System.Linq;
using RoR2;

namespace ScrapMe;

/// <summary>
/// Holder class for tracking character bans. Implemented to move towards multiplayer compat.
/// </summary>
public class BanRecords
{
    
    /// <summary>
    /// Internal class. DO NOT MAKE THESE YOURSELF.
    /// </summary>
    public class CharItemRecord
    {
        internal Dictionary<BodyIndex, HashSet<ItemIndex>> record = new();

        private HashSet<ItemIndex> GetValue(BodyIndex key)
        {
            if (!record.ContainsKey(key)) record[key] = [];
            return record[key];
        }
        
        /// <summary>
        /// Gets bans for a particular character.
        /// </summary>
        /// <param name="index">BodyIndex of the character.</param>
        public HashSet<ItemIndex> this[BodyIndex index]
        {
            get => GetValue(index);
            set => record[index] = value;
        }
    }

    /// <summary>
    /// Developer item bans. Feel free to modify these.
    /// </summary>
    public CharItemRecord dev = new();
    internal CharItemRecord quality = new();
    internal CharItemRecord user = new();
    internal CharItemRecord unbans = new();

    /// <summary>
    /// Gets ALL item bans for a particular character.
    /// </summary>
    /// <param name="index">BodyIndex of the character.</param>
    /// <param name="withQuality">Whether to account for Quality variants.</param>
    /// <returns></returns>
    public HashSet<ItemIndex> All(BodyIndex index, bool withQuality = true)
    {
        var ret = dev[index].Union(user[index]);
        if (withQuality)
        {
            ret = ret.Union(quality[index]);
        }
        return ret.Except(unbans[index]).ToHashSet();
    }
    
    internal IEnumerable<BodyIndex> MappedBodies => dev.record.Keys
        .Union(quality.record.Keys)
        .Union(user.record.Keys)
        .Union(unbans.record.Keys);

    internal class PresetBans
    {
        private readonly string charName;
        private readonly HashSet<string> itemNames;
        
        public PresetBans(string bodyName, IEnumerable<string> itemNames)
        {
            this.charName = bodyName;
            this.itemNames = new HashSet<string>(itemNames);
        }
        
        public void Resolve()
        {
            if (charName == null) return;
            var body = BodyCatalog.FindBodyIndex(charName);
            if (body == BodyIndex.None)
            {
                Log.Warning($"Couldn't find body matching {charName}, skipping");
                return;
            }

            HashSet<ItemIndex> items = [];
            foreach (var item in itemNames)
            {
                var itemIdx = ItemCatalog.FindItemIndex(item);
                if (itemIdx == ItemIndex.None)
                {
                    Log.Message($"Couldn't find item matching {item}, skipping");
                    continue;
                }

                items.Add(itemIdx);
            }
            ScrapMe.plugin.bans.dev[body].UnionWith(items);
            QualityCompat.SetQualityVariantBans(body);
        }
    }

    internal static readonly List<PresetBans> presets =
    [
        new PresetBans("RobRavagerBody", ["JumpBoost", "JumpDamageStrike"]),
        new PresetBans("RobBelmontBody", ["BarrierOnCooldown", "JumpBoost", "JumpDamageStrike"])
    ];
}