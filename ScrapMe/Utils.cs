using System.Collections.Generic;
using System.Linq;
using RoR2;

namespace ScrapMe;

public static class Utils
{
    public static HashSet<string> GetBans(IEnumerable<HashSet<string>> bans, IEnumerable<HashSet<string>> unbans = null)
    {
        HashSet<string> filters = [];
        if (bans == null) return null;
        foreach (var banSet in bans)
        {
            if (banSet == null) continue;
            filters.UnionWith(banSet);
        }
        if (unbans == null) return filters;
        
        foreach (var unbanSet in unbans)
        {
            if (unbanSet == null) continue;
            filters.ExceptWith(unbanSet);
        }
        return filters;
    }

    public static string GetPrefabNameFromItem(string itemName)
    {
        return itemName.Substring(itemName.IndexOf("Index.") + 6);
    }
    
    public static PickupDef GetReplacementPickup(PickupDef pickupDef, HashSet<string> itemBans)
    {
        var itemName = GetPrefabNameFromItem(pickupDef.internalName);
        
        if (!itemBans.Contains(itemName)) return null; // item is fine
        
        // item needs to be replaced, depending on the tier
        PickupDef newPickup = null;
        
        if (pickupDef.itemIndex == ItemIndex.None)
        {
            if (pickupDef.coinValue > 0) return null; // lunar coin
            if (pickupDef.droneIndex != DroneIndex.None) return null;
            if (pickupDef.artifactIndex != ArtifactIndex.None) return null; // ???? why are these pickups
            if (pickupDef.equipmentIndex != EquipmentIndex.None) return null; // could do equipment swap but idrc
            Log.Error($"Undefined behavior for pickup {pickupDef.internalName}");
            return null;
        }
        
        switch (pickupDef.itemTier)
        {
            case ItemTier.Tier1:
            case ItemTier.Tier2:
            case ItemTier.Tier3:
            case ItemTier.Boss:
                newPickup = PickupCatalog.GetPickupDef(
                    PickupCatalog.FindScrapIndexForItemTier(pickupDef.itemTier)
                );
                break;
            case ItemTier.Lunar:
                newPickup = PickupCatalog.GetPickupDef(
                    PickupCatalog.FindPickupIndex(ItemCatalog.FindItemIndex("Pearl"))
                );
                // this is what happens when you "scrap" a lunar.
                // except it auto-scraps so no irradiant pearl for you
                break;
            case ItemTier.VoidTier1:
                newPickup = PickupCatalog.GetPickupDef(
                    PickupCatalog.FindScrapIndexForItemTier(ItemTier.Tier1)
                );
                break;
            case ItemTier.VoidTier2:
                newPickup = PickupCatalog.GetPickupDef(
                    PickupCatalog.FindScrapIndexForItemTier(ItemTier.Tier2)
                );
                break;
            case ItemTier.VoidTier3:
                newPickup = PickupCatalog.GetPickupDef(
                    PickupCatalog.FindScrapIndexForItemTier(ItemTier.Tier3)
                );
                break;
            case ItemTier.VoidBoss:
                newPickup = PickupCatalog.GetPickupDef(
                    PickupCatalog.FindScrapIndexForItemTier(ItemTier.Boss)
                );
                break;
            default:
                Log.Error($"Failed to find appropriate replacement for item {pickupDef.internalName}");
                return null;
            // was thinking of full interrupting here but idk what that would have side effects on
            // so nothing happens instead
            // __runOriginal = false;
            // you just will not be able to pick up the item
            // break;
        }

        return newPickup;
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
}