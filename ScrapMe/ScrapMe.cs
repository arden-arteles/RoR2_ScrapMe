using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using RoR2;

[assembly: HG.Reflection.SearchableAttribute.OptIn] // necessary for console commands
namespace ScrapMe
{

    /// <summary>
    /// Main plugin class. Named so you can invoke a using directive on the namespace.
    /// </summary>
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.Gorakh.ItemQualities", BepInDependency.DependencyFlags.SoftDependency)]
    public class ScrapMe : BaseUnityPlugin
    {
        /// singleton
        public static ScrapMe plugin;
        
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "not_score";
        public const string PluginName = "ScrapMe";
        public const string PluginVersion = "0.3.0";
    #pragma warning restore CS1591 // Missing XML comment for publicly visible type or member        
        
        
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

        private void OnLoad()
        {
            PresetBans(); // load dev bans first to leverage the quality compat update
            configManager.Load(); // in here now, to let every character and item load first
            RiskOfOptionsCompat.InitConfigMenu();
        }

        /// <summary>
        /// Gets the developer-set bans for a given character.
        /// After changing these bans, make sure to call <see cref="QualityCompat.SetQualityVariantBans(BodyIndex)"/>
        /// </summary>
        /// <param name="bodyIndex">Name of the body's prefab.</param>
        /// <returns>HashSet for performing set operations on, or null if the index provided was None</returns>
        public HashSet<ItemIndex> GetDevBans(BodyIndex bodyIndex) => bodyIndex == BodyIndex.None ? null : bans.dev[bodyIndex];
        
        internal void PresetBans()
        {
            BanRecords.presets.ForEach(preset => preset.Resolve());
        }

        internal readonly BanRecords bans = new();
    }
}
