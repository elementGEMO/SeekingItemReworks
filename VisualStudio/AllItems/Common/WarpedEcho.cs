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
            string.Format("Incoming ".Style(Color.cIsDamage) + "non-lethal ".Style(Color.cIsHealth) + "damage ".Style(Color.cIsDamage) + "is split " + "50%".Style(Color.cIsDamage) + ", with the other " + "50% ".Style(Color.cIsDamage) + "delayed for " + "{0}s ".Style(Color.cIsDamage) + "(+{1}s per stack)".Style(Color.cStack) + ". Recharges when damage is no longer delayed.",
                Math.Round(MainConfig.CWE_Delay.Value, roundVal), Math.Round(MainConfig.CWE_Stack.Value, roundVal)),

            "Half of incoming damage is delayed.",
            "Incoming ".Style(Color.cIsDamage) + "non-lethal ".Style(Color.cIsHealth) + "damage ".Style(Color.cIsDamage) + "is split " + "50%".Style(Color.cIsDamage) + ", with the other " + "50% ".Style(Color.cIsDamage) + "delayed for " + "3s ".Style(Color.cIsDamage) + "(+1 instance per stack)".Style(Color.cStack) + ". Recharges every " + "10 ".Style(Color.cIsUtility) + "seconds."
        )
        { } 
    }
}
