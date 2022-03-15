using R2API;
using UnityEngine;
using System.Collections.Generic;
using Zio;
using Zio.FileSystems;
using System.IO;
using System.Reflection;

namespace SniperClassic.Modules
{
    public static class Tokens
    {
        public static SubFileSystem fileSystem;
        internal static string languageRoot => System.IO.Path.Combine(Tokens.assemblyDir, "language");

        internal static string assemblyDir
        {
            get
            {
                return System.IO.Path.GetDirectoryName(SniperClassic.pluginInfo.Location);
            }
        }

        public static void RegisterLanguageTokens()
        {

            RoR2.RoR2Application.onLoad += (delegate ()
            {
                if (Directory.Exists(Tokens.languageRoot))
                {
                    FixLanguageFolders(Tokens.languageRoot);
                }
            });
        }

        //Credits to Anreol for this code
        public static void FixLanguageFolders(string rootFolder)
        {
            var allLanguageFolders = Directory.EnumerateDirectories(rootFolder);
            foreach (RoR2.Language language in RoR2.Language.GetAllLanguages())
            {
                foreach (var folder in allLanguageFolders)
                {
                    if (folder.Contains(language.name))
                    {
                        HG.ArrayUtils.ArrayAppend<string>(ref language.folders, folder);
                    }
                }
            }
            //Reload all folders, by this time, the language has already been initialized, thats why we are doing this.
            RoR2.Language.currentLanguage.UnloadStrings();
            RoR2.Language.currentLanguage.LoadStrings();
            RoR2.Language.english.UnloadStrings();
            RoR2.Language.english.LoadStrings();
        }
    }
}