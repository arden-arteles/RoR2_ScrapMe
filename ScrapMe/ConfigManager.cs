using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;

namespace ScrapMe;

internal class ConfigManager
        {
            /*public readonly ConfigEntry<string> copyHolder = plugin.Config.Bind(
                "! Main",
                "Copy/Paste Holder",
                "",
                "Copy things from here."
            );*/
            public readonly ConfigEntry<string> charNames = ScrapMe.plugin.Config.Bind(
                "! Main",
                "Characters to Change",
                "",
                "Characters to look for, comma-separated. Must be as prefab names, i.e. CommandoBody."
            );
            public readonly Dictionary<string, ConfigEntry<string>> itemBans = new();
            public readonly Dictionary<string, ConfigEntry<string>> itemUnbans = new();
            public bool BindBody(string body)
            {
                if (itemBans.ContainsKey(body)) return false; // skip work, body is already bound
                var itemBanEntry = ScrapMe.plugin.Config.Bind(
                    "Characters",
                    $"{body}_ItemBans",
                    "",
                    $"Items to auto-scrap for {body}, comma-separated, using prefab names, i.e. HealingPotion."
                );
                itemBans[body] = itemBanEntry;
                RiskOfOptionsCompat.CreateBanEntry(itemBanEntry);
                var itemUnbanEntry = ScrapMe.plugin.Config.Bind(
                    "Characters",
                    $"{body}_ItemUnbans",
                    "",
                    $"Unbanned items for {body}. Use at your own risk; the items were likely banned for a reason."
                );
                
                return true; // work done
            }
            public void Load()
            {
                // get new bodies
                var bodies = new HashSet<string>(charNames.Value.Split(",")
                    .Select(b => b.Trim())
                    .Where(b => !string.IsNullOrEmpty(b))
                );

                // add binds
                foreach (var body in bodies)
                {
                    BindBody(body);
                    HashSet<string> items = new(itemBans[body].Value.Split(",")
                        .Select(b => b.Trim())
                        .Where(b => !string.IsNullOrEmpty(b))
                    );
                    ScrapMe.plugin.userItemBans[body] = items;
                }
            }

            public void Save()
            {
                // TODO
                // update charNames
                charNames.Value = ScrapMe.plugin.userItemBans.Keys.Join();
                // update binds
                foreach (var record in ScrapMe.plugin.userItemBans)
                {
                    itemBans[record.Key].Value = record.Value.Join();
                }
                // no cleanup necessary...
                // because we do it when loading right after
                Load();
            }

            public void Cleanup()
            {
                // TODO
            }
        }