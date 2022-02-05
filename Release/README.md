## SniperClassic Beta
BETA RELEASE: Everything (including multiplayer) works fine, but the animations/visuals need polish.
When the mod comes out of beta, Sniper and his alt skills will be locked by default, but there will be a config option to automatically unlock everything.

The Sniper is a long-range DPS who uses powerful charged attacks to take down big targets. His Spotter drone allows him to analyze enemies, causing heavy attacks to transfer their damage to nearby enemies when hit.
[![](https://i.imgur.com/PgrW5ED.png)]()
[![](https://i.imgur.com/n8K5qWj.png)]()


## IMPORTANT: READ THIS
- All players need the mod in multiplayer.
- Scope input settings can be found in the config.

## To-do list
Full Release Goals:

- Unlockables and Mastery/Grandmastery skin
- Model
	- Fix ragdoll
	- Polish anims
		- Add select anim
		- Slightly reduce bounce on sprint
		- Add vertical aiming
	- Gun models
		- Make alt gun models show up in the Character Select
		- Reduce the size of Mark's gun
		- Change the gun model for Hard Impact to reflect it being a heavy explosive sniper
- HUD
	- Fix reload HUD showing through command/scrapper menus
	- Fix laser sight positioning
- VFX
	- Change spotter targeting indicator, make it stay still when locked on.
- General
	- Fix hoof/capacitor/armor plate displays
	- Tweak skill icons
	- Fix certain items obstructing scope view
	- Add item displays for anniversary update items

## Installation
Place SniperClassic.dll in /Risk of Rain 2/BepInEx/plugins/
Settings can be changed in BepInEx/config/com.Moffein.SniperClassic.cfg

## Credits
Moffein - Code
LucidInceptor - Models
Timeweeper - Chiropractor, Animations, Code, finishing up the fucker
Rob - Animations, Code
DestroyedClone - Alt Spotter aggro code
Swuff - Character select icon outlines, animation help
Papazach - Skill Icons

Big thanks to Rob and Timesweeper for helping me with all the code-related stuff to get Lucid's models in-game, along with doing the animations! You guys are lifesavers! Also huge thanks to Lucid for all the models!

Sounds taken from Dirty Bomb, TF2, L4D2, and https://www.youtube.com/watch?v=aoBWUs8poYU

## Changelog
or 1.0.0 maybe
`0.10.0`

- Added mastery and grandmastery skin
- Added their achievements
- Fixed up and added remaining item displays
- Fixed up animations
  - Learned the arcane knowledge of aiming up and down animations
  - Set up little placeholder css animation and sound
- feels like a lot more, and I could elaborate, but that pretty much sums it -timesweeper
- [maybe unlock and m1 achievements, maybe skils++ on m2]

`0.9.8`

- Spotted enemies now have a red glow.
- Added config option to sort Sniper among the vanilla survivors based on his planned unlock condition (disabled by default).

`0.9.7`

- Increased Mark damage from 320% -> 340%

This should put it at around the same spot it was when Sniper's damage was 15, since it's mainly just Snipe that was too strong damagewise.

`0.9.6`

- Increased base damage from 13 -> 14.
- Hard Impact now has sweetspot falloff (need to direct hit to get full damage).
- Increased Hard Impact damage from 500% -> 540%.
- Increased Hard Impact base radius from 4m -> 5m.

Most people agreed that 13 damage was too low so I'm bumping it up back to 14. Experimenting with sweetspot falloff on Hard Impact, since it bugged me that  you dont actually have to go for direct hits to deal full damage with it. Since this should make it a lot harder to use at range, it's getting a buff to its damage and blast radius to assist it. The damage buff essentially just makes it do the same damage it used to do when Sniper had 15 damage.

`0.9.5`

- Lowered base damage 15 -> 13.
I've been feeling Sniper's damage has been a bit too high for some time, so I'm experimenting with lowering it slightly. This change will still allow him to 1shot trash enemies on Stage 1, but he'll no longer be able to 1shot golems without items/levelups.

`0.9.4`

- Spotter: DISRUPT can now distract elite horde teleporter bosses.

`0.9.3`

- Fixed Spotter: DISRUPT only distracting bosses and not regular enemies. Now is only restricted from distracting teleporter bosses (isBoss), instead of all boss-type enemies (isChampion).
- Added vanish ending text.

`0.9.2`

- Fixed Steady Aim charging faster when reloading while scoped.

`0.9.1`

- Spotter: DISRUPT no longer distracts bosses.

`0.9.0`

Somewhat of an experimental update, trying out lots of different changes. The general idea is that I want to make Sniper's alt skills have a larger impact on how he plays. I think his base kit felt good to use, but it was pretty much the only way to play Sniper. With these changes and additions, I hope that Sniper will have much more replay value.
Let me know how these feel, and if they make Sniper too strong/weak, or if they make him awkward/better to play. Ping me, open an issue on the github, DM me. Every bit of feedback helps!

New special: Spotter: DISRUPT
- Analyze an enemy for 7 seconds, distracting nearby enemies and stunning them for 7x100% damage.
- Scepter version doubles the damage, AOE size, and enemy distraction range.

This skill should be familiar if you've played the original Starstorm mod for Risk of Rain 1. Instead of being a passive debuff targeted towards a single enemy, this Spotter mode actively damages enemies while drawing nearby enemies towards it. Special thanks to DestroyedClone for lending me the code for it!

General:
- Fixed Visions of Heresy permanently replacing Sniper's reload.
- Removed Smoke Grenade. I tried a lot of numbers, but I feel the skill is too flawed at its core to work.

Steady Aim:
- Steady Aim charge multiplier reduced 4x -> 3x
- Fixed scope overlay on ultrawide resolutions.

Steady Aim's max charge multiplier has been reduced to compensate for all of Sniper's primaries getting buffs to their uncharged damage. Overall compared to before, Sniper will be doing more damage when unscoped, while charged shots will do less damage in total than before (but still enough to 1shot a Golem with a fully charged Feedback + Snipe on stage 1).

Primaries:
- Reworked Hard Impact
	- Damage increased 480% -> 500%
	- Scope penalty removed.
	- Bolt pull time reduced 0.75s -> 0.6s
	- Charge time reduced 4.5s -> 4s
	- Reload bar length reduced 1.4s -> 1.0s
	- Now fires an explosive projectile that detonates on impact.
	- Blast radius scales with distance traveled.

Hard Impact was pretty much the same as Snipe, but with different numbers. The movement penalty didn't matter much since you could just backflip away. With these changes, I hope to give the skill a unique identity of its own, while still keeping the heavy and hard-hitting feeling of before.
	
- Snipe damage increased 360% -> 430%
- Added config to slow down Snipe's reload bar.

A common complaint about Snipe was that it couldn't 1shot trash on stage 1. Additionally, the whole reason Hard Impact existed originally was because Snipe's reload bar was too fast for some people to hit. Instead of trying to have 2 copies of the same skill but with different numbers, I want the default Snipe to be the only 'sniping' skill, and I want it to feel right to use. With the increased damage, Snipe should be able to 1shot beetles and lemurians on stage 1. To account for people who can't hit the fast reload timing, I've added a config option to slow it down. By default it will use RoR1's fast reload timing.

- Mark damage increased 300% -> 320%
- Mark reload bar length decreased 1.6s -> 1.2s
- Recoil reduced 90% while scoped.

To account for the lower value of fully charged shots, Mark has had a slight damage increase and a decent reload time decrease to compensate. I was hesitant to buff the damage too much due to feedback I received early-on about the skill being too strong compared to Sniper's other options, but let me know how this feels since I spent the least time testing with this.

Combat Training:
- Reduced cooldown to 4s, since a ground-based roll is inherently less valuable than Military Training's aerial backflip that allows Sniper to completely evade danger.

Spotter: FEEDBACK
- Manually recalling the Spotter now refunds skill cooldown based on the cooldown of Feedback.
- Increased skill cooldown 7s -> 10s.

`0.8.0`

- Removed Trickshot.
- Turns out I accidentally included Sniper's WiP Smoke Grenade skill so I guess it's a feature now.
- Fixed a load error related to the Smoke Grenade.

`0.7.1`

- Buffed Mark damage from 290% to 300%
- Fixed Mark stunning on every shot
- Disabled Spotter's move speed slow when the King's Kombat Arena gamemode is active. Get the mod at https://thunderstore.io/package/kinggrinyov/KingKombatArena/ . This nerf can be disabled in the config.
Next update will be a skill update to give Sniper more variety to his loadouts and playstyle. Current plan is to scrap Trickshot due to the skill being generally underwhelming, and to rework Hard Impact into a projectile-based skill like the Kraber from Overwatch. Other stuff are planned but are still in the process of being developed.

`0.7.0`

- Added Alt Secondary: Trickshot. Currently missing anims.
	- The damage bonus from using this skill can stack!
- Steady Aim now stuns regardless of charge percentage.
- Spotter skill cooldown reduced from 10s -> 7s. Chain Lightning cooldown remains the same.
- Added Spotter: OVERLOAD (Scepter Skill)
	- Doubles chain lightning damage and range.
- Removed debug text when using Visions of Heresy.
- Improved Vengeance AI. Enemy Snipers will shoot a lot more often now.

`0.6.8`

- Updated for Anniversary Hotfix update.

`0.6.7`

- Fixed survivor select position.

`0.6.6`

- Moved back to R2API.

`0.6.5`

- Remembered to include dll this time

`0.6.4`

- Fixed Hard Impact disabling jumping even after unscoping.
- Fixed missing Spotter debuff icon.

`0.6.3`

- Fixed item displays.
	- Anniversary update item displays will come later.

`0.6.2`

- BUG: Item displays currently broken. Will fix soon.
- Mod moved from R2API to EnigmaticThunder.
- Charge no longer affects force for all primaries.
- Snipe force increased 500 -> 2000 (same as MUL-T Rebar)
- Hard Impact force increased 750 -> 2500
- Mark force increased 250 -> 1000
- Combat Training duration reduced 0.5 -> 0.4
- Combat Training initial speed increased 5 -> 6
- Spotter Chain Lightning damage increased from 40% TOTAL -> 50% TOTAL
- Spotter armor debuff increased -20 -> -25

`0.6.1`

- Moved Utility auto-reload behind an Authority check (just some under-the-hood networking stuff).
- Fixed incorrect version numbering.

`0.6.0`

- Rewrote code for all primaries.
	- This fixes the bug where the skill icon would get stuck as the reload icon in multiplayer.
	- Reloading no longer cancels sprint (but firing your weapon still does).
	- Holding down the fire button after reloading will make you fire as soon as your next shot is ready.
	- Reload bar no longer lingers when frozen mid-reload.
	- Scope charge now properly stays at 0 while reloading instead of appearing to gain charge before getting reset.
- Improved reload sound syncing in multiplayer.
- The reload bar now turns red when performing a bad reload on all primaries.
- Snipe bolt pull time increased from 0.4s to 0.5s
- Mark empty ping sound now plays in multiplayer.
- Mark damage reduced 300% -> 280%
- Heavy Snipe renamed to Hard Impact.
- Hard Impact bolt pull time increased from 0.4s to 0.75s
- Duck and Cover renamed to Combat Training.
- Increased default scroll wheel zoom speed from 20 -> 30
- Charged shot sound now plays in multiplayer.

`0.5.4`

- Visions of Heresy is now affected by the scope.
	- Scoping in will make Visions cost 2 ammo, but deal double damage regardless of charge.
	- Scope Charge multiplier is applied on top of the scope damage multiplier of Visions.
	
`0.5.3`

- Tweaked skill icons.
- Firing animation tweaks.
- Fixed scope movement penalties not applying in multiplayer.
- Roll renamed to Duck and Cover.
- Duck and Cover distance reduced. Re-enabled trimping on roll (was broken due to some unused testing stuff from pre-release).
- Backflip renamed to Military Training.
- Backflip is now the default utility.
- Spotter: Feedback proc coefficient increased 0.33 -> 0.5

`0.5.2`

- Fixed Crowbar/Rejuvenation Rack/Elemental Band displays
- Moved Resonance Disk display to Sniper's shoulder.

`0.5.1`

- Adjusted/added a lot of animations
- Added spotter idle animations
- Added new Backflip utility
- Added new bullet tracers
- Added work in progress ragdoll

`0.5.0`

- Release
