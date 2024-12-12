using R2API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SniperClassic.Modules
{
    internal static class SoundBanks
    {
        private static bool initialized = false;
        public static string SoundBankDirectory
        {
            get
            {
                return Path.Combine(Files.assemblyDir, "SoundBanks");
            }
        }

        public static void Init()
        {
            if (initialized) return;
            initialized = true;

            using (Stream manifestResourceStream = new FileStream(SoundBankDirectory + "\\SniperClassic_Sounds.bnk", FileMode.Open))
            {
                byte[] array = new byte[manifestResourceStream.Length];
                manifestResourceStream.Read(array, 0, array.Length);
                SoundAPI.SoundBanks.Add(array);
            }
        }
    }
}
