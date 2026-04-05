using System.Collections.Generic;
using HarmonyLib;
using RoR2;

namespace ScrapMe;

internal static class RoR2Patches
{
    [HarmonyPatch(typeof(GenericPickupController), "AttemptGrant")]
    [HarmonyPrefix]
    public static void ReplaceWithScrapIfBannedForChar(
        CharacterBody body,
        GenericPickupController __instance,
        bool __runOriginal
    )
    {
        if (!__runOriginal || __instance == null || body == null) return;
        // Log.Info($"Body {body.name}");
        // var name = body.name.Substring(0,body.name.LastIndexOf("(Clone)"));
        var bodyName = ScrapMe.GetPrefabNameFromClone(body.name);
        
        HashSet<string> itemBans = new();
        
        //if (!plugin.itemBans.TryGetValue(body.baseNameToken, out var bannedItems) || bannedItems == null) return;
        if (ScrapMe.plugin.devItemBans.TryGetValue(bodyName, out var dBans) && dBans != null)
        {
            itemBans.UnionWith(dBans);
        }

        if (ScrapMe.plugin.userItemBans.TryGetValue(bodyName, out var uBans) && uBans != null)
        {
            itemBans.UnionWith(uBans);
        }
        
        var pickupState = __instance.pickup;
        // pickupState.pickupIndex guaranteed never null
        var pickupDef = PickupCatalog.GetPickupDef(pickupState.pickupIndex);
        if (pickupDef == null || pickupDef.nameToken == null) return;
        var itemName = ScrapMe.GetPrefabNameFromItem(pickupDef.internalName);
        Log.Debug($"{bodyName} picking up {itemName}");
        
        if (itemBans.Count == 0) return; // if there aren't bans
        
        PickupDef newPickup = null; // explicit init because i'm from C++
        if (!itemBans.Contains(itemName)) return;
        // item needs to be replaced, depending on the tier
        
        if (pickupDef.itemIndex == ItemIndex.None)
        {
            if (pickupDef.coinValue > 0) return; // lunar coin
            if (pickupDef.droneIndex != DroneIndex.None) return;
            if (pickupDef.artifactIndex != ArtifactIndex.None) return; // ???? why are these pickups
            if (pickupDef.equipmentIndex != EquipmentIndex.None) return; // could do equipment swap but idrc
            Log.Error($"Undefined behavior for pickup {pickupDef.internalName}");
            return;
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
                return;
            // was thinking of full interrupting here but idk what that would have side effects on
            // so nothing happens instead
            // __runOriginal = false;
            // you just will not be able to pick up the item
            // break;
        }
        if (newPickup == null)
        {
            Log.Error($"Replacement item for {pickupDef.internalName} resolved null, not replacing");
            return;
        }
        Log.Info($"Replacing {itemName} with {ScrapMe.GetPrefabNameFromItem(newPickup.internalName)}");
        __instance.pickup = new UniquePickup(newPickup.pickupIndex);
    }
    
    // patch 2: when character enters stage, check inventory for offending items,
    // and scrap them.
    
}