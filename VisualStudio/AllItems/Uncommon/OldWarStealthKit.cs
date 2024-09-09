using System;
using static SeekingItemReworks.ColorCode;

namespace SeekerItems.Uncommon
{
    internal class OldWarStealthKit : RiskItem
    {
        public OldWarStealthKit() : base(
            "PHASING",
            "Turn invisible and cleanse debuffs at low health.",
            "Falling below " + "25% health ".Style(Color.cIsHealth) + "causes you to become " + "invisible ".Style(Color.cIsUtility) + "for " + "5s".Style(Color.cIsUtility) + ", boost " + "movement speed ".Style(Color.cIsUtility) + "by " + "40% ".Style(Color.cIsUtility) + "and " + "cleanse 2 ".Style(Color.cIsUtility) + "debuffs ".Style(Color.cIsDamage) + "(+1 per stack)".Style(Color.cStack) + ". Recharges every " + "30 ".Style(Color.cIsUtility) + "(-50% per stack) ".Style(Color.cStack) + "seconds."
        ) { }
    }
}
