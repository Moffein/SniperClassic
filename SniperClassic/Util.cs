using BepInEx.Configuration;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace SniperClassic
{
    public static class Util
    {
        //Taken from https://github.com/ToastedOven/CustomEmotesAPI/blob/main/CustomEmotesAPI/CustomEmotesAPI/CustomEmotesAPI.cs
        public static bool GetKeyPressed(ConfigEntry<KeyboardShortcut> entry)
        {
            foreach (var item in entry.Value.Modifiers)
            {
                if (!Input.GetKey(item))
                {
                    return false;
                }
            }
            return Input.GetKeyDown(entry.Value.MainKey);
        }

        internal static void HandleLuminousShotServer(CharacterBody body)
        {
            if (!NetworkServer.active || !body || !body.inventory) return;
            if (body.inventory.GetItemCount(DLC2Content.Items.IncreasePrimaryDamage) <= 0) return;

            body.AddIncreasePrimaryDamageStack();
        }
    }
}
