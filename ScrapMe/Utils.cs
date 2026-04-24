using System;
using System.Collections.Generic;
using System.Linq;
using RoR2;

namespace ScrapMe;
/// <summary>
/// General utility functions for the plugin.
/// </summary>
public static class Utils
{

    /// <summary>
    /// Gets the name of a prefab from an item.
    /// </summary>
    /// <param name="itemName">Name of the item.</param>
    /// <returns>Name of the prefab.</returns>
    [Obsolete]
    public static string GetPrefabNameFromItem(string itemName)
    {
        return itemName.Substring(itemName.IndexOf("Index.") + 6);
    }

    /// <summary>
    /// Internal caching dictionary for item tiers. DO NOT TOUCH.
    /// </summary>
    private static Dictionary<ItemTier, ItemIndex> scrapLookup = [];
    
    /// <summary>
    /// Gets the correct scrap item for the relevant tier. Should only be called if necessary for logging reasons.
    /// </summary>
    /// <param name="tier"></param>
    /// <returns></returns>
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

    
    /// <summary>
    /// Gets the correct replacement item for a given item, applying all relevant compatibility layers.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static ItemIndex GetReplacementItem(ItemIndex item)
    {
        var scrap = GetScrapForTier(ItemCatalog.GetItemDef(item).tier);
        if (QualityCompat.enabled)
        {
            return QualityCompat.CarryQualityToNewItem(item, scrap);
        }

        return scrap;
    }
    
    /// <summary>
    /// Gets the name of a character prefab from a clone of that prefab.
    /// </summary>
    /// <param name="cloneName"></param>
    /// <returns></returns>
    public static string GetPrefabNameFromClone(string cloneName)
    {
        return cloneName.Substring(0, cloneName.LastIndexOf("(Clone)"));
    }
    
    /// <summary>
    /// Intended to sanitize character prefab names, for the inevitable day someone reports an issue with Cho'Gath.
    /// </summary>
    /// <param name="prefabName"></param>
    /// <returns></returns>
    public static string SanitizePrefabName(string prefabName)
    {
        // TODO upon bug report
        return prefabName;
    }

    /// <summary>
    /// Scrubs the player's inventory of any illegal items. DOES NOT CHECK IF THERE ARE VOID CORRUPTIONS.
    /// </summary>
    /// <param name="body"></param>
    public static void CheckInventory(CharacterBody body)
    {
        if (body == null) return;
        
        var bans = ScrapMe.plugin.bans.All(body.bodyIndex);
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

    /// <summary>
    /// Checks whether there is a corresponding void item in the player's inventory.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="inv"></param>
    /// <returns></returns>
    public static bool IsCorrespondingVoidInInv(ItemIndex item, Inventory inv)
    {
        return GetCorrespondingVoids(item)
            .Any(i => i != ItemIndex.None && inv.GetItemCountEffective(i) > 0);
    }

    internal static IEnumerable<ItemIndex> GetCorrespondingVoids(ItemIndex corruptionCheck)
    {
        var respVoid = RoR2.Items.ContagiousItemManager.GetTransformedItemIndex(corruptionCheck);
        if (respVoid == ItemIndex.None) return [];
        if (QualityCompat.enabled)
            return QualityCompat.GetQualityVariants(respVoid);
        return [respVoid];
    }
}