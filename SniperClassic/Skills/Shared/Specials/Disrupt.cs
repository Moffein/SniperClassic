using EntityStates.SniperClassicSkills;
using SniperClassic;
using System;
using System.Collections.Generic;
using System.Text;

namespace EntityStates.SniperClassicSkills
{
    class SendSpotterDisrupt : SendSpotter
    {
        public override void SetSpotterMode()
        {
            spotterMode = SpotterMode.Disrupt;
        }
    }
}
