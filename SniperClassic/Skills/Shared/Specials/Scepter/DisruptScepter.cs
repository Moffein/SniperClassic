using EntityStates.SniperClassicSkills;
using SniperClassic;
using System;
using System.Collections.Generic;
using System.Text;

namespace EntityStates.SniperClassicSkills
{
    class SendSpotterDisruptScepter : SendSpotter
    {
        public override void SetSpotterMode()
        {
            spotterMode = SpotterMode.DisruptScepter;
        }
    }
}
