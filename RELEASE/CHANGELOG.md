`1.5.8`

- Updated RU TL (Thanks marklecarde!)

`1.5.7`

- Added Japanese translation (Thanks punch!)

`1.5.6`

- Fixed Spotter not working on dedicated servers.

`1.5.5`

- Updated CN TL (Thanks Meteorite1014!)

`1.5.4`

- Updated language to refer to Reload/Steady Aim bonus damage as multipliers instead of percentages for clarity.

`1.5.3`

- Updated BR TL.

`1.5.2`

- Added Brazilian Portuguese translation (Thanks Kauzok!)

`1.5.1`

- Fixed Spotter drone having no cooldown if you respawn via Dios and use it on an enemy you were previously spotting.

`1.5.0`

- Fixed Scroll Zoom (Thanks yekoc!)
- Added mod icon to RiskOfOptions.

`1.4.0`

- Spotter: FEEDBACK now only triggers if the attacker is a Sniper. (Can be reverted in config)

`1.3.2`

- Updated the localization files.

`1.3.1`

- Added Korean translation (Thanks CaffeinePain!)

`1.3.0`

- RiskOfOptions is now a softdependency instead of a harddependency.
- AssetBundle/Soundbank are no longer embedded in the DLL. This will reduce RAM usage.
- Enabled extra mastery skin by default in the config.
	- If updating, you will need to manually change your config to get it.

`1.2.11`

- Added French translation (Thanks FyreBW!)
- Disabled Lysate Cell interaction by default.
	- Existing configs need to be manually updated.

`1.2.10`

- Fixed EmoteAPI skeleton applying root motion on anims.

`1.2.9`

- Fixed EmoteAPI skeleton having weird legs.

`1.2.8`

- Increased Weakpoint damage from 40% -> 50%
- Added Ukrainian translation (Thanks Damglador!)

`1.2.7`

- Fixed Mark ping sound not playing online(?)

`1.2.6`

- Remembered to set CachedName field in SurvivorDef. Hopefully this will fix Eclipse progress not saving.

`1.2.5`

- Fixed Snipe always causing Spotter to trigger even when not fully-charged.

`1.2.4`

- Scope FOV is no longer affected by Frost Relic.

`1.2.3`

- Fixed Hard Impact being unable to headshot online.
- Fully-charged shots now will always trigger the Spotter.

`1.2.2`

- Fixed Steady Aim not saving the 3rdperson setting.

`1.2.1`

- Added missing Risk of Options dependency to Thunderstore manifest.

`1.2.0`

- CustomEmotesAPI support.
- Added Risk of Options support to some settings.
	- This may reset your keybinds, but you can now change them in-game.
	
- Steady Aim
	- All primaries can now headshot for an extra 40% TOTAL damage when fully charged.
		- Not considered a crit, still affected by crit chance.
		- Extra charge from Backup Mags is considered as overcharging your weapon beyond full charge.
	- Now remembers zoom level between stages.
	
	*Headshots are restricted to full-charge so that there needs to be an explicit choice between fast uncharged shots or big charged headshots, which helps differentiate him from Railgunner's quickscope-focused gameplay.*

- Hard Impact
	- Reduced charge time from 4s -> 3s
	- Reduced bullet drop by 50% (increased antigravity coefficient from 0.5 -> 0.75)
	
	*Being a projectile attack, lacking piercing, and harder headshots are already enough downsides for this skill, so the extra charge time isn't necessary. Lower bullet drop should make this more reliable at long range. Let me know if there's any enemies that are unheadshottable.*
	
- Spotter: FEEDBACK
	- Changed indicator visuals.
	- Indicator no longer rotates when Spotter is on a target.
	- Fixed target display size not scaling with screen resolution.
	- Target display now is located on the target's Weak Point.
	- Reduced damage transfer from 60% -> 50%
	- Lysate Cells reduce recharge time by 15%.
		- Stacks like Fuel Cells.
		- Multiplies off of attack speed multipliers.
		- Can be disabled in config.
	
	*Reverted to the damage used in earlier versions since headshot damage gets transferred.*

`1.1.3`

- Added a check to make sure that the mod loads after Inferno loads.

`1.1.2`

- Grandmastery skin can now unlock on Inferno.

`1.1.1`

- Fixed Spotter not showing up in the Character Select screen (bug introduced in 1.1.0)
- networked the backflip's stun effect for clients
- fixed item display initialization for other mods looking to add custom item displays
- added one (1) item display for laser scope

`1.1.0`

- Fixed Spotter not showing up for clients in multiplayer.
	- Bug was introduced by the DLC update.
- Increased visor glow to be similar to Railgunner.

`1.0.13`

- Fixed ClassicItems issues?

`1.0.12`

- Tweaked scope UI visuals. (Thanks TimeSweeper!)

`1.0.11`

- Added extra null check to item display code for mod compatibility.
- Added support for ClassicItems Scepter (untested).

`1.0.10`

- More language fixes. This should fix the German translation not working.

`1.0.9`

- Fixed incompatibility with ShowDeathCause(?)
	- German translation appears to be bugged due to this fix. Need to figure out what's causing it.

`1.0.8`

- Added DE translation (Thanks MojoJMP!)

`1.0.7`

- Added CN translation (Thanks Edge-R!)
- Fixed Spanish translation.
- Added extra authority checks to Spotter to reduce console spam.
	- Let me know if this causes any bugs.

`1.0.6`

- Fixed ItemDisplays (Thanks Timesweeper!)
	- DLC itemdisplays to come later
- Fixed buff icons.

`1.0.5`

- Added weakpoint to Sniper.

`1.0.4`
- Fixed for DLC update.
	- ItemDisplays are currently broken.
	- Changed the way language tokens are loaded.
		- For now, adding new languages will require changes to the code, instead of simply being able to create a new file+folder. Hoping to fix this later.
		
- Mark
	- Added config option to show ammo while sprinting.
	
- Steady Aim
	- Max charge per Backup Mag reduced from +100% -> +50%
	
	*Steady Aim was scaling a bit too hard off of a single white item.*
	
- Steady Aim Zoom Input Settings
	- Is now firstperson by default, can be toggled to thirdperson by pressing V (button can be changed in config).
		- The toggle button feels slightly unresponsive and I have no clue why.
	- Removed CSGO Zoom, Scroll Wheel Zoom, and Zoom In/Out Button settings.
	
	*Had to remove a bunch of settings since the update changed how camera stuff works, which broke the gradual FOV change settings. You now swap between thirdperson/firstperson by pressing a button to toggle it.*

`1.0.3`
- Added Russian translation (Thanks Noto#1111!)

`1.0.2`
- Fixed BuffDefs missing ScriptableObject names.
- Removed Steady Aim debug text.
- Slightly shortened Steady Aim description (changed "maximum" to "max").

`1.0.1`
- Fixed SkillDefs missing ScriptableObject names.
- readme fixes

`1.0.0`

*Huge thanks to TimeSweeper for coming in and pretty much finishing up everything that needed to be done with regards to animations, skins, and unlocks.*

- General
	- Added unlock requirement.
		- Can be force unlocked in config.
	- Added Mastery skin (Thanks LucidInceptor!)
	- Added Grandmastery skin (Thanks bruh!)
		- Unlocks on Typhoon and difficulities with an equivalent/higher scaling coefficient, along with Eclipse if you don't want to install external difficulty mods.
    - Gun models for alternate skills now appear in character select
    - Animations polished, including aiming up and down
	- Added placeholder character select anim.
		- Proper character select anim coming later, maybe.
    - Added missing item displays, and tweaked exsiting
      - Featuring proper goat hoof!
	- Added Spanish translation (Thanks Anreol!)
	
- Reload
	- New reload bar visuals.
	- Perfect/Good reload zone now increase in size with attack speed.
	- Fixed Reload UI showing over Command/Scrapper menus.
	
- Snipe
	- Now has crosshair bloom when firing.
		- This is purely cosmetic, the shot will always be perfectly accurate.
	
- Hard Impact
	- Bolt pull duration reduced from 0.6s -> 0.5s (same as Snipe)
		- Reload duration remains the same.
	- Now has crosshair bloom when firing.
		- This is purely cosmetic, the shot will always be perfectly accurate.

	*This should help make Hard Impact's shot rhythm feel a little less awkward.*
	
- Mark
	- Damage increased from 340% -> 360%
	- Clip size reduced from 6 -> 5
	- Steady Aim charge is now reset between each shot.
	- Perfect reloads no longer restore Secondary stocks.
	- Reloads now recharge your SPOTTER (50% cooldown for Perfect, 25% for Good)
	- Can now be spamfired with low accuracy (0.33s min duration)
	- Fixed a bug where the skill would be stuck at 0 shots when frozen mid-reload.
	
	*Mark had anti-synergy with SPOTTER: Feedback due to its lower per-shot damage. With this change, Mark can now trigger Feedback more often, but with lower damage than the other primary options. If you coordinate with teammates, you can abuse this to get huge damage if there are other burst characters on your team. Spamfire should help it feel more natural to use, instead of being forced to always wait out the whole 0.5s duration when firing.*
	
- Steady Aim.
	- Fixed anims not being synced online.
	- Fixed camera issues with Frost Relic.
	- Removed cooldown.
	- Backup Mags now increase max charge capacity.
		- +3s charge duration, +2x max charge mult
	- Now only stuns when fully charged.
			
	*Being unable to use the scope without wasting a cooldown felt awkward, and after playtesting with this, I've found that removing the cooldown doesn't actually make too much of a difference balancewise. Backup Mags increasing the max charge capacity also has the side effect of making attack speed more useful due to the increased maximum charge time.*
	
- Military Training (Backflip)
	- Now automatically activates sprint when used.
	- No longer affected by mods that modify Acrid's Leaps.
	- Now stuns nearby enemies.
	
*This effect should help turn this into a setup/combo tool, instead of purely being for repositioning.*
	
- Combat Training (Roll)
	- Cooldown increased from 4s -> 6s
	- Velocity increased 45%
	- Now automatically activates sprint when used.
	- Now gives 1s of invisibility.
	
*Invis + increased distance should hopefully make this better for actually evading attacks.*
	
- Spotter: FEEDBACK
	- Fixed anims not being synced online.
	- Separated skill cooldown from FEEDBACK cooldown.
		- Spotter can always be sent to different enemies, while FEEDBACK has its own cooldown.
	- FEEDBACK recharge is now handled via a debuff like Elemental Bands. (Credits to SOM for the buff icon!)
		- Can be cleansed with Blast Shower.
		- Recharge rate scales with Attack Speed.
	- Activation damage requirement increased from 400% -> 1000%
	- Lightning damage increased 50% -> 60%
	- Lightning range increased 20m -> 30m
	- Increased initial bounce targets from 1 -> 20
		- This was an oversight.
	- Bounces reduced from 5 -> 1
	- Targets per bounce reduced from 20 -> 3
		- Each of the initial 20 lightning targets can chain to up to 3 additional enemies each.
	- Added new Spotting HUD (Credits to TheMysticSword!)
		- Can be disabled in config if it's too obstructive.
	
	*Spotter felt pretty underwhelming for a 10s cooldown skill. Its range was too short to hit Wisps at times, and it would usually accidentally get triggered by other players due to its low activation requirement. Increased activation damage allows Sniper to shoot unscoped shots at spotted enemies without putting Feedback on cooldown, and ensures that only powerful attacks will trigger it. New cooldown behavior is intended to give Sniper a reason to stack attack speed, allowing him to pull off his signature move more often. FEEDBACK's lightning bounces have also been made much more consistent. While its potential range has been reduced, it should now reliably always hit enemies that are close to it.*
	
- SPOTTER: Disrupt
	- Now disabled by default.
		- Can be re-enabled via the Cursed config option (all players need to enable it).
	- Added attack speed scaling to the lightning damage.
	
	*Disrupt was removed because it was fundamentally incompatible with the new Spotter system and didn't have much synergy with Sniper's primaries. It's been left available in the config for those who still want to use it. I feel this would be better-suited for an equipment or a skill on a completely different survivor.*

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