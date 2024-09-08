using System;
using static SeekingItemReworks.ColorCode;

namespace SeekerItems.Uncommon
{
    internal class UnstableTransmitter : RiskItem
    {
        public UnstableTransmitter() : base(
            "TELEPORTONLOWHEALTH",
            "Become intangible and explode, bleeding nearby enemies at low health.",
            string.Format("Falling below " + "25% health ".Style(Color.cIsHealth) + "causes you to fade away, becoming " + "intangible ".Style(Color.cIsUtility) + "and exploding, inflicting " + "bleed ".Style(Color.cIsDamage) + "to enemies within " + "30m ".Style(Color.cIsDamage) + "for " + "{0}% ".Style(Color.cIsDamage) + "(+{1}% per stack) ".Style(Color.cStack) + "base damage. Lasts " + "2s ".Style(Color.cIsUtility) + "(+1s per stack)".Style(Color.cStack) + ". Recharges every " + "60 ".Style(Color.cIsUtility) + "seconds.",
                Math.Round(MainConfig.UUT_BBase.Value, roundVal), Math.Round(MainConfig.UUT_BStack.Value, roundVal), Math.Round(MainConfig.UUT_DBase.Value, roundVal), Math.Round(MainConfig.UUT_DStack.Value, roundVal))
        ) {} 
    }
}
