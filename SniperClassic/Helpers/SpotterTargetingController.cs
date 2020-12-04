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
        [Command]
        private void CmdSendSpotter(uint masterID)
        {
            if (masterID != uint.MaxValue)
            {
                __spotterLockedOn = true;
                spotterFollower.__AssignNewTarget(masterID);
            }
            else
            {
                CmdReturnSpotter();
            }
        }

        [Command]
        private void CmdReturnSpotter()
        {
            __spotterLockedOn = false;
            spotterFollower.__AssignNewTarget(uint.MaxValue);
        }

        [Client]
        public void ClientSendSpotter()
        {
            uint netID = uint.MaxValue;
            if (hasTrackingTarget)
            {
                netID = trackingTarget.healthComponent.body.masterObject.GetComponent<NetworkIdentity>().netId.Value;
            }
            CmdSendSpotter(netID);
        }
        [Client]
        public void ClientReturnSpotter()
        {
            CmdReturnSpotter();
        }

        private void Start()
        {
            this.characterBody = base.GetComponent<CharacterBody>();
            this.inputBank = base.GetComponent<InputBankTest>();
            this.teamComponent = base.GetComponent<TeamComponent>();
        }

        private void ForceEndSpotterSkill()
        {
            if (base.hasAuthority)
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
                if (!this.spotterFollower)
                {
                    SpawnSpotter();
                }
            }
            else
            {
                if (__hasSpotter && !this.spotterFollower)
                {
                    CmdUpdateSpotter();
                }
            }
            
            if (!__spotterLockedOn)
            {
                this.trackerUpdateStopwatch += Time.fixedDeltaTime;
                if (this.trackerUpdateStopwatch >= 1f / this.trackerUpdateFrequency)
                {
                    this.trackerUpdateStopwatch -= 1f / this.trackerUpdateFrequency;
                    Ray aimRay = new Ray(this.inputBank.aimOrigin, this.inputBank.aimDirection);
                    this.SearchForTarget(aimRay);
                }
            }
            else
            {
                if (!this.trackingTarget || !this.trackingTarget.healthComponent.alive)
                {
                    this.trackingTarget = null;
                    this.hasTrackingTarget = false;
                    if (base.hasAuthority)
                    {
                        CmdReturnSpotter();
                        ForceEndSpotterSkill();
                    }
                }
            }

            this.indicator.targetTransform = (this.trackingTarget ? this.trackingTarget.transform : null);
        }

        [Command]
        private void CmdUpdateSpotter()
        {
            if (this.spotterFollower)
            {
                RpcSetSpotterFollower(spotterFollower.GetComponent<NetworkIdentity>().netId.Value);
            }
            else
            {
                this.__hasSpotter = false;
            }
        }

        [ClientRpc]
        void RpcSetSpotterFollower(uint id)
        {
            if (!NetworkServer.active)
            {
                GameObject go = ClientScene.FindLocalObject(new NetworkInstanceId(id));
                if (!go)
                {
                    Debug.Log("ERROR: No spotter follower found on client\n\n");
                    return;
                }
                Debug.Log("Spotter follower found on client");
                this.spotterFollower = go.GetComponent<SpotterFollowerController>();
                if (this.spotterFollower != null)
                {
                    Debug.Log("Spotter follower has controller on client\n\n");
                }
                else
                {
                    Debug.Log("ERROR: Spotter follower has NO controller on client\n\n");
                }
            }
        }

        [Server]
        private void SpawnSpotter()
        {
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(spotterFollowerGameObject, base.transform.position, Quaternion.identity);
            this.spotterFollower = gameObject.GetComponent<SpotterFollowerController>();
            this.spotterFollower.ownerBodyObject = base.gameObject;
            this.spotterFollower.__ownerMasterNetID = characterBody.masterObject.GetComponent<NetworkIdentity>().netId.Value;
            NetworkServer.Spawn(gameObject);
            __hasSpotter = true;
            RpcSetSpotterFollower(spotterFollower.GetComponent<NetworkIdentity>().netId.Value);
        }

        private void SearchForTarget(Ray aimRay)
        {
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
            if (this.trackingTarget && this.trackingTarget.healthComponent && this.trackingTarget.healthComponent.body && !this.trackingTarget.healthComponent.body.HasBuff(SniperClassic.spotterStatDebuff) && this.trackingTarget.healthComponent.body.masterObject)
            {
                this.hasTrackingTarget = true;
                return;
            }
            this.hasTrackingTarget = false;
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
        private bool __spotterLockedOn = false;

        private bool hasTrackingTarget = false;

        private uint trackingTargetNetID;

        private CharacterBody characterBody;
        private TeamComponent teamComponent;
        private InputBankTest inputBank;
        private float trackerUpdateStopwatch;
        private Indicator indicator;
        private readonly BullseyeSearch search = new BullseyeSearch();

        private SpotterFollowerController spotterFollower;

        [SyncVar]
        private bool __hasSpotter = false;

        public static GameObject spotterFollowerGameObject = null;
        
    }
}
