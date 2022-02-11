using BepInEx.Configuration;
using EntityStates.SniperClassicSkills;
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

            bool snipeSlowReload = 
                Config.Bind<bool>("10 - Primary - Snipe", 
                                  "Slower reload.",
                                  false,
                                  "Slows down the reload bar of Snipe.").Value;
            if (snipeSlowReload) { Snipe.reloadBarLength = 1f; }    //HeavySnipe.reloadBarLength = 1f; Add this if changing Hard Impact.

            bool scopeCSGOZoom = 
                Config.Bind<bool>("20 - Secondary - Steady Aim",
                                  "Preset Zoom (Overrides all other settings)",
                                  false,
                                 "Pressing M2 cycles through preset zoom levels.").Value;

            bool scopeToggle = 
                Config.Bind<bool>("20 - Secondary - Steady Aim",
                                  "Toggle Scope",
                                  false,
                                 "Makes Steady Aim not require you to hold down the skill key to use.").Value;

            float scopeZoomFOV = 
                Config.Bind<float>("20 - Secondary - Steady Aim",
                                  "Default FOV",
                                  50f,
                                 "Default zoom level of Steady Aim (accepts values from 5-50).").Value;

            bool scopeResetZoom = 
                Config.Bind<bool>("20 - Secondary - Steady Aim",
                                  "Reset Zoom on Unscope",
                                  false,
                                 "Reset scope zoom level when unscoping.").Value;

            bool scopeUseScrollWheel = 
                Config.Bind<bool>("20 - Secondary - Steady Aim",
                                  "Use Scroll Wheel for Zoom",
                                  true,
                                 "Scroll wheel changes zoom level. Scroll up to zoom in, scroll down to zoom out.").Value;

            bool scopeInvertScrollWheel = 
                Config.Bind<bool>("20 - Secondary - Steady Aim",
                                  "Invert Scroll Wheel",
                                  false,
                                 "Reverses scroll wheel direction. Scroll up to zoom out, scroll down to zoom in.").Value;

            float scopeScrollZoomSpeed = 
                Config.Bind<float>("20 - Secondary - Steady Aim",
                                  "Scroll Wheel Zoom Speed",
                                  30f,
                                 "Zoom speed when using the scroll wheel.").Value;

            KeyCode scopeZoomInKey = 
                Config.Bind<KeyCode>("20 - Secondary - Steady Aim",
                                     "Zoom-In Button",
                                     KeyCode.None,
                                     "Keyboard button that zooms the scope in.").Value;

            KeyCode scopeZoomOutKey = 
                Config.Bind<KeyCode>("20 - Secondary - Steady Aim",
                                     "Zoom-Out Button",
                                     KeyCode.None,
                                     "Keyboard button that zooms the scope out.").Value;

            float scopeButtonZoomSpeed = 
                Config.Bind<float>("20 - Secondary - Steady Aim",
                                  "Button Zoom Speed",
                                  1f,
                                 "Zoom speed when using keyboard buttons.").Value;

            spotterUI = Config.Bind<bool>("40 - Spotter",
                                  "Show HUD",
                                  true,
                                  "Shows a stat display when Spotting an enemy.").Value;


            SecondaryScope.zoomFOV = scopeZoomFOV;
            if (SecondaryScope.zoomFOV < SecondaryScope.minFOV)
            {
                SecondaryScope.zoomFOV = SecondaryScope.minFOV;
            }
            else if (SecondaryScope.zoomFOV > SecondaryScope.maxFOV)
            {
                SecondaryScope.zoomFOV = SecondaryScope.maxFOV;
            }
            SecondaryScope.useScrollWheelZoom = scopeUseScrollWheel;
            SecondaryScope.invertScrollWheelZoom = scopeInvertScrollWheel;
            SecondaryScope.zoomInKey = scopeZoomInKey;
            SecondaryScope.zoomOutKey = scopeZoomOutKey;
            SecondaryScope.scrollZoomSpeed = scopeScrollZoomSpeed;
            SecondaryScope.buttonZoomSpeed = scopeButtonZoomSpeed;
            SecondaryScope.resetZoom = scopeResetZoom;
            SecondaryScope.toggleScope = scopeToggle;
            SecondaryScope.csgoZoom = scopeCSGOZoom;
        }
    }
}