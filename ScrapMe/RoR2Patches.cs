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
        
        // var itemName = Utils.GetPrefabNameFromItem(pickupDef.internalName);
        // realistically should not be null
        
        Log.Debug($"{body.name} picking up {pickupDef.internalName}");
        
        if (!itemBans.Contains(pickupDef.itemIndex)) return;

        /*var voidItem = ContagiousItemManager.GetTransformedItemIndex(pickupDef.itemIndex);
        if (voidItem != ItemIndex.None && body.inventory.GetItemCountEffective(voidItem) > 0) return;*/
        // check if the player has corresponding void items. plural.
        var voids = QualityCompat.GetCorrespondingVoids(pickupDef.itemIndex);
        foreach (var voidItem in voids)
        {
            if (voidItem == ItemIndex.None) continue;
            if (body.inventory.GetItemCountEffective(voidItem) > 0) return;
        }
        
        var newItem = Utils.GetReplacementItem(pickupDef.itemIndex);
        if (newItem == ItemIndex.None) return;
        var newDef = ItemCatalog.GetItemDef(newItem);
        var newPickupIdx = PickupCatalog.FindPickupIndex(newDef.itemIndex);
        // var newPickup = 
        
        Log.Info($"Replacing {pickupDef.internalName} with {newDef.name} (index {newPickupIdx})");
        __instance.pickup = new UniquePickup
        {
            pickupIndex = newPickupIdx,
            decayValue = __instance.pickup.decayValue,
            upgradeValue = __instance.pickup.upgradeValue
        };
    }
    
    // TODO [0.4] Apply on character transform
    
    
}