using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using RoR2;

[assembly: HG.Reflection.SearchableAttribute.OptIn] // necessary for console commands
namespace ScrapMe
{

    // This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    
    public class ScrapMe : BaseUnityPlugin
    {
        /// singleton
        public static ScrapMe plugin;
        // why are these not project constants
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "not_score";
        public const string PluginName = "ScrapMe";
        public const string PluginVersion = "0.1.0";
        
        /*
        // failed attempt at impl config here
        // leaving this in here because i want halloween graveyard in april
        internal class ConfigHolder
        {
            public readonly ConfigEntry<string[]> changedBodies;
            public readonly Dictionary<string, ConfigEntry<string[]>> bannedItems = new();
            public ConfigHolder()
            {
                changedBodies = plugin.Config.Bind<string[]>(
                    "General",
                    "Patched Bodies",
                    [],
                    "Bodies to be patched. Used for runtime config binding. Bodies not in here won't be saved in config."
                );
                foreach (var bodyName in changedBodies.Value)
                {
                    bannedItems[bodyName] = plugin.Config.Bind<string[]>(
                        "Item Bans",
                        $"{bodyName}_BANS",
                        [],
                        $"Items to auto-scrap for {bodyName}"
                    );
                }
            }

            public void Load()
            {
                // get new bodies
                
                // bind new config
                
                // release old config
                
                
                plugin.itemBans = new Dictionary<string, HashSet<string>>();
                foreach (var entry in bannedItems)
                {
                    plugin.itemBans[entry.Key] = new(entry.Value.Value);
                }
            }

            public void Save()
            {
                var newBodies = new HashSet<string>(plugin.itemBans.Keys);
                changedBodies.Value = [..newBodies];
                foreach (var bodyName in newBodies)
                {
                    bannedItems[bodyName].Value = [..plugin.itemBans[bodyName]];
                }
                // clean up empty
                foreach (var bodyName in bannedItems.Keys)
                {
                    if (!newBodies.Contains(bodyName))
                    {
                        plugin.Config.Remove(bannedItems[bodyName].Definition);
                    }
                }
            }
        }

        internal ConfigHolder config;
        */

        internal class ConfigHolder
        {
            public readonly ConfigEntry<string> charNameTokens = plugin.Config.Bind(
                "! Main",
                "Characters to Change",
                "",
                "Characters to look for, comma-separated. Must be as name tokens, i.e. COMMANDO_BODY_NAME."
            );
            public readonly Dictionary<string, ConfigEntry<string>> itemBans = new();
            

            public void BindBody(string body)
            {
                if (itemBans.ContainsKey(body)) return; // skip work, body is already bound
                itemBans[body] = plugin.Config.Bind(
                    "Characters",
                    $"{body}_ItemBans",
                    "",
                    $"Items to auto-scrap for {body}, comma-separated, using name tokens, i.e. ITEM_HEALINGPOTION_NAME."
                );
            }
            public void Load()
            {
                // get new bodies
                var bodies = new HashSet<string>(charNameTokens.Value.Split(",")
                    .Select(b => b.Trim())
                    .Where(b => !string.IsNullOrEmpty(b))
                );
                // cleanup old binds and ban entries, try to be minimal
                foreach (var entry in itemBans.Values)
                {
                    if (bodies.Contains(entry.Definition.Key)) continue;
                    plugin.Config.Remove(entry.Definition);
                    plugin.userItemBans.Remove(entry.Definition.Key);
                }
                // add binds
                foreach (var body in bodies)
                {
                    BindBody(body);
                    HashSet<string> items = new(itemBans[body].Value.Split(",")
                        .Select(b => b.Trim())
                        .Where(b => !string.IsNullOrEmpty(b))
                    );
                    plugin.userItemBans[body] = items;
                }
            }

            public void Save()
            {
                // TODO
                // update charNameTokens
                charNameTokens.Value = plugin.userItemBans.Keys.Join();
                
                // update binds
                // no cleanup necessary
            }
        }

        internal ConfigHolder config;
        
        internal void Awake()
        {
            // singleton
            plugin = this;
            // log init
            Log.Init(Logger);

            var harmony = new Harmony(PluginGUID);
            
            harmony.PatchAll(typeof(RoR2Patches));

            config = new();
            config.Load();
        }

        public HashSet<string> GetBans(string bodyNameToken)
        {
            if (!devItemBans.ContainsKey(bodyNameToken)) 
                devItemBans[bodyNameToken] = new();
            return devItemBans[bodyNameToken];
        }

        public void SetBans(string bodyNameToken, IEnumerable<string> bans)
        {
            devItemBans[bodyNameToken] = new(bans);
        }
        
        internal void Start()
        {
            // Log.Info("we made it lesgo");
            // SetBans("COMMANDO_BODY_NAME", ["ITEM_HEALINGPOTION_NAME", "ITEM_SEED_NAME", "ITEM_BARRIERONOVERHEAL_NAME", "ITEM_PARENTEGG_NAME"]);
            // power elixir leeching seed aegis and planula
        }
        
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
                if (!__runOriginal || __instance == null || body == null || body.baseNameToken == null) return;

                HashSet<string> itemBans = new();
                
                //if (!plugin.itemBans.TryGetValue(body.baseNameToken, out var bannedItems) || bannedItems == null) return;
                if (plugin.devItemBans.TryGetValue(body.baseNameToken, out var dBans) && dBans != null)
                {
                    itemBans.UnionWith(dBans);
                }

                if (plugin.userItemBans.TryGetValue(body.baseNameToken, out var uBans) && uBans != null)
                {
                    itemBans.UnionWith(uBans);
                }

                if (itemBans.Count == 0) return; // if there aren't bans
                
                var pickupState = __instance.pickup;
                // pickupState.pickupIndex guaranteed never null
                var pickupDef = PickupCatalog.GetPickupDef(pickupState.pickupIndex);
                if (pickupDef == null || pickupDef.nameToken == null) return;
                
                PickupDef newPickup = null; // explicit init because i'm from C++
                if (!itemBans.Contains(pickupDef.nameToken)) return;
                // item needs to be replaced, depending on the tier
                
                if (pickupDef.itemIndex == ItemIndex.None)
                {
                    if (pickupDef.coinValue > 0) return; // lunar coin
                    if (pickupDef.droneIndex != DroneIndex.None) return;
                    if (pickupDef.artifactIndex != ArtifactIndex.None) return; // ???? why are these pickups
                    if (pickupDef.equipmentIndex != EquipmentIndex.None) return; // could do equipment swap but idrc
                    Log.Error($"Undefined behavior for pickup {pickupDef.nameToken}");
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
                        Log.Error($"Failed to find appropriate replacement for item {pickupDef.nameToken}");
                        return;
                    // was thinking of full interrupting here but idk what that would have side effects on
                    // so nothing happens instead
                    // __runOriginal = false;
                    // you just will not be able to pick up the item
                    // break;
                }
                if (newPickup == null)
                {
                    Log.Error($"Replacement item for {pickupDef.nameToken} resolved null, not replacing");
                    return;
                }
                Log.Info($"Replacing {pickupDef.nameToken} with {newPickup.nameToken}");
                __instance.pickup = new UniquePickup(newPickup.pickupIndex);
            }
            
            // patch 2: when character enters stage, check inventory for offending items,
            // and scrap them.
            
        }

        /// <summary>
        /// Apply your necessary auto-scrap rules here.
        /// </summary>
        internal Dictionary<string, HashSet<string>> devItemBans = new();

        internal Dictionary<string, HashSet<string>> userItemBans = new();
    }
}
