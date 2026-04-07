using System.Runtime.CompilerServices;
using ItemQualities;
using RoR2;

namespace ScrapMe;

public static class QualityCompat
{
    private static bool? _enabled;

    public static bool enabled {
        get {
            if (_enabled == null) {
                _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Gorakh.ItemQualities");
            }
            return (bool)_enabled;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static void SetQualityVariantBans()
    {
        if (!enabled) return;
        ScrapMe.plugin.qualityBans.Clear();
        foreach (var body in ScrapMe.plugin.mappedBodies)
        {
            SetQualityVariantBans(body);
        }
    }
    
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static void SetQualityVariantBans(BodyIndex bodyIndex)
    {
        if (!enabled) return;
        foreach (var itemIndex in Utils.GetBans(bodyIndex, false))
        {
            for (var i = QualityCatalog.GetQualityTier(itemIndex) + 1; i < QualityTier.Count; i++)
            {
                var variant = QualityCatalog.GetItemIndexOfQuality(itemIndex, i);
                if (variant == ItemIndex.None) continue;
                if (!ScrapMe.plugin.qualityBans.ContainsKey(bodyIndex))
                {
                    ScrapMe.plugin.qualityBans[bodyIndex] = [];
                }
                ScrapMe.plugin.qualityBans[bodyIndex].Add(variant);
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
}