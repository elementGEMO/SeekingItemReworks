using System;
using static SeekingItemReworks.ColorCode;

namespace SeekerItems.Legendary
{
    internal class WarBonds : RiskItem
    {
        public WarBonds() : base(
            "GOLDONSTAGESTART",
            "Gain free purchases at the start of each stage.\nGain 20% experience on cash spent on all purchases.",
            string.Format("Gain " + "{0} ".Style(Color.cIsUtility) + "(+{1} per stack) ".Style(Color.cStack) + "free purchases ".Style(Color.cIsUtility) + "at the " + "start of each stage".Style(Color.cIsUtility) + ". When making a gold purchase, get " + "{2}% ".Style(Color.cIsUtility) + "(+{3}% per stack) ".Style(Color.cStack) + "of spent gold as " + "experience".Style(Color.cIsUtility) + ".",
                MainConfig.LWB_PBase.Value, MainConfig.LWB_PStack.Value, Math.Round(MainConfig.LWB_EBase.Value, roundVal), Math.Round(MainConfig.LWB_EStack.Value, roundVal))
        ) {} 
    }
}
