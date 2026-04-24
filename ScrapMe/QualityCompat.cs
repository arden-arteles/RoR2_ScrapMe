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

    /// <summary>
    /// Returns a quality variant of newItem with oldItem's quality.
    /// 
    /// </summary>
    /// <param name="oldItem"></param>
    /// <param name="newItem"></param>
    /// <returns>DEFAULT: <see cref="newItem"/></returns>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static ItemIndex CarryQualityToNewItem(ItemIndex oldItem, ItemIndex newItem)
    {
        if (!enabled) return newItem;
        if (QualityCatalog.GetQualityTier(oldItem) == QualityCatalog.GetQualityTier(newItem)) return newItem;
        return QualityCatalog.GetItemIndexOfQuality(newItem, QualityCatalog.GetQualityTier(oldItem));
    }
    
    /// <summary>
    /// Gets all quality variants of an item.
    /// </summary>
    /// <param name="baseItemIndex">Index of an item</param>
    /// <returns>
    /// All items that are in the quality group.
    /// DEFAULT: <see cref="baseItemIndex"/>
    /// </returns>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static IEnumerable<ItemIndex> GetQualityVariants(ItemIndex baseItemIndex)
    {
        if (!enabled) return [baseItemIndex];
        
        var group = QualityCatalog.GetItemQualityGroup(QualityCatalog.FindItemQualityGroupIndex(baseItemIndex));
        return [group.BaseItemIndex, group.UncommonItemIndex, group.RareItemIndex, group.EpicItemIndex, group.LegendaryItemIndex];
    }
}