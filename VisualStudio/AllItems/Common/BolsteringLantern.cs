using SeekingItemReworks;
using System;
using static SeekingItemReworks.ColorCode;

namespace SeekerItems.Common
{
    internal class BolsteringLantern : RiskItem
    {
        public BolsteringLantern() : base(
            "LOWERHEALTHHIGHERDAMAGE",
            string.Format("Ignite enemies at low health until health is restored {0}%.",
               Math.Max(Math.Min(MainConfig.CBL_HealthAbove.Value, 100), 0)),
            string.Format("Falling below " + "{0}% health".Style(Color.cIsHealth) + " grants a buff that " + "ignites ".Style(Color.cIsDamage) + "enemies for " + "{1}% ".Style(Color.cIsDamage) + "(+{2}% per stack) ".Style(Color.cStack) + "base damage on hit, until " + "health ".Style(Color.cIsHealing) + "is restored to " + "{3}%".Style(Color.cIsHealing) + ".",
                Math.Max(Math.Min(MainConfig.CBL_HealthBelow.Value, 100), 0), Math.Round(MainConfig.CBL_BBase.Value, roundVal),
                Math.Round(MainConfig.CBL_BStack.Value, roundVal), Math.Max(Math.Min(MainConfig.CBL_HealthAbove.Value, 100), 0)),
            "Chance on hit to ignite. Inherited by allies.",
            string.Format("{0}% ".Style(Color.cIsDamage) + "(+{1}% per stack) ".Style(Color.cStack) + "chance on hit to " + "ignite ".Style(Color.cIsDamage) + "enemies for " + "{2}% ".Style(Color.cIsDamage) + "(+{3}% per stack) ".Style(Color.cStack) + "base damage. This item is " + "inherited by allies ".Style(Color.cIsUtility) + "and is boosted with " + "Ignition Tank".Style(Color.cIsUtility) + ".",
                Math.Round(MainConfig.CBL_ProcBase.Value, roundVal), Math.Round(MainConfig.CBL_ProcStack.Value, roundVal),
                Math.Round(MainConfig.CBL_BBase.Value, roundVal), Math.Round(MainConfig.CBL_BStack.Value, roundVal))
        ) { } 
    }
}
