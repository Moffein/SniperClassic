using R2API;
using UnityEngine;
using System.Collections.Generic;
using Zio;
using Zio.FileSystems;
using System.IO;
using System.Reflection;
using System.Linq;

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
            On.RoR2.Language.SetFolders += fixme;
        }

        //Credits to Anreol for this code
        private static void fixme(On.RoR2.Language.orig_SetFolders orig, RoR2.Language self, System.Collections.Generic.IEnumerable<string> newFolders)
        {
            if (System.IO.Directory.Exists(Tokens.languageRoot))
            {
                var dirs = System.IO.Directory.EnumerateDirectories(System.IO.Path.Combine(Tokens.languageRoot), self.name);
                orig(self, newFolders.Union(dirs));
                return;
            }
            orig(self, newFolders);
        }
    }
}