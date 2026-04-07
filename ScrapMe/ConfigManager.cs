using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using RoR2;

namespace ScrapMe;

internal class ConfigManager
{
    public readonly ConfigEntry<string> charNames = ScrapMe.plugin.Config.Bind(
        "! Main",
        "Characters to Change",
        "",
        "Characters to look for, comma-separated. Must be as prefab names, i.e. CommandoBody."
    );
    public readonly Dictionary<string, ConfigEntry<string>> itemBans = new();
    public readonly Dictionary<string, ConfigEntry<string>> itemUnbans = new();
    public readonly HashSet<string> mappedChars = [];
    public bool BindBody(string body)
    {
        if (mappedChars.Add(body) == false) return false; // skip work, body is already bound
        var itemBanEntry = ScrapMe.plugin.Config.Bind(
            "Characters",
            $"{body}_ItemBans",
            "",
            $"Items to auto-scrap for {body}, comma-separated, using prefab names, i.e. HealingPotion."
        );
        itemBans[body] = itemBanEntry;
        RiskOfOptionsCompat.CreateConfigEntry(itemBanEntry);
        var itemUnbanEntry = ScrapMe.plugin.Config.Bind(
            "Characters",
            $"{body}_ItemUnbans",
            "",
            $"Unbanned items for {body}. Use at your own risk; the items were likely banned for a reason."
        );
        itemUnbans[body] = itemUnbanEntry;
        RiskOfOptionsCompat.CreateConfigEntry(itemUnbanEntry);
        return true; // work done
    }
    public void Load()
    {
        ScrapMe.plugin.mappedBodies.Clear();
        // get new bodies
        var bodies = charNames.Value.Split(",")
            .Select(b => b.Trim())
            .Where(b => !string.IsNullOrEmpty(b));

        // add binds
        foreach (var bodyName in bodies)
        {
            var bodyIndex = BodyCatalog.FindBodyIndex(bodyName);
            BindBody(bodyName);
            HashSet<string> userBans = new(itemBans[bodyName].Value.Split(",")
                .Select(b => b.Trim())
                .Where(b => !string.IsNullOrEmpty(b))
            );
            ScrapMe.plugin.userItemBans[bodyIndex] = new(userBans.Select(ItemCatalog.FindItemIndex));
            HashSet<string> userUnbans = new(itemUnbans[bodyName].Value.Split(",")
                .Select(b => b.Trim())
                .Where(b => !string.IsNullOrEmpty(b))
            );
            ScrapMe.plugin.userItemUnbans[bodyIndex] = new(userUnbans.Select(ItemCatalog.FindItemIndex));
            ScrapMe.plugin.mappedBodies.Add(bodyIndex);
        }
        QualityCompat.SetQualityVariantBans();
    }

    public void Cleanup()
    {
        foreach (var bodyIndex in ScrapMe.plugin.mappedBodies)
        {
            var bodyName = BodyCatalog.GetBodyName(bodyIndex);
            UnbindBody(bodyName);
        }
    }

    public bool UnbindBody(string bodyName)
    {
        if (RiskOfOptionsCompat.mappedEntries.Contains(bodyName)) return false;
        ScrapMe.plugin.Config.Remove(itemBans[bodyName].Definition);
        ScrapMe.plugin.Config.Remove(itemUnbans[bodyName].Definition);
        return true;
    }
}