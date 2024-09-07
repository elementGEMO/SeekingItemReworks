using System;
using static SeekingItemReworks.ColorCode;

namespace SeekingItemReworks.AllItems.Common
{
    internal class ChronicExpansion : RiskItem
    {
        public ChronicExpansion() : base(
            "INCREASEDAMAGEONMULTIKILL",
            "Gain stacking damage after killing 5 enemies while in combat.",
            string.Format("Killing 5 enemies ".Style(Color.cIsDamage) + "in combat buffs damage by " + "{0}% ".Style(Color.cIsDamage) + "(+{1}% per stack) ".Style(Color.cStack) + "temporarily each time, until leaving combat.",
                Math.Round(MainConfig.CCE_Base.Value, roundVal), Math.Round(MainConfig.CCE_Stack.Value, roundVal))
        ) {} 
    }
}
