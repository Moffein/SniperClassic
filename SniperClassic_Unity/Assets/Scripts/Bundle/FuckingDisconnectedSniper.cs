using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SniperClassic {
    public class FuckingDisconnectedSniper : MonoBehaviour {
        [SerializeField]
        private Transform fuckingSniper;

        [SerializeField]
        private Animator fuckingAnimator;

        [SerializeField]
        private bool pos;
        [SerializeField]
        private bool rot;

        private void LateUpdate() {

            if (fuckingSniper) {
                if (fuckingAnimator && fuckingAnimator.GetFloat("aiming") < 0.5f) {
                    if (pos)
                        fuckingSniper.position = transform.position;
                    if (rot)
                        fuckingSniper.rotation = transform.rotation;
                }
            }
        }
    }
}