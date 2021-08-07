using BepInEx;
using System;
using System.Collections.Generic;
using System.Text;
using RoR2;

namespace SniperClassic.Controllers.SmokeGrenade
{
    public class SmokeSound : BaseUnityPlugin
    {
        public void Start()
        {
            Util.PlaySound("Play_clayboss_M1_explo", this.gameObject);
            Destroy(this);
        }
    }
}
