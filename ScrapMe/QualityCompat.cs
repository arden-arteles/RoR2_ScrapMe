using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ItemQualities;
using RoR2;

namespace ScrapMe;

/// <summary>
/// Container class for compatibility with Quality.
/// </summary>
public static class QualityCompat
{
    private static bool? _enabled;

    /// <summary>
    /// Whether or not Quality is enabled.
    /// </summary>
    public static bool enabled {
        get {
            if (_enabled == null) {
                _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Gorakh.ItemQualities");
            }
            return (bool)_enabled;
        }
    }

    /// <summary>
    /// Updates the quality variant bans for ALL characters. Avoid using unless absolutely necessary.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void SetQualityVariantBans()
    {
        if (!enabled) return;
        ScrapMe.plugin.bans.quality.record.Clear();
        foreach (var body in ScrapMe.plugin.bans.MappedBodies)
        {
            SetQualityVariantBans(body);
        }
    }
    
    /// <summary>
    /// Updates the quality variant bans for a given character.
    /// </summary>
    /// <param name="bodyIndex">BodyIndex of the character to update.</param>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void SetQualityVariantBans(BodyIndex bodyIndex)
    {
        if (!enabled) return;
        var record = ScrapMe.plugin.bans.quality[bodyIndex];
        record.Clear();
        foreach (var itemIndex in ScrapMe.plugin.bans.All(bodyIndex, false))
        {
            for (var i = QualityCatalog.GetQualityTier(itemIndex) + 1; i < QualityTier.Count; i++)
            {
                var variant = QualityCatalog.GetItemIndexOfQuality(itemIndex, i);
                if (variant == ItemIndex.None) continue;
                /*if (!ScrapMe.plugin.qualityBans.ContainsKey(bodyIndex))
                {
                    ScrapMe.plugin.qualityBans[bodyIndex] = [];
                }*/
                // ScrapMe.plugin.qualityBans[bodyIndex].Add(variant);
                // if (ScrapMe.plugin.bans.unbans[bodyIndex].Contains(variant)) continue;
                    // not necessary since i'm unbanning later in the sequence anyways
                record.Add(variant);
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static ItemIndex CarryQualityToNewItem(ItemIndex oldItem, ItemIndex newItem)
    {
        if (!enabled) return newItem;
        if (QualityCatalog.GetQualityTier(oldItem) == QualityCatalog.GetQualityTier(newItem)) return newItem;
        return QualityCatalog.GetItemIndexOfQuality(newItem, QualityCatalog.GetQualityTier(oldItem));
    }
    
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static IEnumerable<ItemIndex> GetCorrespondingVoids(ItemIndex corruptionCheck)
    {
        var respVoid = RoR2.Items.ContagiousItemManager.GetTransformedItemIndex(corruptionCheck);
        if (respVoid == ItemIndex.None) return [];
        if (!enabled) return [respVoid];
        
        var group = QualityCatalog.GetItemQualityGroup(QualityCatalog.FindItemQualityGroupIndex(respVoid));
        return [group.BaseItemIndex, group.UncommonItemIndex, group.RareItemIndex, group.EpicItemIndex, group.LegendaryItemIndex];
    }
}