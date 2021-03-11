using AK.Wwise;
using EntityStates.Commando.CommandoWeapon;
using EntityStates.Engi.EngiMissilePainter;
using EntityStates.SniperClassicSkills;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace SniperClassic
{
    class ReloadController : NetworkBehaviour
    {
        public void AutoReload()
        {
            if (skillLocator && skillLocator.primary)
            {
                if (skillLocator.primary.stateMachine)
                {
                    Debug.Log(skillLocator.primary.stateMachine.state.GetType());
                    Type skillType = skillLocator.primary.stateMachine.state.GetType();
                    if (skillType.IsAssignableFrom(typeof(BaseSnipeState)))
                    {
                        (skillLocator.primary.stateMachine.state as BaseSnipeState).AutoReload();
                    }
                    else if (skillType.IsAssignableFrom(typeof(BaseReloadState)))
                    {
                        (skillLocator.primary.stateMachine.state as BaseReloadState).AutoReload();
                    }
                    else if (skillType == typeof(FireBattleRifle))
                    {
                        (skillLocator.primary.stateMachine.state as FireBattleRifle).AutoReload();
                    }
                    else if (skillType == typeof(ReloadBR))
                    {
                        (skillLocator.primary.stateMachine.state as ReloadBR).AutoReload();
                    }
                    else
                    {
                        Type equippedSkillType = skillLocator.primary.skillDef.activationState.GetType();
                        if (equippedSkillType.IsAssignableFrom(typeof(BaseSnipeState)))
                        {
                            if (GetReloadQuality() != ReloadQuality.Perfect)
                            {
                                SetReloadQuality(ReloadQuality.Perfect, false);
                            }
                        }
                        else if (skillType == typeof(FireBattleRifle))
                        {
                            SetReloadQuality(ReloadQuality.Good, false);
                        }
                    }

                    if (skillLocator.primary.maxStock > 1)
                    {
                        skillLocator.primary.stock = skillLocator.primary.maxStock;
                    }
                }
            }
        }

        public void BattleRiflePerfectReload()
        {
            if (skillLocator.secondary.stock < skillLocator.secondary.maxStock)
            {
                skillLocator.secondary.stock++;
                if (skillLocator.secondary.stock == skillLocator.secondary.maxStock)
                {
                    skillLocator.secondary.rechargeStopwatch = 0f;
                }
            }
        }

        //The sound code is duplicated in RpcPlayReloadSound because the reload sound should be instantaneous for the client triggering it.
        public void SetReloadQuality(ReloadQuality r, bool playLoadSound = true)
        {
            this.currentReloadQuality = r;
            if (playLoadSound)
            {
                switch (this.currentReloadQuality)
                {
                    case ReloadQuality.Good:
                        Util.PlaySound(ReloadController.goodReloadSoundString, base.gameObject);
                        break;
                    case ReloadQuality.Perfect:
                        Util.PlaySound(ReloadController.perfectReloadSoundString, base.gameObject);
                        break;
                    default:
                        break;
                }
            }
            Util.PlaySound(ReloadController.boltReloadSoundString, base.gameObject);
            Util.PlaySound(ReloadController.casingSoundString, base.gameObject);

            CmdPlayReloadSound((int)this.currentReloadQuality, playLoadSound);
        }

        [Command]
        public void CmdPlayReloadSound(int rq, bool playLoadSound)
        {
            RpcPlayReloadSound(rq, playLoadSound);
        }

        [ClientRpc]
        public void RpcPlayReloadSound(int rq, bool playLoadSound)
        {
            if (!this.hasAuthority)
            {
                if (playLoadSound)
                {
                    switch (rq)
                    {
                        case (int)ReloadQuality.Good:
                            Util.PlaySound(ReloadController.goodReloadSoundString, base.gameObject);
                            break;
                        case (int)ReloadQuality.Perfect:
                            Util.PlaySound(ReloadController.perfectReloadSoundString, base.gameObject);
                            break;
                        default:
                            break;
                    }
                }
                Util.PlaySound(ReloadController.boltReloadSoundString, base.gameObject);
                Util.PlaySound(ReloadController.casingSoundString, base.gameObject);
            }
        }

        [Command]
        public void CmdPlayPing()
        {
            RpcPlayPing();
        }

        [ClientRpc]
        private void RpcPlayPing()
        {
            Util.PlaySound(ReloadController.pingSoundString, base.gameObject);
        }


        [Command]
        private void CmdPlayReloadFail()
        {
            RpcPlayReloadFail();
        }

        [ClientRpc]
        private void RpcPlayReloadFail()
        {
            if (!this.hasAuthority)
            {
                Util.PlaySound(ReloadController.failSoundString, base.gameObject);
            }
        }

        public float GetDamageMult()
        {
            switch (this.currentReloadQuality)
            {
                case ReloadQuality.Good:
                    return 1.25f;
                case ReloadQuality.Perfect:
                    return 1.5f;
                default:
                    return 1f;
            }
        }

        public ReloadQuality GetReloadQuality()
        {
            return this.currentReloadQuality;
        }

        public void Awake()
        {
            reloadProgress = 0f;

            rectBar.width = Screen.height * 144f * reloadBarScale / 1080f;
            rectBar.height = Screen.height * 24f * reloadBarScale / 1080f;

            rectCursor.width = Screen.height * 24f * reloadBarScale / 1080f;
            rectCursor.height = Screen.height * 24f * reloadBarScale / 1080f;

            rectIndicator.width = Screen.height * 48f * reloadIndicatorScale / 1080f;
            rectIndicator.height = Screen.height * 48f * reloadIndicatorScale / 1080f;

            rectBar.position = new Vector2(Screen.width / 2 - rectBar.width / 2, Screen.height / 2 + 3 * rectBar.height / 2);
            barLeftBound = Screen.width / 2 - (Screen.height * 80f * reloadBarScale / 1080f); // 80 used to be -68-12
            rectCursor.position = new Vector2(barLeftBound, Screen.height / 2 + rectCursor.width / 2 + rectBar.height);

            characterBody = base.GetComponent<CharacterBody>();
            healthComponent = characterBody.healthComponent;
            skillLocator = characterBody.skillLocator;
        }

        public void EnableReloadBar(float reloadBarLength, bool brReload = false, float brReloadTime = 0f)
        {
            standardReload = !brReload;
            isReloading = true;
            hideLoadIndicator = false;
            reloadProgress = 0f;
            reloadLength = reloadBarLength;
            reloadBarBounces = !brReload;
            reloadReverse = brReload;
            reloadMovingBackwards = false;
            failedReload = false;
            pauseReload = false;
            finishedReload = false;
            triggeredBRReload = false;

            brReloadDuration = brReloadTime;

            rectBar.width = Screen.height * 144f * reloadBarScale / 1080f * (reloadReverse ? -1f : 1f);
            rectBar.height = Screen.height * 24f * reloadBarScale / 1080f;

            rectCursor.width = Screen.height * 24f * reloadBarScale / 1080f;
            rectCursor.height = Screen.height * 24f * reloadBarScale / 1080f;

            rectIndicator.width = Screen.height * 48f * reloadIndicatorScale / 1080f;
            rectIndicator.height = Screen.height * 48f * reloadIndicatorScale / 1080f;

            rectBar.position = new Vector2(Screen.width / 2 - rectBar.width / 2, Screen.height / 2 + 3 * rectBar.height / 2);
            barLeftBound = Screen.width / 2 - (Screen.height * 80f * reloadBarScale / 1080f); // 80 used to be -68-12
            rectCursor.position = new Vector2(barLeftBound, Screen.height / 2 + rectCursor.width / 2 + rectBar.height);
        }

        private void OnGUI()
        {
            if (this.hasAuthority && !RoR2.PauseManager.isPaused && healthComponent && healthComponent.alive)
            {
                if (isReloading && !finishedReload)
                {
                    GUI.DrawTexture(rectBar, failedReload ? reloadBarFail : reloadBar, ScaleMode.StretchToFill, true, 0f);
                    GUI.DrawTexture(rectCursor, failedReload ? reloadCursorFail : reloadCursor, ScaleMode.StretchToFill, true, 0f);
                }
                else if (!hideLoadIndicator)
                {
                    if (currentReloadQuality != ReloadQuality.Bad)
                    {
                        rectIndicator.position = new Vector2(Screen.width / 2 - rectIndicator.width / 2, Screen.height / 2 + rectIndicator.height * 3 / 4);
                        if (currentReloadQuality == ReloadQuality.Good)
                        {
                            GUI.DrawTexture(rectIndicator, indicatorGood, ScaleMode.StretchToFill, true, 0f);
                        }
                        else
                        {
                            GUI.DrawTexture(rectIndicator, indicatorPerfect, ScaleMode.StretchToFill, true, 0f);
                        }
                    }
                }
            }
        }

        private void FixedUpdate()
        {
            if (isReloading && !pauseReload)
            {
                reloadProgress += Time.fixedDeltaTime * (reloadAttackSpeedScale ? characterBody.attackSpeed : 1f) * (reloadMovingBackwards ? -1f : 1f);
                if (standardReload)
                {
                    if (reloadProgress > reloadLength || reloadProgress < 0f)
                    {
                        reloadMovingBackwards = !reloadMovingBackwards;
                        if (reloadProgress > reloadLength)
                        {
                            while (reloadProgress - reloadLength > reloadLength)
                            {
                                reloadProgress -= reloadLength;
                            }
                            reloadProgress = reloadLength * 2f - reloadProgress;
                        }
                        else
                        {
                            while (reloadProgress + reloadLength < 0f)
                            {
                                reloadProgress += reloadLength;
                            }
                            reloadProgress *= -1f;
                        }
                    }
                }
                else
                {
                    if (reloadProgress > reloadLength && !triggeredBRReload)
                    {
                        failedReload = true;
                        ReloadBR(brReloadDuration, true);
                    }
                }
                rectCursor.position = new Vector2(barLeftBound + (Screen.height * reloadProgress / reloadLength * 136f * reloadBarScale / 1080f), rectCursor.position.y);
            }
            if (reloadLingerTimer > 0f)
            {
                reloadLingerTimer -= Time.fixedDeltaTime;
                if (reloadLingerTimer <= 0f)
                {
                    finishedReload = true;
                }
            }

            if (isReloading && healthComponent.isInFrozenState && !finishedReload && reloadLingerTimer <= 0f)
            {
                DisableReloadBar();
                SetReloadQuality(ReloadQuality.Bad);
            }
        }

        public float GetReloadBarLength()
        {
            return reloadAttackSpeedScale ? reloadLength * characterBody.attackSpeed : reloadLength;
        }

        public void ReloadBR(float lingerTimer = 0f, bool forceFail = false)
        {
            hideLoadIndicator = true;
            if (!this.hasAuthority)
            {
                return;
            }
            if (!forceFail)
            {
                float reloadPercent = reloadProgress / reloadLength;
                ReloadQuality r = ReloadQuality.Bad;
                if (reloadPercent >= 1 - reloadBarGoodEndPercent && reloadPercent < 1 - reloadBarGoodBeginPercent)
                {
                    r = ReloadQuality.Good;
                }
                else if (reloadPercent >= 1 - reloadBarGoodBeginPercent && reloadPercent <= 1 - reloadBarPerfectBeginPercent)
                {
                    r = ReloadQuality.Perfect;
                }
                else
                {
                    r = ReloadQuality.Bad;
                }

                if (r != ReloadQuality.Bad)
                {
                    pauseReload = true;
                    if (r == ReloadQuality.Perfect)
                    {
                        BattleRiflePerfectReload();
                    }
                    SetReloadQuality(r, true);
                    reloadLingerTimer = lingerTimer;
                    if (reloadLingerTimer <= 0f)
                    {
                        finishedReload = true;
                    }
                    triggeredBRReload = true;
                }
                else
                {
                    failedReload = true;
                    Util.PlaySound(ReloadController.failSoundString, base.gameObject);
                    CmdPlayReloadFail();
                }    
            }
            else if (forceFail)
            {
                pauseReload = true;
                triggeredBRReload = true;
                SetReloadQuality(ReloadQuality.Bad,false);
                reloadLingerTimer = lingerTimer;
                if (reloadLingerTimer <= 0f)
                {
                    finishedReload = true;
                }
            }
        }

        public void Reload(float lingerTimer = 0f)
        {
            if (!this.hasAuthority)
            {
                return;
            }
            reloadLingerTimer = lingerTimer;
            pauseReload = true;
            float reloadPercent = reloadProgress / reloadLength;
            ReloadQuality r = ReloadQuality.Bad;
            if (reloadPercent >= reloadBarPerfectBeginPercent && reloadPercent < reloadBarGoodBeginPercent)
            {
                r = ReloadQuality.Perfect;
            }
            else if (reloadPercent >= reloadBarGoodBeginPercent && reloadPercent <= reloadBarGoodEndPercent)
            {
                r = ReloadQuality.Good;
            }
            else
            {
                r = ReloadQuality.Bad;
                failedReload = true;
            }


            reloadLingerTimer = lingerTimer;

            SetReloadQuality(r);
            hideLoadIndicator = false;

            if (reloadLingerTimer <= 0f)
            {
                finishedReload = true;
            }
        }

        public void DisableReloadBar()
        {
            isReloading = false;
            failedReload = false;
            reloadProgress = 0f;
        }

        public static Texture2D reloadBar = null;
        public static Texture2D reloadCursor = null;
        public static Texture2D indicatorGood = null;
        public static Texture2D indicatorPerfect = null;

        public static Texture2D reloadBarFail = null;
        public static Texture2D reloadCursorFail = null;

        public static string boltReloadSoundString = "Play_SniperClassic_reload_bolt";
        public static string failSoundString = "Play_commando_M2_grenade_throw";
        public static string pingSoundString = "Play_SniperClassic_m1_br_ping";
        public static string goodReloadSoundString = "Play_SniperClassic_reload_good";
        public static string perfectReloadSoundString = "Play_SniperClassic_reload_perfect";
        public static string casingSoundString = "Play_SniperClassic_casing";
        public static float reloadBarScale = 1.2f;
        public static float reloadIndicatorScale = 1.0f;
        public static bool reloadAttackSpeedScale = false;

        public bool brReload = false;
        public bool failedReload = false;

        private float reloadProgress = 0f;
        private float barLeftBound;
        private ReloadQuality currentReloadQuality = ReloadQuality.Bad;
        private float brReloadDuration = 0.6f;

        private float reloadLength = 0.6f;
        private bool reloadBarBounces = true;
        private bool reloadMovingBackwards = false;
        private bool reloadReverse = false;
        private float reloadLingerTimer = 0f;
        private bool pauseReload = false;
        private bool standardReload = true;
        private bool triggeredBRReload = false;

        private SkillLocator skillLocator;
        private HealthComponent healthComponent;
        private CharacterBody characterBody;

        public bool hideLoadIndicator = false;
        public bool isReloading = false;
        public Rect rectBar = new Rect();
        public Rect rectCursor = new Rect();
        public Rect rectIndicator = new Rect();
        public enum ReloadQuality
        {
            Bad, Good, Perfect
        }

        internal const float reloadBarPerfectBeginPercent = 0.15f / 0.6f;
        internal const float reloadBarGoodBeginPercent = 0.255f / 0.6f;
        internal const float reloadBarGoodEndPercent = 0.38f / 0.6f;

        public bool finishedReload = false;
    }
}
