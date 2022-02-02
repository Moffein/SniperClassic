using RoR2;
using SniperClassic;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using R2API;
using RoR2.Orbs;
using System.Linq;

namespace EntityStates.SniperClassicSkills
{
    public class Backflip : BaseSkillState
    {
        public static float duration = 0.4f;
        public static float stunRadius = 12f;
        public static GameObject stunEffectPrefab;
        public static GameObject shockEffectPrefab;

        private float previousAirControl;

        public override void OnEnter()
        {
            base.OnEnter();
            this.previousAirControl = base.characterMotor.airControl;

            base.characterMotor.airControl = 0.15f;

            Vector3 direction = -base.GetAimRay().direction;

            //Spotter linger runs on all players.
            SpotterTargetingController stc = base.GetComponent<SpotterTargetingController>();
            bool hasSpotterFollower = false;

            if (base.characterBody.HasBuff(SniperClassic.Modules.SniperContent.spotterPlayerReadyBuff))
            {
                hasSpotterFollower = stc && stc.spotterFollower;
                if (hasSpotterFollower)
                {
                    stc.spotterFollower.SetLinger(base.characterBody.corePosition, 2f);
                    EffectManager.SimpleEffect(shockEffectPrefab, stc.spotterFollower.gameObject.transform.position, default, false);
                }
            }

            if (base.isAuthority)
            {

                direction.y = Mathf.Max(direction.y, 0.05f);
                Vector3 a = direction.normalized * 4f * 10f;
                Vector3 b = Vector3.up * 7f;
                Vector3 b2 = new Vector3(direction.x, 0f, direction.z).normalized * 3f;

                base.characterMotor.Motor.ForceUnground();
                base.characterMotor.velocity = a + b + b2;
                base.characterMotor.velocity.y *= 0.8f;
                if (base.characterMotor.velocity.y < 0) base.characterMotor.velocity.y *= 0.1f;

                base.characterBody.isSprinting = true;
                TriggerReload();
            }

            base.characterDirection.moveVector = direction;

            base.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;

            base.PlayAnimation("FullBody, Override", "Backflip", "Backflip.playbackRate", 1.5f * Backflip.duration);
            Util.PlayAttackSpeedSound(EntityStates.Commando.DodgeState.dodgeSoundString, base.gameObject, 1.5f);

            if (NetworkServer.active)
            {
                /*if (stunRadius > 0f)
                {
                    StunEnemies();
                }*/

                if (hasSpotterFollower)
                {
                    SpotterShockEnemies(stc);
                }
            }
        }

        //Locked behind Network check
        private void SpotterShockEnemies(SpotterTargetingController stc)
        {
            Vector3 spotterPosition = stc.spotterFollower.transform.position;

            List<HealthComponent> bouncedObjects = new List<HealthComponent>();
            int targets = 30;
            float range = 30f;
            TeamIndex team = base.GetTeam();

            bool isTargeting = stc.spotterFollower.IsTargetingEnemy();
            Vector3 lightningDirection = isTargeting ? Vector3.down : base.GetAimRay().direction;

            //Need to individually find all targets for the first bounce.
            for (int i = 0; i < targets; i++)
            {
                LightningOrb spotterLightning = new LightningOrb
                {
                    bouncedObjects = bouncedObjects,
                    attacker = base.gameObject,
                    inflictor = base.gameObject,
                    damageValue = 1f,
                    procCoefficient = 0f,
                    teamIndex = team,
                    isCrit = false,
                    procChainMask = default,
                    lightningType = LightningOrb.LightningType.Tesla,
                    damageColorIndex = DamageColorIndex.Nearby,
                    bouncesRemaining = 0,
                    targetsToFindPerBounce = 1,
                    range = range,
                    origin = spotterPosition,
                    damageType = DamageType.NonLethal,
                    speed = 120f,
                    arrivalTime = OrbManager.instance.time + 0.5f
                };

                spotterLightning.AddModdedDamageType(SniperClassic.Modules.SniperContent.Shock5sNoDamage);
                BullseyeSearch search = new BullseyeSearch();
                search.searchOrigin = base.characterBody.corePosition;
                search.searchDirection =  lightningDirection;
                search.maxAngleFilter = isTargeting ? 180f : 60f;
                search.teamMaskFilter = TeamMask.allButNeutral;
                search.teamMaskFilter.RemoveTeam(spotterLightning.teamIndex);
                search.filterByLoS = false;
                search.sortMode = BullseyeSearch.SortMode.Distance;
                search.maxDistanceFilter = spotterLightning.range;
                search.RefreshCandidates();
                HurtBox hurtBox = (from v in search.GetResults()
                                   where !spotterLightning.bouncedObjects.Contains(v.healthComponent)
                                   select v).FirstOrDefault<HurtBox>();

                //Fire orb if HurtBox is found.
                if (hurtBox)
                {
                    spotterLightning.target = hurtBox;
                    OrbManager.instance.AddOrb(spotterLightning);
                    spotterLightning.bouncedObjects.Add(hurtBox.healthComponent);
                }
            }
        }

        private void StunEnemies()
        {
            if (base.characterBody)
            {
                if(base.characterBody.coreTransform)
                {
                    EffectManager.SimpleEffect(Backflip.stunEffectPrefab, base.characterBody.corePosition, base.characterBody.coreTransform.rotation, true);
                }

                List<HealthComponent> hcList = new List<HealthComponent>();
                Collider[] array = Physics.OverlapSphere(base.characterBody.corePosition, Backflip.stunRadius, LayerIndex.entityPrecise.mask);
                for (int i = 0; i < array.Length; i++)
                {
                    HurtBox hurtBox = array[i].GetComponent<HurtBox>();
                    if (hurtBox && hurtBox.healthComponent && !hcList.Contains(hurtBox.healthComponent))
                    {
                        hcList.Add(hurtBox.healthComponent);
                        if (hurtBox.healthComponent.body.teamComponent && hurtBox.healthComponent.body.teamComponent.teamIndex != base.GetTeam())
                        {
                            SetStateOnHurt ssoh = hurtBox.healthComponent.gameObject.GetComponent<SetStateOnHurt>();
                            if (ssoh && ssoh.canBeStunned)
                            {
                                ssoh.SetStun(1f);
                            }
                        }
                    }
                }
            }
        }

        private void TriggerReload()
        {
            ReloadController rc = base.characterBody.GetComponent<ReloadController>();
            if (rc) rc.AutoReload();
        }

        public override void OnExit()
        {
            base.OnExit();

            base.characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
            base.characterMotor.airControl = this.previousAirControl;

            base.characterMotor.velocity *= 0.9f;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            base.StartAimMode(0.5f, false);

            if (base.fixedAge >= Backflip.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}