using HG;
using System;
using System.Text;
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
            string.Format("Incoming ".Style(Color.cIsDamage) + "non-lethal ".Style(Color.cIsHealth) + "damage ".Style(Color.cIsDamage) + "is split " + "50%".Style(Color.cIsDamage) + ", with the other " + "50% ".Style(Color.cIsDamage) + "delayed for " + "{0}s ".Style(Color.cIsDamage) + "(+{1}s per stack)".Style(Color.cStack) + ". Recharges when damage is no longer delayed.",
                Math.Round(MainConfig.CKF_KBase.Value, roundVal), Math.Round(MainConfig.CKF_KStack.Value, roundVal)),

            "Half of incoming damage is delayed.",
            string.Format("Incoming ".Style(Color.cIsDamage) + "non-lethal ".Style(Color.cIsHealth) + "damage ".Style(Color.cIsDamage) + "is split " + "50%".Style(Color.cIsDamage) + ", with the other " + "50% ".Style(Color.cIsDamage) + "delayed for " + "3s ".Style(Color.cIsDamage) + "(+1 instance per stack)".Style(Color.cStack) + ". Recharges every " + "10 ".Style(Color.cIsUtility) + "seconds.",
                Math.Round(MainConfig.CKF_KBase.Value, roundVal), Math.Round(MainConfig.CKF_KStack.Value, roundVal))
        ) {} 
    }
}
