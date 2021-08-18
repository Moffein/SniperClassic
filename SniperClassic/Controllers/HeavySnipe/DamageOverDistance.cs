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
        public static float rampupPerSecond = 0.7f;

        public void Awake()
        {
            pie = base.GetComponent<ProjectileImpactExplosion>();
        }

        public void Start()
        {
            originalDamage = pie.blastDamageCoefficient;
            originalRadius = pie.blastRadius;
        }

        public void FixedUpdate()
        {
            pie.blastDamageCoefficient += originalDamage * rampupPerSecond * Time.fixedDeltaTime;
            pie.blastRadius += originalRadius * rampupPerSecond * Time.fixedDeltaTime;
        }
    }
}
