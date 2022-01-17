using EntityStates.SniperClassicSkills;
using SniperClassic;
using System;
using System.Collections.Generic;
using System.Text;

namespace EntityStates.SniperClassicSkills
{
    class SendSpotterScepter : SendSpotter
    {
        public override void SetSpotterMode()
        {
            spotterMode = SpotterMode.ChainLightningScepter;
        }
    }
}
