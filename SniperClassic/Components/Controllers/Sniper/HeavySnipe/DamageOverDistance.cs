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
        private float originalDamage;
        private float originalRadius;

        private float totalDamageRampup;
        private float totalRadiusRampup;

        public static float radiusRampupPerSecond = 2f;
        public static float damageRampupPerSecond = 0.6f;
        public static float maxDamageRampup = 0.0f; //was testing 0.2f
        public static float maxRadiusRampup = 10000f;

        //private Vector3 startPos;

        public void Awake()
        {
            pie = base.GetComponent<ProjectileImpactExplosion>();
            totalDamageRampup = 0f;
            totalRadiusRampup = 0f;
        }

        public void Start()
        {
            originalDamage = pie.blastDamageCoefficient;
            originalRadius = pie.blastRadius;
            //startPos = base.transform.position;

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
            totalDamageRampup = Mathf.Min(maxDamageRampup, totalDamageRampup + damageRampupPerSecond * Time.fixedDeltaTime);
            totalRadiusRampup = Mathf.Min(maxRadiusRampup, totalRadiusRampup + radiusRampupPerSecond * Time.fixedDeltaTime);

            pie.blastDamageCoefficient = originalDamage + originalDamage * totalDamageRampup;
            pie.blastRadius = originalRadius + originalRadius * totalRadiusRampup;

            //float distance = (base.transform.position - startPos).magnitude;
            //Debug.Log(totalDamageRampup + " - " +distance +"m" );
        }
    }
}
