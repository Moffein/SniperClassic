using BepInEx;
using System;
using System.Collections.Generic;
using System.Text;
using RoR2;
using UnityEngine;

namespace SniperClassic.Controllers.SmokeGrenade
{
    public class SmokeSound : MonoBehaviour
    {
        public void Start()
        {
            Util.PlaySound("Play_clayboss_M1_explo", this.gameObject);
            Destroy(this);
        }
    }
}
