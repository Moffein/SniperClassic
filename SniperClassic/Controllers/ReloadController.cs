using AK.Wwise;
using EntityStates.Commando.CommandoWeapon;
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
                    if (skillType == typeof(Snipe))
                    {
                        (skillLocator.primary.stateMachine.state as Snipe).AutoReload();
                    }
                    else if (skillType == typeof(ReloadSnipe))
                    {
                        (skillLocator.primary.stateMachine.state as ReloadSnipe).AutoReload();
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
                        switch (skillLocator.primary.skillDef.skillName)
                        {
                            case "Snipe":
                                if (GetReloadQuality() != ReloadQuality.Perfect)
                                {
                                    SetReloadQuality(ReloadQuality.Perfect, false);
                                    hideLoadIndicator = false;
                                }
                                break;
                            case "BattleRifle":
                                if (skillLocator.primary.stock < skillLocator.primary.maxStock || skillLocator.secondary.stock < skillLocator.secondary.maxStock)
                                {
                                    SetReloadQuality(ReloadQuality.Good, false);
                                    hideLoadIndicator = true;
                                    //BattleRiflePerfectReload();
                                }
                                break;
                            default:
                                break;
                        }

                        if (skillLocator.primary.maxStock > 1)
                        {
                            skillLocator.primary.stock = skillLocator.primary.maxStock;
                        }
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
                        if (brReload)
                        {
                            Util.PlaySound(ReloadController.perfectReloadBRSoundString, base.gameObject);
                        }
                        break;
                    default:
                        break;
                }
                CmdPlayReloadSound((int)this.currentReloadQuality);
            }
            Util.PlaySound(ReloadController.boltReloadSoundString, base.gameObject);
            if (!brReload)
            {
                Util.PlaySound(ReloadController.casingSoundString, base.gameObject);
            }
        }

        [Command]
        public void CmdPlayReloadSound(int rq)
        {
            RpcPlayReloadSound(rq);
        }

        [ClientRpc]
        public void RpcPlayReloadSound(int rq)
        {
            if (!this.hasAuthority)
            {
                switch ((ReloadQuality)rq)
                {
                    case ReloadQuality.Good:
                        Util.PlaySound(ReloadController.goodReloadSoundString, base.gameObject);
                        break;
                    case ReloadQuality.Perfect:
                        Util.PlaySound(ReloadController.perfectReloadSoundString, base.gameObject);
                        if (brReload)
                        {
                            Util.PlaySound(ReloadController.perfectReloadBRSoundString, base.gameObject);
                        }
                        break;
                    default:
                        break;
                }
                Util.PlaySound(ReloadController.boltReloadSoundString, base.gameObject);
                if (!brReload)
                {
                    Util.PlaySound(ReloadController.casingSoundString, base.gameObject);
                }
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

        public int GetMagSize()
        {
            switch (this.currentReloadQuality)
            {
                case ReloadQuality.Good:
                    return 5;
                case ReloadQuality.Perfect:
                    return 6;
                default:
                    return 4;
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

            CharacterBody cb = base.GetComponent<CharacterBody>();
            healthComponent = cb.healthComponent;
            skillLocator = cb.skillLocator;
        }

        public void EnableReloadBar()
        {
            isReloading = true;
            hideLoadIndicator = false;
            reloadProgress = 0f;

            rectBar.width = Screen.height * 144f * reloadBarScale / 1080f * (brReload ? -1f : 1f);
            rectBar.height = Screen.height * 24f * reloadBarScale / 1080f;

            rectCursor.width = Screen.height * 24f * reloadBarScale / 1080f;
            rectCursor.height = Screen.height * 24f * reloadBarScale / 1080f;

            rectIndicator.width = Screen.height * 48f * reloadIndicatorScale / 1080f;
            rectIndicator.height = Screen.height * 48f * reloadIndicatorScale / 1080f;

            rectBar.position = new Vector2(Screen.width / 2 - rectBar.width/2, Screen.height / 2 + 3*rectBar.height/2);
            barLeftBound = Screen.width / 2 - (Screen.height * 80f * reloadBarScale / 1080f); // 80 used to be -68-12
            rectCursor.position = new Vector2(barLeftBound, Screen.height / 2 + rectCursor.width/2 + rectBar.height);
        }

        private void OnGUI()
        {
            if (this.hasAuthority && !RoR2.PauseManager.isPaused && healthComponent && healthComponent.alive)
            {
                if (isReloading)
                {
                    GUI.DrawTexture(rectBar, failedReload ? reloadBarFail : reloadBar, ScaleMode.StretchToFill, true, 0f);
                    GUI.DrawTexture(rectCursor, failedReload ? reloadCursorFail : reloadCursor, ScaleMode.StretchToFill, true, 0f);
                }
                else if (!hideLoadIndicator)
                {
                    if (currentReloadQuality != ReloadQuality.Bad)
                    {
                        rectIndicator.position = new Vector2(Screen.width / 2 - rectIndicator.width/2, Screen.height / 2 + rectIndicator.height * 3/4);
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

        public void UpdateReloadBar(float percent)
        {
            reloadProgress = percent;
            rectCursor.position = new Vector2(barLeftBound + (Screen.height*reloadProgress*136f * reloadBarScale / 1080f), rectCursor.position.y);
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
        public static string goodReloadSoundString = "Play_SniperClassic_reload_good";
        public static string perfectReloadSoundString = "Play_SniperClassic_reload_perfect";
        public static string perfectReloadBRSoundString = "Play_item_proc_bandolierPickup";
        public static string casingSoundString = "Play_SniperClassic_casing";
        public static float reloadBarScale = 1.2f;
        public static float reloadIndicatorScale = 1.0f;

        public bool brReload = false;
        public bool failedReload = false;

        private float reloadProgress = 0f;
        private float barLeftBound;
        private ReloadQuality currentReloadQuality = ReloadQuality.Bad;

        private SkillLocator skillLocator;
        private HealthComponent healthComponent;

        public bool hideLoadIndicator = false;
        public bool isReloading = false;
        public Rect rectBar = new Rect();
        public Rect rectCursor = new Rect();
        public Rect rectIndicator = new Rect();
        public enum ReloadQuality
        {
            Bad, Good, Perfect
        }
    }
}
