using RoR2;
using UnityEngine;

namespace SniperClassic
{
    public class MenuSoundComponent : MonoBehaviour
    {
        private uint playIDReload = 0;

        private void OnEnable()
        {                              //Sounds.CharSelect
            this.playIDReload = Util.PlaySound(ReloadController.boltReloadSoundString, base.gameObject);
        }
        private void OnDestroy()
        {
            if (this.playIDReload != 0) AkSoundEngine.StopPlayingID(this.playIDReload);
        }
    }
}