using R2API;
using RoR2;
using SniperClassic.Modules;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

namespace SniperClassic.Hooks
{
    public class TakeDamage
    {
        public static void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo.HasModdedDamageType(SniperContent.Shock5sNoDamage))
            {
                damageInfo.rejected = true;
                damageInfo.damage = 0f;

                //Manually trigger shock
                SetStateOnHurt ssoh = self.gameObject.GetComponent<SetStateOnHurt>();
                if (ssoh && ssoh.canBeStunned)
                {
                    ssoh.SetShock(5f);
                }
            }
            orig(self, damageInfo);
        }
    }
}
