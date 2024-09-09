using System;
using static SeekingItemReworks.ColorCode;

namespace SeekerItems.Common
{
    internal class AntlerShield : RiskItem
    {
        public AntlerShield() : base(
            "NEGATEATTACK",
            "Gain armor from movement speed, and increase movement speed.",
            string.Format("Increase " + "armor ".Style(Color.cIsHealing) + "by " + "{0}% ".Style(Color.cIsHealing) + "(+{1}% per stack) ".Style(Color.cStack) + "of " + "current movement speed".Style(Color.cIsUtility) + ". Increase " + "movement speed ".Style(Color.cIsUtility) + "by " + "{2}% ".Style(Color.cIsUtility) + "(+{3}% per stack)".Style(Color.cStack) + ".",
                Math.Round(MainConfig.CAS_ABase.Value, roundVal), Math.Round(MainConfig.CAS_AStack.Value, roundVal), Math.Round(MainConfig.CAS_MBase.Value, roundVal), Math.Round(MainConfig.CAS_MStack.Value, roundVal))
        ) { }
    }
}
