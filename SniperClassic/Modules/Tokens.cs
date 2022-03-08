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
        public static void RegisterLanguageTokens()
        {
            /*PhysicalFileSystem physicalFileSystem = new PhysicalFileSystem();
            Tokens.fileSystem = new SubFileSystem(physicalFileSystem, physicalFileSystem.ConvertPathFromInternal(System.IO.Path.GetDirectoryName(SniperClassic.pluginInfo.Location)), true);
            if (Tokens.fileSystem.DirectoryExists("/language/"))
            {
                UnityEngine.Debug.Log("\n\n\n\n\nDirectory found");
                Language.collectLanguageRootFolders += delegate (List<string> list)
                {
                    list.Add(Tokens.fileSystem.GetDirectoryEntry("/language/").FullName); //todo: fix this
                };
            }*/

            //TODO: Replace this with the proper way of loading languages once someone figures that out.
            string languageFileName = "Sniper.txt";
            string pathToLanguage = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\language";
            LanguageAPI.AddPath(Path.Combine(pathToLanguage + @"\en", languageFileName));
            LanguageAPI.AddPath(Path.Combine(pathToLanguage + @"\es-419", languageFileName));
            LanguageAPI.AddPath(Path.Combine(pathToLanguage + @"\RU", languageFileName));

            /*var path = Path.Combine(pathToLanguage, languageFileName);
            LanguageAPI.AddPath(path);*/
        }
    }
}