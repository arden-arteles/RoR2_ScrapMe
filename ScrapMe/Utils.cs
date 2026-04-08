using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RoR2;

namespace ScrapMe;

public static class Utils
{
    [NotNull]
    public static HashSet<ItemIndex> GetBans(BodyIndex bodyIndex, bool withQuality = true)
    {
        if (bodyIndex == BodyIndex.None) return [];
        var bans = ScrapMe.plugin.devItemBans[bodyIndex]
            .Union(ScrapMe.plugin.userItemBans[bodyIndex]);
        if (withQuality)
        {
            bans = bans.Union(ScrapMe.plugin.qualityBans[bodyIndex]);
        }
        return bans.Except(ScrapMe.plugin.userItemUnbans[bodyIndex]).ToHashSet();
    }

    public static string GetPrefabNameFromItem(string itemName)
    {
        return itemName.Substring(itemName.IndexOf("Index.") + 6);
    }

    private static Dictionary<ItemTier, ItemIndex> scrapLookup = [];
    
    public static ItemIndex GetScrapForTier(ItemTier tier)
    {
        if (!scrapLookup.ContainsKey(tier))
        {
            switch (tier)
            {
                case ItemTier.Lunar:
                    // this is what happens when you "scrap" a lunar.
                    // except it auto-scraps so no irradiant pearl for you
                    scrapLookup[tier] = ItemCatalog.FindItemIndex("Pearl");
                    break;
                case ItemTier.Tier1:
                case ItemTier.VoidTier1:
                    scrapLookup[tier] = ItemCatalog.FindItemIndex("ScrapWhite");
                    break;
                case ItemTier.Tier2:
                case ItemTier.VoidTier2:
                    scrapLookup[tier] = ItemCatalog.FindItemIndex("ScrapGreen");
                    break;
                case ItemTier.Tier3:
                case ItemTier.VoidTier3:
                    scrapLookup[tier] = ItemCatalog.FindItemIndex("ScrapRed");
                    break;
                case ItemTier.Boss:
                case ItemTier.VoidBoss:
                    scrapLookup[tier] = ItemCatalog.FindItemIndex("ScrapYellow");
                    break;
                default:
                    Log.Error($"Failed to find appropriate replacement for item in tier {ItemTierCatalog.GetItemTierDef(tier).name}");
                    return ItemIndex.Count;
            }
        }

        return scrapLookup[tier];
    }

    
    
    public static ItemIndex GetReplacementItem(ItemIndex item)
    {
        var scrap = GetScrapForTier(ItemCatalog.GetItemDef(item).tier);
        return QualityCompat.CarryQualityToNewItem(item, scrap);
    }
    
    public static string GetPrefabNameFromClone(string cloneName)
    {
        return cloneName.Substring(0, cloneName.LastIndexOf("(Clone)"));
    }
    
    public static string SanitizePrefabName(string prefabName)
    {
        // TODO
        return prefabName;
    }

    public static void CheckInventory(CharacterBody body)
    {
        if (body == null) return;
        
        var bans = GetBans(body.bodyIndex);
        foreach (var itemBan in bans)
        {
            var itemCount = body.inventory.GetItemCountPermanent(itemBan);
            if (itemCount > 0)
            {
                body.inventory.RemoveItemPermanent(itemBan, itemCount);
                body.inventory.GiveItemPermanent(GetReplacementItem(itemBan), itemCount);
            }

            var tempItemCount = body.inventory.GetTempItemRawValue(itemBan);
            if (tempItemCount > 0)
            {
                body.inventory.RemoveItemTemp(itemBan, (int) Math.Ceiling(tempItemCount));
                body.inventory.GiveItemTemp(GetReplacementItem(itemBan), tempItemCount);
            }
        }
    }
}