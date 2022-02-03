//This needs a rewrite. Too many variables for what it does. Inconsistent on whether attack speed changes are handled internally or externally. Overall a mess on how it interacts with other states.
using EntityStates.SniperClassicSkills;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace SniperClassic
{
    class ReloadController : NetworkBehaviour
    {
        public void AutoReload()    //Original plan was to check if the type inherits from BaseSnipeState.
        {
            if (skillLocator && skillLocator.primary)
            {
                if (skillLocator.primary.stateMachine)
                {
                    Type skillType = skillLocator.primary.stateMachine.state.GetType();
                    if (skillType == typeof(Snipe))
                    {
                        (skillLocator.primary.stateMachine.state as Snipe).AutoReload();
                    }
                    else if (skillType == typeof(HeavySnipe))
                    {
                        (skillLocator.primary.stateMachine.state as HeavySnipe).AutoReload();
                    }
                    else if (skillType == typeof(ReloadHeavySnipe))
                    {
                        (skillLocator.primary.stateMachine.state as ReloadHeavySnipe).AutoReload();
                    }
                    else if (skillType == typeof(ReloadSnipe))
                    {
                        (skillLocator.primary.stateMachine.state as ReloadSnipe).AutoReload();
                    }
                    else if (skillType == typeof(FireBattleRifle))
                    {
                        DisableReloadBar();
                        SetReloadQuality(ReloadQuality.Good, false);
                        (skillLocator.primary.stateMachine.state as FireBattleRifle).AutoReload();
                    }
                    else if (skillType == typeof(ReloadBR))
                    {
                        DisableReloadBar();
                        SetReloadQuality(ReloadQuality.Good, false);
                        (skillLocator.primary.stateMachine.state as ReloadBR).AutoReload();
                    }
                    else
                    {
                        switch (skillLocator.primary.skillDef.skillName)
                        {
                            case "Snipe":
                            case "ReloadSnipe":
                            case "HeavySnipe":
                            case "ReloadHeavySnipe":
                                if (GetReloadQuality() != ReloadQuality.Perfect)
                                {
                                    SetReloadQuality(ReloadQuality.Perfect, false);
                                    hideLoadIndicator = false;
                                }
                                break;
                            case "BattleRifle":
                            case "ReloadBR":
                                if (skillLocator.primary.stock < skillLocator.primary.maxStock)
                                {
                                    DisableReloadBar();
                                    SetReloadQuality(ReloadQuality.Good, false);
                                }
                                break;
                            default:
                                DisableReloadBar();
                                break;
                        }
                    }

                    if (skillLocator.primary.maxStock > 1)
                    {
                        skillLocator.primary.stock = skillLocator.primary.maxStock;
                    }
                }
            }
            finishedReload = true;
        }

        public void ResetReloadQuality()
        {
            currentReloadQuality = ReloadQuality.Bad;
        }

        public void BattleRiflePerfectReload()
        {
            if (characterBody.HasBuff(Modules.SniperContent.spotterPlayerCooldownBuff.buffIndex))
            {
                characterBody.ClearTimedBuffs(Modules.SniperContent.spotterPlayerCooldownBuff.buffIndex);
            }
        }

        //The sound code is duplicated in RpcPlayReloadSound because the reload sound should be instantaneous for the client triggering it, while it's ok if it's delayed for other players.
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
            if (!this.hasAuthority)
            {
                Util.PlaySound(ReloadController.pingSoundString, base.gameObject);
            }
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

            characterBody = base.GetComponent<CharacterBody>();
            healthComponent = characterBody.healthComponent;
            skillLocator = characterBody.skillLocator;
        }

        private float ScaleToScreen(float pixelValue)   //Everything is based on 1080p screren
        {
            return pixelValue * Screen.height * reloadBarScale * screenFraction;
        }

        private Vector2 CenterRect(Rect rect, float xOffset = 0f, float yOffset = 0f)
        {
            return new Vector2(Screen.width / 2 - rect.width / 2 + xOffset, Screen.height / 2 - rect.height / 2 + yOffset);
        }

        public void EnableReloadBar(float reloadBarLength, bool brReload = false, float brReloadTime = 0f)
        {
            standardReload = !brReload;
            isReloading = true;
            hideLoadIndicator = brReload;
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

            rectBar.width = ScaleToScreen(144f) * (reloadReverse ? -1f : 1f);
            rectBar.height = ScaleToScreen(24f);

            rectCursor.width = ScaleToScreen(24f);
            rectCursor.height = ScaleToScreen(24f);

            rectIndicator.width = ScaleToScreen(48f);
            rectIndicator.height = ScaleToScreen(48f);

            rectBar.position = new Vector2(Screen.width / 2 - rectBar.width / 2, Screen.height / 2 + 3 * rectBar.height / 2);
            barLeftBound = Screen.width / 2 - (Screen.height * 80f * reloadBarScale * screenFraction); // 80 used to be -68-12
            rectCursor.position = new Vector2(barLeftBound, Screen.height / 2 + rectCursor.width / 2 + rectBar.height);

            rectBar2.width = ScaleToScreen(400f);
            rectBar2.height = ScaleToScreen(36f);
            rectBar2.position = CenterRect(rectBar2, 0f, ScaleToScreen(bar2VerticalOffset));

            rectSlider2.width = ScaleToScreen(5f);
            rectSlider2.height = ScaleToScreen(24f);
            rectSlider2.position = CenterRect(rectSlider2, ScaleToScreen(-196f), ScaleToScreen(bar2VerticalOffset));

            bar2LeftBound = rectSlider2.position.x;

            float attackSpeed = Mathf.Max(characterBody.attackSpeed, 1f);
            float scaledPerfectLength = (basePerfectEndPercent - basePerfectBeginPercent) * attackSpeed;
            float scaledGoodLength = (baseGoodEndPercent - baseGoodBeginPercent) * attackSpeed;
            float regionHeight = ScaleToScreen(8f);
            float perfectStart = 0f;
            float perfectEnd = 0f;
            float goodStart = 0f;
            float goodEnd = 0f;

            if (standardReload)
            {
                perfectBeginPercent = basePerfectBeginPercent;
                perfectEndPercent = Mathf.Min(1f, perfectBeginPercent + scaledPerfectLength);
                goodBeginPercent = perfectEndPercent;
                goodEndPercent = Mathf.Min(1f, goodBeginPercent + scaledGoodLength);
            }
            else
            {
                perfectEndPercent = 1f - basePerfectBeginPercent;
                perfectBeginPercent = Mathf.Max(0f, perfectEndPercent - scaledPerfectLength);
                goodEndPercent = perfectBeginPercent;
                goodBeginPercent = Mathf.Max(0f,goodEndPercent - scaledGoodLength);
            }


            perfectStart = ScaleToScreen(perfectBeginPercent * bar2PixelLength);
            perfectEnd = ScaleToScreen(perfectEndPercent * bar2PixelLength);

            goodStart = ScaleToScreen(goodBeginPercent * bar2PixelLength);
            goodEnd = ScaleToScreen(goodEndPercent * bar2PixelLength);

            float xScaled = ScaleToScreen(6f);
            float yScaled = ScaleToScreen(14f);

            rectPerfect.width = perfectEnd - perfectStart;
            rectPerfect.height = Mathf.Max(regionHeight, 1f);
            rectPerfect.position = rectBar2.position + new Vector2(xScaled + perfectStart, yScaled);   //6,14 is offset for dead space in the sprite

            rectGood.width = goodEnd - goodStart;
            rectGood.height = Mathf.Max(regionHeight, 1f);
            rectGood.position = rectBar2.position + new Vector2(xScaled + goodStart, yScaled);   //6,14 is offset for dead space in the sprite
        }

        private void OnGUI()
        {
            if (this.hasAuthority && !menuActive && !RoR2.PauseManager.isPaused && healthComponent && healthComponent.alive)
            {
                if (isReloading && !finishedReload)
                {
                    //GUI.DrawTexture(rectBar, failedReload ? reloadBarFail : reloadBar, ScaleMode.StretchToFill, true, 0f);
                    //GUI.DrawTexture(rectCursor, failedReload ? reloadCursorFail : reloadCursor, ScaleMode.StretchToFill, true, 0f);

                    //Draw bar
                    GUI.DrawTexture(rectBar2, reloadLingerTimer > 0f ? reloadBar2BorderFinish : reloadBar2Border, ScaleMode.StretchToFill, true, 0f);
                    GUI.DrawTexture(rectBar2, reloadBar2, ScaleMode.StretchToFill, true, 0f);

                    //Draw reload region
                    GUI.DrawTexture(rectGood, failedReload ? reloadRegionGoodFail : reloadRegionGood, ScaleMode.StretchToFill, true, 0f);
                    GUI.DrawTexture(rectPerfect, failedReload ? reloadRegionPerfectFail : reloadRegionPerfect, ScaleMode.StretchToFill, true, 0f);

                    //Draw reload feedback
                    if (failedReload)
                    {
                        GUI.DrawTexture(rectBar2, reloadBar2Fail, ScaleMode.StretchToFill, true, 0f);
                    }
                    else if (reloadLingerTimer > 0f)
                    {
                        GUI.DrawTexture(rectBar2, currentReloadQuality == ReloadQuality.Perfect ? reloadBar2Perfect : reloadBar2Good, ScaleMode.StretchToFill, true, 0f);
                    }

                    //Draw slider
                    GUI.DrawTexture(rectSlider2, failedReload ? reloadSlider2Fail : reloadSlider2, ScaleMode.StretchToFill, true, 0f);
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
                reloadProgress += Time.fixedDeltaTime * 1f * (reloadMovingBackwards ? -1f : 1f);
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
                //rectCursor.position = new Vector2(barLeftBound + (Screen.height * reloadProgress / reloadLength * 136f * reloadBarScale * screenFraction), rectCursor.position.y);

                rectSlider2.position = new Vector2(bar2LeftBound + ScaleToScreen(reloadProgress / reloadLength * bar2PixelLength), rectSlider2.position.y);
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
                if (reloadPercent >= perfectBeginPercent && reloadPercent <= perfectEndPercent)
                {
                    r = ReloadQuality.Perfect;
                }
                else if (reloadPercent >= goodBeginPercent && reloadPercent <= goodEndPercent)
                {
                    r = ReloadQuality.Good;
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
            hideLoadIndicator = false;
            if (!this.hasAuthority)
            {
                return;
            }
            reloadLingerTimer = lingerTimer;
            pauseReload = true;
            float reloadPercent = reloadProgress / reloadLength;
            ReloadQuality r = ReloadQuality.Bad;
            if (reloadPercent >= perfectBeginPercent && reloadPercent <= perfectEndPercent)
            {
                r = ReloadQuality.Perfect;
            }
            else if (reloadPercent >= goodBeginPercent && reloadPercent <= goodEndPercent)
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
            triggeredBRReload = true;
            pauseReload = true;
            reloadLingerTimer = 0f;
            isReloading = false;
            failedReload = false;
            reloadProgress = 0f;
        }

        //public static Texture2D reloadBar = null;
        //public static Texture2D reloadCursor = null;
        public static Texture2D indicatorGood = null;
        public static Texture2D indicatorPerfect = null;

        //public static Texture2D reloadBarFail = null;
        //public static Texture2D reloadCursorFail = null;

        public static Texture2D reloadBar2Border = null;
        public static Texture2D reloadBar2BorderFinish = null;
        public static Texture2D reloadBar2 = null;
        public static Texture2D reloadBar2Fail = null;
        public static Texture2D reloadBar2Good = null;
        public static Texture2D reloadBar2Perfect = null;
        public Rect rectBar2 = new Rect();
        private float bar2LeftBound = 0f;
        private float bar2PixelLength = 388f;
        private float bar2VerticalOffset = 140f;

        public static Texture2D reloadSlider2 = null;
        public static Texture2D reloadSlider2Fail = null;
        private Rect rectSlider2 = new Rect();

        public static string boltReloadSoundString = "Play_SniperClassic_reload_bolt";
        public static string failSoundString = "Play_commando_M2_grenade_throw";
        public static string pingSoundString = "Play_SniperClassic_m1_br_ping";
        public static string goodReloadSoundString = "Play_SniperClassic_reload_good";
        public static string perfectReloadSoundString = "Play_SniperClassic_reload_perfect";
        public static string casingSoundString = "Play_SniperClassic_casing";
        public static float reloadBarScale = 1f;
        public static float reloadIndicatorScale = 1f;

        public bool brReload = false;
        public bool failedReload = false;
        public bool menuActive = false;

        private float reloadProgress = 0f;
        private float barLeftBound;
        private ReloadQuality currentReloadQuality = ReloadQuality.Bad;
        private float brReloadDuration = 0.5f;

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
        private Rect rectBar = new Rect();
        private Rect rectCursor = new Rect();
        private Rect rectIndicator = new Rect();
        public enum ReloadQuality
        {
            Bad, Good, Perfect
        }

        internal const float basePerfectBeginPercent = 0.15f / 0.6f;
        internal const float basePerfectEndPercent = 0.255f / 0.6f;
        internal const float baseGoodBeginPercent = basePerfectEndPercent;
        internal const float baseGoodEndPercent = 0.38f / 0.6f;
        internal const float screenFraction = 1f / 1080f;


        public static Texture2D reloadRegionGood = null;
        public static Texture2D reloadRegionPerfect = null;
        public static Texture2D reloadRegionGoodFail = null;
        public static Texture2D reloadRegionPerfectFail = null;

        public float perfectBeginPercent;
        public float perfectEndPercent;
        public float goodBeginPercent;
        public float goodEndPercent;
        private Rect rectGood = new Rect();
        private Rect rectPerfect = new Rect();

        public bool finishedReload = false;
    }
}
