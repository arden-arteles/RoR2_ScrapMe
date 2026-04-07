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
        
        var itemBans = Utils.GetBans(body.bodyIndex);
        if (itemBans.Count == 0) return; // if there aren't bans
        
        var pickupState = __instance.pickup;
        // pickupState.pickupIndex guaranteed never null
        
        var pickupDef = PickupCatalog.GetPickupDef(pickupState.pickupIndex);
        if (pickupDef == null || pickupDef.itemIndex == ItemIndex.None) return;
        
        // var itemName = Utils.GetPrefabNameFromItem(pickupDef.internalName);
        // realistically should not be null
        
        Log.Debug($"{body.name} picking up {pickupDef.internalName}");

        if (!itemBans.Contains(pickupDef.itemIndex)) return;
        
        var newItem = Utils.GetReplacementItem(pickupDef.itemIndex);
        if (newItem == ItemIndex.None) return;
        var newDef = ItemCatalog.GetItemDef(newItem);
        var newPickup = newDef.CreatePickupDef();
        
        Log.Info($"Replacing {pickupDef.internalName} with {newDef.name}");
        __instance.pickup = new UniquePickup(newPickup.pickupIndex);
    }
    
}