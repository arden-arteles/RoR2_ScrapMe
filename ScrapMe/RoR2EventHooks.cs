using RoR2;

namespace ScrapMe;


/// <summary>
/// Contains all event hooks. Registers them on Register()
/// </summary>
public static class RoR2EventHooks
{
    /// <summary>
    /// Registers events. Called in Awake().
    /// </summary>
    public static void Register()
    {
        RoR2Application.onLoad += OnLoad;
        Inventory.ItemTransformation.onItemTransformedServerGlobal += HandleItemTransform;
        
    }

    /// <summary>
    /// Hooks <see cref="Inventory.ItemTransformation.onItemTransformedServerGlobal"/>
    /// to catch and re-transform items that are banned.
    /// </summary>
    /// <param name="result"></param>
    public static void HandleItemTransform(Inventory.ItemTransformation.TryTransformResult result)
    {
        var inv = result.inventory;
        if (inv == null) return;
        
        var gotItem = result.givenItem.itemIndex;
        if (gotItem == ItemIndex.None) return;
        
        if (Utils.IsCorrespondingVoidInInv(gotItem, inv)) return;
            // check if item can corrupt first since i can early return and avoid GetComponent
        
        var charMaster = inv.GetComponent<CharacterMaster>();
        if (charMaster == null) return; // just in case
        var body = charMaster.GetBody();
        if (body == null) return; // why is this possible
        if (!ScrapMe.plugin.bans.All(body.bodyIndex).Contains(gotItem)) 
            return;
            // item isn't banned
        // item IS banned and ISN'T being corrupted, let's transform it
        var newItem = Utils.GetReplacementItem(gotItem);
        if (newItem == gotItem) return; // no replacement
        if (newItem == ItemIndex.None)
        {
            Log.Warning($"Replacing transformed item {gotItem} with no item");
        }
        var trans = new Inventory.ItemTransformation
        {
            originalItemIndex = gotItem,
            newItemIndex = newItem,
            maxToTransform = int.MaxValue,
            transformationType = ItemTransformationTypeIndex.None // it's technically a scrapping interaction
        };
        if (!trans.TryTransform(inv, out var _))
        {
            Log.Warning($"Failed to replace transformed item {gotItem} with {newItem}");
        }
    }
    
    /// <summary>
    /// Hooks <see cref="RoR2Application.onLoad"/> instead of using Start() like a normal person
    /// </summary>
    public static void OnLoad()
    {
        var plugin = ScrapMe.plugin;
        plugin.PresetBans(); // load dev bans first to leverage the quality compat update
        plugin.configManager.Load(); // in here now, to let every character and item load first
        RiskOfOptionsCompat.InitConfigMenu();
    }
}