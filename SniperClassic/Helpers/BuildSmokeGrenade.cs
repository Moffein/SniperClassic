using R2API;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SniperClassic
{
    public class SmokeComponent : MonoBehaviour
    {
        private int count;
        private int lastCount;
        private uint playID;

        public static event Action<int> GasCheck = delegate { };

        private void Awake()
        {
            playID = Util.PlaySound("looping smoke sound", base.gameObject);

            InvokeRepeating("Wat", 0.25f, 0.25f);
        }

        private void Wat()
        {
            //this is gross and hacky pls someone do this a different way eventually

            count = 0;

            foreach (CharacterBody i in GameObject.FindObjectsOfType<CharacterBody>())
            {
                if (i && i.HasBuff(BuffIndex.Cloak)) count++;
            }

            if (lastCount != count) GasCheck(count);

            lastCount = count;
        }

        private void OnDestroy()
        {
            AkSoundEngine.StopPlayingID(playID);
        }
    }
}
