using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using Zio;
using Zio.FileSystems;

namespace SniperClassic.Modules
{
    public static class Tokens
    {
        public static SubFileSystem fileSystem;
        public static void RegisterLanguageTokens()
        {
            PhysicalFileSystem physicalFileSystem = new PhysicalFileSystem();
            Tokens.fileSystem = new SubFileSystem(physicalFileSystem, physicalFileSystem.ConvertPathFromInternal(System.IO.Path.GetDirectoryName(SniperClassic.pluginInfo.Location)), true);
            if (Tokens.fileSystem.DirectoryExists("/language/"))
            {
                Language.collectLanguageRootFolders += delegate (List<DirectoryEntry> list)
                {
                    list.Add(Tokens.fileSystem.GetDirectoryEntry("/language/"));
                };
            }

            //Need to figure out how to set up language file so that this isn't needed.
            string increment = Achievements.CharacterUnlockAchievement.TestAchievementIncrement;
            LanguageAPI.Add(increment + "SNIPERCLASSIC_CHARACTERUNLOCKABLE_ACHIEVEMENT_NAME", "Spotted");
            LanguageAPI.Add(increment + "SNIPERCLASSIC_CHARACTERUNLOCKABLE_ACHIEVEMENT_DESC", "Repair an Equipment Drone with a Radar Scanner.");
            LanguageAPI.Add(increment + "SNIPERCLASSIC_CHARACTERUNLOCKABLE_UNLOCKABLE_NAME", "Spotted");
        }
    }
}