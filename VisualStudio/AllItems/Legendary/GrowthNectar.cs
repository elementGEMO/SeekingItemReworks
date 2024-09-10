using System;
using static SeekingItemReworks.ColorCode;

namespace SeekerItems.Legendary
{
    internal class GrowthNectar : RiskItem
    {
        public GrowthNectar() : base(
            "BOOSTALLSTATS",
            "Not using an equipment increases ALL of your stats. Scales with equipment charges.",
            string.Format("When your Equipment is off " + "cooldown".Style(Color.cIsUtility) + ", increase " + "ALL stats ".Style(Color.cIsUtility) + "by " + "{0}% ".Style(Color.cIsUtility) + "(+{1}% per stack)".Style(Color.cStack) + ", and by " + "{2}% ".Style(Color.cIsUtility) + "per " + "Equipment charge".Style(Color.cIsUtility) + ", up to a maximum of " + "{3}% ".Style(Color.cIsUtility) + "(+{4}% per stack)".Style(Color.cStack) + ".",
                Math.Round(MainConfig.LGN_SBase.Value, roundVal), Math.Round(MainConfig.LGN_SStack.Value, roundVal), Math.Round(MainConfig.LGN_Charge.Value, roundVal), Math.Round(MainConfig.LGN_CBase.Value, roundVal), Math.Round(MainConfig.LGN_CStack.Value, roundVal))
        ) {} 
    }
}
