using R2API.Networking;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace SniperClassic
{
    class SpotterTargetingController : NetworkBehaviour
    {
        [Server]
        public void SendSpotter()
        {
            if (trackingTargetObject)
            {
                spotterLockedOn = true;
                if (trackingTargetObject.GetComponent<HealthComponent>())
                {
                    Debug.Log("healthcomponent from sent gameobject");
                }
                if (trackingTargetObject.GetComponent<CharacterBody>())
                {
                    Debug.Log("characterboddy from sent gameobject");
                }
                spotterFollower.AssignNewTarget(trackingTargetObject, trackingTargetNetIDRaw);
            }
            else
            {
                ReturnSpotter();
                Debug.Log("Attempted to assign target to null gameobject");
            }
        }

        [Server]
        public void ReturnSpotter()
        {
            spotterLockedOn = false;
            spotterFollower.AssignNewTarget(null,0);
        }

        private void Start()
        {
            this.characterBody = base.GetComponent<CharacterBody>();
            this.inputBank = base.GetComponent<InputBankTest>();
            this.teamComponent = base.GetComponent<TeamComponent>();
        }

        [ClientRpc]
        private void RpcForceEndSpotterSkill()
        {
            if(base.hasAuthority)
            {
                if (characterBody.skillLocator)
                {
                    if (characterBody.skillLocator && characterBody.skillLocator.special)
                    {
                        EntityStateMachine stateMachine = characterBody.skillLocator.special.stateMachine;
                        if (stateMachine)
                        {
                            EntityStates.SniperClassicSkills.SendSpotter sendSpotter = stateMachine.state as EntityStates.SniperClassicSkills.SendSpotter;
                            if (sendSpotter != null)
                            {
                                sendSpotter.OnExit();
                            }
                        }
                    }
                }
            }
        }

        private void FixedUpdate()
        {
            if (characterBody.skillLocator.special.stock < 1)
            {
                OnDisable();
            }
            else if (!this.indicator.active)
            {
                OnEnable();
            }


            if (NetworkServer.active)
            {
                if (!spotterLockedOn)
                {
                    this.trackerUpdateStopwatch += Time.fixedDeltaTime;
                    if (this.trackerUpdateStopwatch >= 1f / this.trackerUpdateFrequency)
                    {
                        this.trackerUpdateStopwatch -= 1f / this.trackerUpdateFrequency;
                        Ray aimRay = new Ray(this.inputBank.aimOrigin, this.inputBank.aimDirection);
                        this.SearchForTarget(aimRay);
                        this.indicator.targetTransform = (this.trackingTarget ? this.trackingTarget.transform : null);
                    }
                }
                else
                {
                    if (!this.trackingTarget || !this.trackingTarget.healthComponent.alive)
                    {
                        this.trackingTarget = null;
                        this.trackingTargetObject = null;
                        ReturnSpotter();
                        RpcForceEndSpotterSkill();
                    }
                }

                if (!this.spotterFollower && spotterFollowerGameObject != null)
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(spotterFollowerGameObject, base.transform.position, Quaternion.identity);
                    this.spotterFollower = gameObject.GetComponent<SpotterFollowerController>();
                    this.spotterFollower.ownerBodyObject = base.gameObject;
                    this.spotterFollower.ownerMasterNetID = characterBody.masterObject.GetComponent<NetworkIdentity>().netId.Value;
                    NetworkServer.Spawn(gameObject);
                }
            }
            else
            {
                if (!hasTrackingTarget)
                {
                    trackingTarget = null;
                    trackingTargetObject = null;
                }
            }
        }

        [ClientRpc]
        void RpcUpdateTrackingTarget()
        {
            if (!NetworkServer.active)
            {
                GameObject masterObject = ClientScene.FindLocalObject(new NetworkInstanceId(trackingTargetNetIDRaw));
                if (masterObject)
                {
                    CharacterMaster master = masterObject.GetComponent<CharacterMaster>();
                    if (master)
                    {
                        if (master.hasBody)
                        {
                            trackingTargetObject = master.GetBodyObject();
                            trackingTarget = trackingTargetObject.GetComponent<HurtBox>();
                            return;
                        }
                    }
                }
                Debug.Log("SpotterTargetingController: Could not resolve netid");
            }
        }

        [Server]
        private void SearchForTarget(Ray aimRay)
        {
            HurtBox trackingTargetPrev = this.trackingTarget;

            this.search.teamMaskFilter = TeamMask.GetUnprotectedTeams(this.teamComponent.teamIndex);
            this.search.filterByLoS = true;
            this.search.searchOrigin = aimRay.origin;
            this.search.searchDirection = aimRay.direction;
            this.search.sortMode = BullseyeSearch.SortMode.Angle;
            this.search.maxDistanceFilter = this.maxTrackingDistance;
            this.search.maxAngleFilter = this.maxTrackingAngle;
            this.search.RefreshCandidates();
            this.search.FilterOutGameObject(base.gameObject);
            this.trackingTarget = this.search.GetResults().FirstOrDefault<HurtBox>();
            if (this.trackingTarget && this.trackingTarget.healthComponent && this.trackingTarget.healthComponent.body)
            {
                this.hasTrackingTarget = true;
                this.trackingTargetObject = this.trackingTarget.healthComponent.body.gameObject;
                if (trackingTarget.healthComponent.body.masterObject)
                {
                    NetworkIdentity id = this.trackingTarget.healthComponent.body.masterObject.GetComponent<NetworkIdentity>();
                    if (id)
                    {
                        this.trackingTargetNetIDRaw = id.netId.Value;
                        RpcUpdateTrackingTarget();
                        return;
                    }
                }
            }
            this.hasTrackingTarget = false;
            this.trackingTargetObject = null;
            this.trackingTarget = null;
        }

        private void Awake()
        {
            this.indicator = new Indicator(base.gameObject, Resources.Load<GameObject>("Prefabs/EngiMissileTrackingIndicator"));
            this.characterBody = base.gameObject.GetComponent<CharacterBody>();
        }

        public HurtBox GetTrackingTarget()
        {
            return this.trackingTarget;
        }

        private void OnEnable()
        {
            this.indicator.active = true;
        }

        private void OnDisable()
        {
            this.indicator.active = false;
        }

        public float maxTrackingDistance = 2000f;
        public float maxTrackingAngle = 90f;
        public float trackerUpdateFrequency = 10f;

        private HurtBox trackingTarget;

        [SyncVar]
        private bool spotterLockedOn = false;

        [SyncVar]
        private uint trackingTargetNetIDRaw;

        [SyncVar]
        private bool hasTrackingTarget = false;

        private GameObject trackingTargetObject;

        private CharacterBody characterBody;
        private TeamComponent teamComponent;
        private InputBankTest inputBank;
        private float trackerUpdateStopwatch;
        private Indicator indicator;
        private readonly BullseyeSearch search = new BullseyeSearch();
        private SpotterFollowerController spotterFollower;

        public static GameObject spotterFollowerGameObject = null;
    }
}
