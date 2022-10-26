using RoR2;
using RoR2.Projectile;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace SniperClassic.Components
{
    public class ProjectileHeadshotComponent : MonoBehaviour, IProjectileImpactBehavior
    {
        public float radius = 1f;

        public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
        {
            if (NetworkServer.active)
            {
                ProjectileController pc = base.GetComponent<ProjectileController>();
                if (pc)
                {
                    Collider[] array = Physics.OverlapSphere(base.transform.position, radius, LayerIndex.entityPrecise.mask);
                    for (int i = 0; i < array.Length; i++)
                    {
                        HurtBox hurtBox = array[i].GetComponent<HurtBox>();
                        if (hurtBox && hurtBox.isSniperTarget && hurtBox.healthComponent && hurtBox.healthComponent.body && hurtBox.healthComponent.body.teamComponent.teamIndex != pc.teamFilter.teamIndex)
                        {
                            ProjectileImpactExplosion pie = base.GetComponent<ProjectileImpactExplosion>();
                            if (pie) pie.blastDamageCoefficient *= ScopeController.weakpointMultiplier;
                            ProjectileDamage pd = base.GetComponent<ProjectileDamage>();
                            if (pd) pd.damageColorIndex = DamageColorIndex.Sniper;

                            if (pc.owner)
                            {
                                ScopeController sc = pc.owner.GetComponent<ScopeController>();
                                if (sc) sc.RpcPlayHeadshotSound();
                            }
                            return;
                        }
                    }
                }
            }
        }
    }
}
