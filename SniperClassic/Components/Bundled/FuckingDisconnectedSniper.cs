using UnityEngine;

namespace SniperClassic {
    //stupid problems get stupid solutions
    //I swear the disconnected rig has added probably days to this development time
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