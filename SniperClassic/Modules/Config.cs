using BepInEx.Configuration;
using EntityStates.SniperClassicSkills;
using RiskOfOptions;
using System;
using UnityEngine;

namespace SniperClassic.Modules
{
    public class Config
    {
        public static bool arenaNerf;
        public static bool forceUnlock;
        public static bool changeSortOrder;
        public static Beret beret;
        public static bool altMastery;

        public static bool cursed;

        public static bool spotterUI;

        public static bool markShowAmmoWhileSprinting;

        public enum Beret { 
            True,
            False,
            AllSkins
        }

        public static void ReadConfig(ConfigFile Config) {

            forceUnlock =
                Config.Bind<bool>("00 - General",
                                  "Force Unlock",
                                  false,
                                  "Unlocks Sniper and his skills. Skins will not be unlocked.").Value;
            changeSortOrder = 
                Config.Bind<bool>("00 - General", 
                                  "Change Sort Order", 
                                  false,
                                  "Sorts Sniper among the vanilla survivors based on unlock condition.").Value;

            arenaNerf =
                Config.Bind<bool>("00 - General",
                                  "Kings Kombat Arena Nerf",
                                  true,
                                  "Disable Spotter Slow when Kings Kombat Arena is active.").Value;

            beret =
                Config.Bind<Beret>("00 - General",
                                  "beret",
                                  Beret.True,
                                  "Enable Beret on Mastery Skin.").Value;
            altMastery =
                Config.Bind<bool>("00 - General",
                                  "Alternate Mastery",
                                  false,
                                  "An extra mastery skin with default sniper color scheme.").Value;
                                   //of course timesweeper enters and suddenly there's extra config skins

            cursed = Config.Bind<bool>("00 - General",
                                  "Cursed",
                                  false,
                                  "Enables extra/unfinished content. Use at your own risk.").Value;

            Snipe.useSlowReload =
                Config.Bind<bool>("10 - Primary - Snipe",
                                  "Slower reload.",
                                  false,
                                  "Slows down the reload bar of Snipe.");
            ModSettingsManager.AddOption(new RiskOfOptions.Options.CheckBoxOption(Snipe.useSlowReload));

            markShowAmmoWhileSprinting =
                Config.Bind<bool>("11 - Primary - Mark",
                                  "Show ammo while sprinting.",
                                  false,
                                  "Shows Mark's ammo counter while sprinting.").Value;

            SecondaryScope.toggleScope = 
                Config.Bind<bool>("20 - Secondary - Steady Aim",
                                  "Toggle Scope",
                                  false,
                                 "Makes Steady Aim not require you to hold down the skill key to use.");
            ModSettingsManager.AddOption(new RiskOfOptions.Options.CheckBoxOption(SecondaryScope.toggleScope));

            ScopeController.defaultShoulderCam = Config.Bind<bool>("20 - Secondary - Steady Aim",
                                  "Thirdperson by Default",
                                  false,
                                 "Makes Steady Aim put you in 3rdperson by default.");
            ModSettingsManager.AddOption(new RiskOfOptions.Options.CheckBoxOption(ScopeController.defaultShoulderCam));

            SecondaryScope.zoomFOV =
                Config.Bind<float>("20 - Secondary - Steady Aim",
                                  "Scoped FOV",
                                  35f,
                                 "Zoom level of Steady Aim while scoped.");
            if (SecondaryScope.zoomFOV.Value < SecondaryScope.minFOV) SecondaryScope.zoomFOV.Value = SecondaryScope.minFOV;
            if (SecondaryScope.zoomFOV.Value >= SecondaryScope.maxFOV) SecondaryScope.zoomFOV.Value = SecondaryScope.maxFOV - 1f;
            ModSettingsManager.AddOption(new RiskOfOptions.Options.SliderOption(SecondaryScope.zoomFOV, new RiskOfOptions.OptionConfigs.SliderConfig() { min = SecondaryScope.minFOV, max = SecondaryScope.maxFOV - 1f }));

            SecondaryScope.cameraToggleKey = 
                Config.Bind<KeyboardShortcut>("20 - Secondary - Steady Aim",
                                     "Camera Toggle Button",
                                     new KeyboardShortcut(KeyCode.V),
                                     "Keyboard button that swaps the Scope between Thirdperson and Firstperson.");
            ModSettingsManager.AddOption(new RiskOfOptions.Options.KeyBindOption(SecondaryScope.cameraToggleKey));

            spotterUI = Config.Bind<bool>("40 - Spotter",
                                  "Show HUD",
                                  true,
                                  "Shows a stat display when Spotting an enemy.").Value;

        }
    }
}