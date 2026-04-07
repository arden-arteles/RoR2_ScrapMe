using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.Options;

namespace ScrapMe;

public static class RiskOfOptionsCompat
{
    private static bool? _enabled;

    public static bool enabled {
        get {
            if (_enabled == null) {
                _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");
            }
            return (bool)_enabled;
        }
    }
    
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static void InitConfigMenu()
    {
        if (!enabled) return;
        ModSettingsManager.AddOption(new StringInputFieldOption(ScrapMe.plugin.configManager.charNames));
        ModSettingsManager.AddOption(
            new GenericButtonOption("Create Entry for Current Character", "! Main", CreateCurrentBodyEntry)
        );
        ModSettingsManager.AddOption(
            new GenericButtonOption("Apply Changes", "! Main", ScrapMe.plugin.configManager.Load)
        );
        // yes this initiates a full configManager load. yes i'm too lazy to find a better solution. deal with it
        
    }
    
    internal static readonly HashSet<string> mappedEntries = [];

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static void CreateCurrentBodyEntry()
    {
        var objNames = RoR2.PlayerCharacterMasterController.instances.Select(b => b.body.name);
        foreach (var name in objNames) {
            var prefabName = Utils.GetPrefabNameFromClone(name);
            if (ScrapMe.plugin.configManager.BindBody(prefabName)) // also creates an area to input. baller
            {
                ScrapMe.plugin.configManager.charNames.Value += $",{prefabName}";
                Log.Info($"Created entry for body {prefabName}");
            }
            else
            {
                Log.Info($"Already had entry for {prefabName}, skipping");
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static void CreateConfigEntry(ConfigEntry<string> associated)
    {
        if (!enabled) return;

        var name = associated.Definition.Key;
        if (mappedEntries.Contains(name)) return;
        
        ModSettingsManager.AddOption(new StringInputFieldOption(associated));
        mappedEntries.Add(name);

    }
}