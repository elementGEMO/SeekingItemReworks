using System;
using static SeekingItemReworks.ColorCode;

namespace SeekerItems.Common
{
    internal class AntlerShield : RiskItem
    {
        public AntlerShield() : base(
            "NEGATEATTACK",
            "Gain armor from movement speed, and increase movement speed.",
            "Increase " + "armor ".Style(Color.cIsHealing) + "by " + "5% ".Style(Color.cIsHealing) + "(+5% per stack) ".Style(Color.cStack) + "of " + "current movement speed".Style(Color.cIsUtility) + ". Increase " + "movement speed ".Style(Color.cIsUtility) + "by " + "7% ".Style(Color.cIsUtility) + "(+3.5% per stack)".Style(Color.cStack) + "."
        ) { }
    }
}
