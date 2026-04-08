using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using RoR2;

[assembly: HG.Reflection.SearchableAttribute.OptIn] // necessary for console commands
namespace ScrapMe
{

    // This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.Gorakh.ItemQualities", BepInDependency.DependencyFlags.SoftDependency)]
    public class ScrapMe : BaseUnityPlugin
    {
        /// singleton
        public static ScrapMe plugin;
        // why are these not project constants
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "not_score";
        public const string PluginName = "ScrapMe";
        public const string PluginVersion = "0.3.0";
        
        
        
        internal ConfigManager configManager;
        
        internal void Awake()
        {
            // singleton
            plugin = this;
            // log init
            Log.Init(Logger);

            var harmony = new Harmony(PluginGUID);
            harmony.PatchAll(typeof(RoR2Patches));

            configManager = new();

            RoR2Application.onLoad += OnLoad;
        }

        public void OnLoad()
        {
            configManager.Load(); // in here now. lets every mod load first
            RiskOfOptionsCompat.InitConfigMenu();
            PresetBans();
        }

        /// <summary>
        /// Gets the developer-set bans for a given character.
        /// </summary>
        /// <param name="bodyIndex">Name of the body's prefab.</param>
        /// <returns>HashSet for performing set operations on</returns>
        public HashSet<ItemIndex> GetDevBans(BodyIndex bodyIndex)
        {
            if (!devItemBans.ContainsKey(bodyIndex))
            {
                devItemBans[bodyIndex] = new();
                mappedBodies.Add(bodyIndex);
            }

            return devItemBans[bodyIndex];
        }

        /// <summary>
        /// Set the bans of a particular character.
        /// </summary>
        /// <param name="bodyIndex">Name of the body's prefab.</param>
        /// <param name="bans">Collection of PickupDef.internalName's for items to be banned.</param>
        public void SetDevBans(BodyIndex bodyIndex, IEnumerable<ItemIndex> bans)
        {
            devItemBans[bodyIndex] = new(bans);
            mappedBodies.Add(bodyIndex);
        }

        public void SetDevBans(string bodyName, IEnumerable<string> bans)
        {
            var bodyIndex = BodyCatalog.FindBodyIndex(bodyName);
            if (bodyIndex == BodyIndex.None)
            {
                Log.Warning($"Couldn't add bans for body {bodyName}; wasn't able to resolve name in catalog");
                return;
            }

            HashSet<ItemIndex> items = new();
            foreach (var ban in bans)
            {
                var itemIndex = ItemCatalog.FindItemIndex(ban);
                if (itemIndex == ItemIndex.None)
                {
                    Log.Info($"Couldn't find definition for item {ban}, skipping");
                }
                else
                {
                    items.Add(itemIndex);
                }
            }
            SetDevBans(bodyIndex, items);
        }

        internal void PresetBans()
        {
            SetDevBans("RobBelmontBody",["BarrierOnCooldown","JumpBoost","JumpDamageStrike"]);
            SetDevBans("RobRavagerBody",["JumpBoost","JumpDamageStrike"]);
        }
        
        internal readonly Dictionary<BodyIndex, HashSet<ItemIndex>> devItemBans = new();

        internal readonly Dictionary<BodyIndex, HashSet<ItemIndex>> userItemBans = new();

        internal readonly Dictionary<BodyIndex, HashSet<ItemIndex>> userItemUnbans = new();

        internal readonly Dictionary<BodyIndex, HashSet<ItemIndex>> qualityBans = new();
        
        internal readonly HashSet<BodyIndex> mappedBodies = [];
    }
}
