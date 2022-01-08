using EntityStates.SniperClassicSkills;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SniperClassic.Controllers
{
    public class DamageOverDistance : MonoBehaviour
    {
        private ProjectileImpactExplosion pie;
        //private float originalDamage;
        private float originalRadius;
        public static float rampupPerSecond = 2f;

        public void Awake()
        {
            pie = base.GetComponent<ProjectileImpactExplosion>();

        }

        public void Start()
        {
            //originalDamage = pie.blastDamageCoefficient;
            originalRadius = pie.blastRadius;

            ProjectileController pc = base.GetComponent<ProjectileController>();
            if (pc && pc.owner)
            {
                CharacterBody ownerBody = pc.owner.GetComponent<CharacterBody>();
                if (ownerBody)
                {
                    ProjectileDamage pd = base.GetComponent<ProjectileDamage>();
                    if (pd)
                    {
                        if (pd.damage > (ownerBody.damage * HeavySnipe.damageCoefficient * ScopeController.maxChargeMult) - 1)
                        {
                            pd.damageType = pd.damageType | DamageType.Stun1s;
                        }
                    }
                }
            }
        }

        public void FixedUpdate()
        {
            //pie.blastDamageCoefficient += originalDamage * rampupPerSecond * Time.fixedDeltaTime;
            pie.blastRadius += originalRadius * rampupPerSecond * Time.fixedDeltaTime;
        }
    }
}
