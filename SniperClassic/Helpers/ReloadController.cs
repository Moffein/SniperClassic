using EntityStates.Commando.CommandoWeapon;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace SniperClassic
{
    class ReloadController : NetworkBehaviour
    {
        public void SetReloadQuality(ReloadQuality r)
        {
            this.currentReloadQuality = r;
            switch (this.currentReloadQuality)
            {
                case ReloadQuality.Good:
                    Util.PlaySound(ReloadController.goodReloadSoundString, base.gameObject);
                    break;
                case ReloadQuality.Perfect:
                    Util.PlaySound(ReloadController.perfectReloadSoundString, base.gameObject);
                    break;
                default:
                    Util.PlaySound(ReloadController.badReloadSoundString, base.gameObject);
                    break;
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

            rectBar.width = Screen.height * 144f / 1080f;
            rectBar.height = Screen.height * 24f / 1080f;

            rectCursor.width = Screen.height * 24f / 1080f;
            rectCursor.height = Screen.height * 24f / 1080f;

            rectIndicator.width = Screen.height * 48f / 1080f;
            rectIndicator.height = Screen.height * 48f / 1080f;

            rectBar.position = new Vector2(Screen.width / 2 - rectBar.width / 2, Screen.height / 2 + 3 * rectBar.height / 2);
            barLeftBound = Screen.width / 2 - (Screen.height * 80f / 1080f); // 80 used to be -68-12
            rectCursor.position = new Vector2(barLeftBound, Screen.height / 2 + rectCursor.width / 2 + rectBar.height);
        }

        public void EnableReloadBar()
        {
            isReloading = true;
            hideLoadIndicator = false;
            reloadProgress = 0f;

            rectBar.width = Screen.height * 144f * reloadBarScale / 1080f;
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
            if (this.hasAuthority && !RoR2.PauseManager.isPaused)
            {
                if (isReloading)
                {
                    GUI.DrawTexture(rectBar, reloadBar, ScaleMode.StretchToFill, true, 0f);
                    GUI.DrawTexture(rectCursor, reloadCursor, ScaleMode.StretchToFill, true, 0f);
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
            reloadProgress = 0f;
        }

        public static Texture2D reloadBar = null;
        public static Texture2D reloadCursor = null;
        public static Texture2D indicatorGood = null;
        public static Texture2D indicatorPerfect = null;
        public static string badReloadSoundString = "Play_commando_M2_grenade_throw";
        public static string goodReloadSoundString = "Play_bandit_M1_pump";
        public static string perfectReloadSoundString = "Play_captain_m1_reload";
        public static float reloadBarScale = 1.2f;
        public static float reloadIndicatorScale = 1.0f;

        private float reloadProgress = 0f;
        private float barLeftBound;
        private ReloadQuality currentReloadQuality = ReloadQuality.Bad;

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
