### Changelog
## Version 2.0.0
- Another **major** re-programming for this mod...
- Added two new reworks for **Chance Doll**
<br>- Rework **#1** for near similar functionality
<br>- Rework **#2** for a more unique functionality
- Added an actual rework for **Seed of Life**
<br>- Rework **#1** for letting Seed of Life revive all allies when you revive yourself instead of just yourself
- Added a config value to disable the mod Logs to not print
- Miscellaneous code optimizations and stuff
- Hopefully even less bugs than usual

### Common Tier

* **Warped Echo**
<br>- Extended default duration
<br>- VFX sync with set duration
* **Chronic Expansion**
<br>- Item display now tallies kill streak <sub><sup> **_( Toggleable via Config )_** </sub></sup>
* **Knockback Fin**
<br>- Removed VFX on dealing **Airborne** damage
<br>- Colored damage on **Airborne** damage <sub><sup> **_( Toggleable via Config )_** </sub></sup>
<br>- Fixed **NRE** from the Meridian's environmental hazards hitting players
<br>- Fixed cursed config leaking into Flying Fin description
<br>- Removed knock CD to provide more increased damage uptime <sub><sup> **_( Configurable Soon )_** </sub></sup>
* **Bolstering Lantern**
<br>- Fixed **NRE** from Meridian's environmental hazards hitting players
<br>- Fixed not properly disabling the original vanilla effect
* **Antler Shield**
<br>- Uses``RecalculateAPI`` now <sub><sup> **_( Rework #2 Soon )_** </sub></sup>

### Uncommon Tier

* **Chance Doll**
<br>- Two new reworks for **Chance Doll**
* **Sale Star**
<br>- Nothing new, sorry!
* **Unstable Transmitter**
<br>- Config option for **Hemorrhage** instead of **Bleed** <sub><sup> **_( Toggleable via Config )_** </sub></sup>
<br>- Fixed it not actually activating at the set **health percentage**
* **Noxious Thorn**
<br>- Nothing new, sorry!
* **Old War Stealthkit**
<br>- Nerfed default value of cleansing **2 debuffs** at base to **1** instead
<br>- Base number of **debuff** cleansed now configurable
<br>- Stacking number of **debuff** cleansed now configurable

### Legendary Tier

* **Growth Nectar**
<br>- Now actually affects the **Armor** stat
* **War Bonds**
<br>- VFX on free unlock now configurable
<br>- Properly made the VFX only activate on **interactables** that cost money
* **Ben's Raincoat**
<br>- Nothing new, sorry!

### Equipment

* **Seed of Life**
<br>- New rework to add more consistency
* **Seed of Life (Consumed)**
<br>- Changed name and rewrite description

## Version 1.4.1
- Noxious Thorn Rework #1 - Now the VFX is properly networked for multiplayer!
- Added a new null check for whenever Frost Relic's particle prefab gets modified
<br>_( This is due to ``RiskyMod`` disabling the bubble, and causing the mod to stop load times if you didn't config ``RiskyMod`` to disable removing the bubble)_

## Version 1.4.0
- New Noxious Thorn rework - Can change the type of DoT it applies via config
- Not entirely tested for networking, have fun until I get nagged about this
- Slight wording adjustment to Unstable Transmitter to be more relative to Tri-Tip and Shatterspleen.

## Version 1.3.1
- Fixed Unstable Transmitter activating from receiving any damage
- Adjusted the damage applied for Unstable Transmitter to do higher damage once again from pre-reprogram
- Just a reminder to delete your config file to make a new one

## Version 1.3.0
- Complete reprogram of this entire mod - Hopefully even more compatible!
- New Sale Star rework
- Slightly adjusted the default stats on some items for general balance
- Overhauled the config screen, and added a few more adjustable stats for items
- Replaced the VFX of War Bonds to instead apply to all free purchases instead - Configurable in next update
- Hopefully less bugs in general, please report any new ones to me
  
## Version 1.2.1
- Modified README.md to instead include alternative reworks at the bottom of respective category
- Fixed Smoldering Lantern Alt rework spamming NREs to enemies that have minions

## Version 1.2.0
- Two different reworks for Bolstering Lantern, one fire, and one ally focused
- Fixed Unstable Transmitter still making you invincible.

## Version 1.1.1
- Warped Echo re-reworked, after finding out it doesn't kill you when taking enough damage for OSP
- New default values for Antler Shield
- Entirely new README.md

## Version 1.1.0
- Growth Nectar rework
- Uploaded the wrong DLL for Version 1.0.0, meaning some of the items weren't functional
- Functional Old War Stealthkit buff
- Unstable Transmitter functional
- Actual descriptions for some of the items that were only in the new DLL
- New Default Values for Misc

## Version 1.0.0
- Initial release
