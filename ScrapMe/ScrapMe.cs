using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using RoR2;

[assembly: HG.Reflection.SearchableAttribute.OptIn] // necessary for console commands
namespace ScrapMe
{

    // This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    public class ScrapMe : BaseUnityPlugin
    {
        /// singleton
        public static ScrapMe plugin;
        // why are these not project constants
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "not_score";
        public const string PluginName = "ScrapMe";
        public const string PluginVersion = "0.2.0";
        
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

        

        public static string GetPrefabNameFromItem(string itemName)
        {
            return itemName.Substring(itemName.IndexOf("Index.") + 6);
        }
        
        internal ConfigManager config;
        
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

            RiskOfOptionsCompat.InitConfigMenu();
        }

        /// <summary>
        /// Gets the bans for a given character.
        /// </summary>
        /// <param name="bodyName">Name of the body's prefab.</param>
        /// <returns></returns>
        public HashSet<string> GetBans(string bodyName)
        {
            if (!devItemBans.ContainsKey(bodyName)) 
                devItemBans[bodyName] = new();
            return devItemBans[bodyName];
        }

        /// <summary>
        /// Set the bans of a particular character.
        /// </summary>
        /// <param name="bodyName">Name of the body's prefab.</param>
        /// <param name="bans">Collection of PickupDef.internalName's for items to be baned.</param>
        public void SetBans(string bodyName, IEnumerable<string> bans)
        {
            devItemBans[bodyName] = new(bans);
        }

        /*[ConCommand(commandName = "scrapme.ban_item", helpText = "Add an item to this character's ban config")]
        public void ConsoleCommandAddBan(ConCommandArgs args)
        {
            
        }*/
        
        
        internal void Start()
        {
            // Log.Info("we made it lesgo");
            // SetBans("COMMANDO_BODY_NAME", ["ITEM_HEALINGPOTION_NAME", "ITEM_SEED_NAME", "ITEM_BARRIERONOVERHEAL_NAME", "ITEM_PARENTEGG_NAME"]);
            // power elixir leeching seed aegis and planula
        }

        /// <summary>
        /// Apply your necessary auto-scrap rules here.
        /// </summary>
        internal Dictionary<string, HashSet<string>> devItemBans = new();

        internal Dictionary<string, HashSet<string>> userItemBans = new();
    }
}
