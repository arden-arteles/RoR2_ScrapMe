## the mod

If you're a mod dev and there's one item you really hate for its interactions with a character
(i.e. Eclipse Lite), but don't want to write auto-scrap code, use this mod's API.

If you're a player and really hate how one item interacts with a character, but don't want to
blacklist it for every character, this is the mod for you.

### how to use (player)

Instructions should appear in the config file after first launch.
Currently, it's recommended to use RiskOfOptions to configure the mod, but this is not required.

Here's a quick guide:
1. Click the button to create an entry for the current character.
2. In the characters section, enter in the item ids you want banned.
3. Click "Apply" in the main section.

The config should be applied to your current run immediately, but won't apply to your current inventory at the moment.

### how to use (dev)

For mod devs, add a dependency on ScrapMe, then call `ScrapMe.ScrapMe.plugin.SetBans(nameToken, [itemTokens])`,
ideally with `RoR2Application.OnLoad`, or from `Start()`.

Don't worry about having to clean config when a broken item stops being broken, this API
is separate from the user config, just remove the item ban and you'll be able to 
pick up the item as normal.

I'm not sure if there are ways in which items get added to inventories that sidestep the
calls I've seen so far, so bring it up on the discord if it becomes an issue.

## evil schemes

* [√] Allow end-users to apply their own bans via config.
* [ ] Config do-over to use `CharacterBody.name` for consistency reasons. Breaking change.
* [.] Add ability to adjust user item bans mid-game.
  * [ ] Add ability to change bans via console command.
  * [.] Add RiskOfOptions support.
    * [ ] Add config auto-apply.
    * [ ] Add scrapping newly-banned items in inventory when applying config.
* [ ] Add auto-scrapping items when changing characters, i.e. Artifact of Metamorphosis.

## thanks

* toast: the amazing and awesome icon
* ror2 modcord: being patient and helpful
* score: preparing me for the inevitable chogath problem
