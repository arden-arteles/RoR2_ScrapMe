using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using ItemQualities;
using JetBrains.Annotations;
using RoR2;

namespace ScrapMe;

/// <summary>
/// Container class for compatibility with Quality.
/// </summary>
public class QualityModule
{
    private static bool? _enabled;

    /// <summary>
    /// Whether Quality mod is loaded.
    /// </summary>
    public static bool enabled {
        get {
            if (_enabled == null) {
                _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Gorakh.ItemQualities");
            }
            return (bool)_enabled;
        }
    }

    [CanBeNull] private static QualityModule _instance = null;
    /// <summary>
    /// Instance of the compatibility module. If null, Quality is not loaded.
    /// </summary>
    /// <remarks>
    /// Intent is to invoke methods like <c>QualityModule.instance.SetQualityVariantBans()</c>
    /// </remarks>
    [CanBeNull]
    public static QualityModule Instance
    {
        get
        {
            if (_instance != null && enabled) return _instance;
            if (enabled) _instance = new QualityModule();
            return _instance;
        }
    }

    /// <summary>
    /// Updates the quality variant bans for ALL characters. Avoid using unless absolutely necessary.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public void SetQualityVariantBans()
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
    public void SetQualityVariantBans(BodyIndex bodyIndex)
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
    /// <param name="oldItem">Item to take the quality tier of.</param>
    /// <param name="newItem">Item to apply the quality tier to.</param>
    /// <returns>DEFAULT: <c>newItem</c></returns>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public ItemIndex CarryQualityToNewItem(ItemIndex oldItem, ItemIndex newItem)
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
    /// DEFAULT: <c>[baseItemIndex]</c>
    /// </returns>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public IEnumerable<ItemIndex> GetQualityVariants(ItemIndex baseItemIndex)
    {
        if (!enabled) return [baseItemIndex];
        
        var group = QualityCatalog.GetItemQualityGroup(QualityCatalog.FindItemQualityGroupIndex(baseItemIndex));
        return [group.BaseItemIndex, group.UncommonItemIndex, group.RareItemIndex, group.EpicItemIndex, group.LegendaryItemIndex];
    }
}