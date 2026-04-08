## 0.3.0
code Quality improvements

* Internal migration to checking against `ItemIndex`es rather than `string`s at runtime.
* Added some manual bans on dev-side for common item malfunctions.
* `ScrapMe.plugin.SetBans` and `GetBans` have migrated to `SetDevBans` and `GetDevBans` for clarity.
* Added compatibility with Quality.
  * Banning an item with Quality will ban all its superior Quality variants.
* Added the ability to unban items in the config.
  * Item unbans OVERRIDE all otherwise set item bans. Use at your own risk.
  * Item unbans can manually unban certain tiers of item for Quality as you would unban normally, but only unban that tier.
* Banned items that can corrupt into other items in your inventory can be picked up and corrupted.

## 0.2.0
me when i break your config

* Config has been slightly restructured. Your old config won't work anymore, but it should be easier to manage now.
  * Config can no longer self-clean. This may get re-added in the future, but I'm not sure how I would go about it at the moment.
* Added compatibility for RiskOfOptions.
  * RiskOfOptions is NOT required to use this mod, but at the moment, strongly recommended.
  * Auto-apply will be added as a feature if people bug me enough about it.

* Internal: Bodies and items are now referenced by prefab name. Check newly generated config for examples.

## 0.1.0
initial release

* Ability to set item bans via API.
* Ability to set item bans via config.