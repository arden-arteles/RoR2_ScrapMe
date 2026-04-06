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
        
        var bodyName = Utils.GetPrefabNameFromClone(body.name);
        if (bodyName == null) return; // should not happen but go my nullcheck

        var itemBans = Utils.GetBans([
                ScrapMe.plugin.devItemBans[bodyName],
                ScrapMe.plugin.userItemBans[bodyName]
            ], [
        
        ]);
        if (itemBans == null || itemBans.Count == 0) return; // if there aren't bans
        
        var pickupState = __instance.pickup;
        // pickupState.pickupIndex guaranteed never null
        
        var pickupDef = PickupCatalog.GetPickupDef(pickupState.pickupIndex);
        if (pickupDef == null || pickupDef.nameToken == null) return;
        
        var itemName = Utils.GetPrefabNameFromItem(pickupDef.internalName);
        // realistically should not be null
        
        Log.Debug($"{bodyName} picking up {itemName}");
        
        PickupDef newPickup = Utils.GetReplacementPickup(pickupDef, itemBans);
        if (newPickup == null) return;
        
        Log.Info($"Replacing {itemName} with {ScrapMe.GetPrefabNameFromItem(newPickup.internalName)}");
        __instance.pickup = new UniquePickup(newPickup.pickupIndex);
    }
    
    // patch 2: when character enters stage, check inventory for offending items,
    // and scrap them.
    
}