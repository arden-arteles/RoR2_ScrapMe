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
        
        var itemBans = ScrapMe.plugin.bans.All(body.bodyIndex);
        if (itemBans.Count == 0) return; // if there aren't bans
        
        var pickupState = __instance.pickup;
        // pickupState.pickupIndex guaranteed never null
        
        var pickupDef = PickupCatalog.GetPickupDef(pickupState.pickupIndex);
        if (pickupDef == null || pickupDef.itemIndex == ItemIndex.None) return;
        
        Log.Debug($"{body.name} picking up {pickupDef.internalName}");
        
        if (!itemBans.Contains(pickupDef.itemIndex)) return;
        if (body.inventory == null) return; // why was i not checking this
        if (Utils.IsCorrespondingVoidInInv(pickupDef.itemIndex, body.inventory)) return;
        
        var newItem = Utils.GetReplacementItem(pickupDef.itemIndex);
        if (newItem == ItemIndex.None) return;
        var newDef = ItemCatalog.GetItemDef(newItem);
        var newPickupIdx = PickupCatalog.FindPickupIndex(newDef.itemIndex);
        
        Log.Info($"Replacing {pickupDef.internalName} with {newDef.name} (index {newPickupIdx})");
        __instance.pickup = new UniquePickup
        {
            pickupIndex = newPickupIdx,
            decayValue = __instance.pickup.decayValue,
            upgradeValue = __instance.pickup.upgradeValue
        };
        // CharacterMasterNotificationQueue.TransformationType
        // TODO Character notification
        // SendTransformNotification(component, result.takenItem.itemIndex, result.givenItem.itemIndex, (TransformationType)result.transformationType);
    }
    
    // TODO Scrub on character transform
}