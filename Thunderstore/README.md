## the mod

If you're a mod dev and there's one item you really hate for its interactions with a character
(i.e. Eclipse Lite), but don't want to write auto-scrap code, use this mod's API.

If you're a player and really hate how one item interacts with a character, but don't want to
blacklist it for every character, this is the mod for you.

### how to use (player)

Instructions should appear in the config file after first launch.
Currently, it's recommended to use RiskOfOptions to configure the mod, but is not required.

Here's a quick guide:
1. Click the button to create an entry for the current character.
2. In the characters section, enter in the item IDs you want banned.
  * With Quality, to ban Rare Leeching Seed, you would add `SeedRare`, for example
3. Click "Apply" in the main section.

The config should be applied to your current run immediately, but won't apply to your current inventory at the moment.

### how to use (dev)

For mod devs, add a dependency on ScrapMe, then modify the set returned by `ScrapMe.plugin.GetDevBans(BodyIndex)`.

Don't worry about having to clean config when a broken item stops being broken, this API
is separate from the user config, just remove the item ban and you'll be able to 
pick up the item as normal.

I'm not sure if there are ways in which items get added to inventories that sidestep the
calls I've seen so far, so bring it up on the discord if it becomes an issue.

## evil schemes

* Config:
  * [X] Re-do config to use `CharacterBody.name` rather than name tokens.
  * [X] Add ability to change item bans mid-game.
    * [ ] Add DebugToolkit compat.
    * [X] Add RiskOfOptions compat.
      * [ ] Add config auto-apply.
    * [ ] Scrub inventory on changes to item bans.
  * [X] Add ability to unban items.
* Implementation
  * [ ] Add ability to ban particular item combos.
  * [ ] Scrub inventory on morphing characters.
  * [ ] Change item pickup notification for clarity a la void item corruption.
* Compatibility:
  * [X] Quality
  * [X] Void item corruption exclusion
  * [ ] Multiplayer (might work, but likely server-authority right now)

## thanks

* toast: the amazing and awesome icon
* ror2 modcord: being patient and helpful
* score: explaining why my soft compat wasn't a soft compat
