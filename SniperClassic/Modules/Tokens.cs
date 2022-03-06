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
                Language.collectLanguageRootFolders += delegate (List<string> list)
                {
                    list.Add(Tokens.fileSystem.GetDirectoryEntry("/language/").FullName); //todo: fix this
                };
            }
        }
    }
}