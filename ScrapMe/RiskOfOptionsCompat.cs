using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using HarmonyLib;
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

    /*[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void SomeMethodThatRequireTheDependencyToBeHere()
    {
        // stuff that require the dependency to be loaded
    }*/
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static void InitConfigMenu()
    {
        if (!enabled) return;
        ModSettingsManager.AddOption(new StringInputFieldOption(ScrapMe.plugin.config.charNames));
        ModSettingsManager.AddOption(
            new GenericButtonOption("Create Entry for Current Character", "! Main", CreateCurrentBodyEntry)
        );
        // ModSettingsManager.AddOption(new StringInputFieldOption(ScrapMe.plugin.config.copyHolder));
        // ModSettingsManager.AddOption(new GenericButtonOption("Copy Character Name Token", "! Main", CopyCharacterNames));
        // ModSettingsManager.AddOption(new GenericButtonOption("Save Config", "! Main", ScrapMe.plugin.config.Save));
        ModSettingsManager.AddOption(
            new GenericButtonOption("Apply Changes", "! Main", ScrapMe.plugin.config.Load)
        );
        // yes this initiates a full config load. yes i'm too lazy to find a better solution. deal with it
        // TODO create config entry for current char
        
    }

    /*[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    static void CopyCharacterNames()
    {
        var names = RoR2.PlayerCharacterMasterController.instances
            .Select(c => ScrapMe.GetPrefabNameFromClone(c.body.name));
        ScrapMe.plugin.config.copyHolder.Value = names.Join();
    }*/
    
    internal static readonly HashSet<string> mappedEntries = [];

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static void CreateCurrentBodyEntry()
    {
        var objNames = RoR2.PlayerCharacterMasterController.instances.Select(b => b.body.name);
        foreach (var name in objNames) {
            var prefabName = ScrapMe.GetPrefabNameFromClone(name);
            if (ScrapMe.plugin.config.BindBody(prefabName)) // also creates an area to input. baller
            {
                ScrapMe.plugin.config.charNames.Value += $",{prefabName}";
                Log.Info($"Created entry for body {prefabName}");
            }
            else
            {
                Log.Info($"Already had entry for {prefabName}, skipping");
            }
        }
    }
    
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static void CreateBanEntry(ConfigEntry<string> associated)
    {
        if (!enabled) return;
        
        var name = associated.Definition.Key;
        if (mappedEntries.Contains(name)) return;
        
        ModSettingsManager.AddOption(new StringInputFieldOption(associated));
        mappedEntries.Add(name);
    }

    internal static void CreateUnbanEntry(ConfigEntry<string> associated)
    {
        if (!enabled) return;
        var name = associated.Definition.Key;
        if (mappedEntries.Contains(name)) return;
        ModSettingsManager.AddOption(new StringInputFieldOption(associated));
        mappedEntries.Add(name);
    }
}