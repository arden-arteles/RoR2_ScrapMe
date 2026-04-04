## the mod

If you're a mod dev and there's one item you really hate for its interactions with a character
(i.e. Eclipse Lite), but don't want to write auto-scrap code, use this mod's API.

If you're a player and really hate how one item interacts with a character, but don't want to
blacklist it entirely, this is also the mod for you.

### how to use (player)

Instructions should appear in the config file after first launch.
Adding item bans will take several launches. There isn't a great way around this without allowing the
config file to explode in size the moment you launch the game.

### how to use (dev)

For mod devs, add a dependency on ScrapMe, then call `ScrapMe.ScrapMe.plugin.SetBans(nameToken, [itemTokens])`,
ideally with `RoR2Application.OnLoad`, or from `Start()`.

Don't worry about having to clean config when a broken item stops being broken, this API
is separate from the user config, just remove the item ban and you'll be able to 
pick up the item as normal.

I'm not sure if there are ways in which items get added to inventories that sidestep the
calls I've seen so far, so bring it up on the discord if it becomes an issue.

## evil schemes

* [X] Allow end-users to apply their own bans via config.
* [ ] Add ability to adjust user item bans mid-game.
  * [ ] Add ability to change bans via console command.
  * [ ] Add RiskOfOptions support.
* [ ] Add auto-scrapping items when changing characters, i.e. Artifact of Metamorphosis.

## thanks

* toast: the amazing and awesome icon
* ror2 modcord: being patient and helpful
* score: idk i just wanted to put him in here
