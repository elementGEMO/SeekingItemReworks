using SeekingItemReworks;
using System;
using static SeekingItemReworks.ColorCode;

namespace SeekerItems.Common
{
    internal class WarpedEcho : RiskItem
    {
        public WarpedEcho() : base(
            "DELAYEDDAMAGE",
            "Half of incoming damage is delayed.",
            string.Format("Incoming damage ".Style(Color.cIsDamage) + "is split " + "50%".Style(Color.cIsDamage) + ", with the other " + "50% ".Style(Color.cIsDamage) + "delayed for " + "{0}s ".Style(Color.cIsDamage) + "(+{1} instances per stack)".Style(Color.cStack) + ". Recharges one instance after delayed damage.",
                Math.Round(MainConfig.CWE_Delay.Value, roundVal), MainConfig.CWE_Stack.Value)
        ) { } 
    }
}
