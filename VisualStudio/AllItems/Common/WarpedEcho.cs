using System;
using static SeekingItemReworks.ColorCode;

namespace SeekingItemReworks.AllItems.Common
{
    internal class WarpedEcho : RiskItem
    {
        static readonly string descOne = $"{MainConfig.CWE_Delay.Value.ToString($"F{MainConfig.RoundNumber.Value}")}";
        static readonly string descTwo = $"{MainConfig.CWE_Stack.Value.ToString($"F{MainConfig.RoundNumber.Value}")}";
        public WarpedEcho() : base(
            "DELAYEDDAMAGE",
            "Half of incoming damage is delayed.",
            "Incoming ".Style(Color.cIsDamage) + "non-lethal ".Style(Color.cIsHealth) + "damage ".Style(Color.cIsDamage) + "is split " + "50%".Style(Color.cIsDamage) + ", with the other " + "50% ".Style(Color.cIsDamage) + "delayed for " + (descOne + "s ").Style(Color.cIsDamage) + ("(+" + descTwo + "s per stack)").Style(Color.cStack) + ". Recharges after damage is delayed.",
            
            "Half of incoming damage is delayed.",
            "Incoming ".Style(Color.cIsDamage) + "non-lethal ".Style(Color.cIsHealth) + "damage ".Style(Color.cIsDamage) + "is split " + "50%".Style(Color.cIsDamage) + ", with the other " + "50% ".Style(Color.cIsDamage) + "delayed for " + "3s ".Style(Color.cIsDamage) + "(+1 instance per stack)".Style(Color.cStack) + ". Recharges every " + "10 ".Style(Color.cIsUtility) + "seconds."
        ) {} 
    }
}
