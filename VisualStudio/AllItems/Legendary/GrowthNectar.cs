using BepInEx.Configuration;
using System;
using static SeekingItemReworks.ColorCode;

namespace SeekerItems
{
    internal class GrowthNectar : ItemTemplate
    {
        public GrowthNectar(int descType)
        {
            ItemInternal = "BOOSTALLSTATS";

            if (descType == 1)
            {
                ItemInfo = "Not using an equipment increases ALL of your stats. Scales with equipment charges.";
                ItemDesc = string.Format(
                    "When your Equipment is off " + "cooldown".Style(FontColor.cIsUtility) + ", increase " + "ALL stats ".Style(FontColor.cIsUtility) + "by " + "{0}% ".Style(FontColor.cIsUtility) + "(+{1}% per stack)".Style(FontColor.cStack) + ", and by " + "{2}% ".Style(FontColor.cIsUtility) + "per " + "Equipment charge".Style(FontColor.cIsUtility) + ", up to a maximum of " + "{3}% ".Style(FontColor.cIsUtility) + "(+{4}% per stack)".Style(FontColor.cStack) + ".",
                    RoundVal(Stat_Base.Value), RoundVal(Stat_Stack.Value),
                    RoundVal(Charge_Stat_Increase.Value),
                    RoundVal(Charge_Cap_Base.Value), RoundVal(Charge_Cap_Stack.Value)
                );
            }
        }

        public static string StaticName = "Growth Nectar";

        public static ConfigEntry<int> Rework;

        public static ConfigEntry<float> Stat_Base;
        public static ConfigEntry<float> Stat_Stack;
        public static ConfigEntry<float> Charge_Stat_Increase;
        public static ConfigEntry<float> Charge_Cap_Base;
        public static ConfigEntry<float> Charge_Cap_Stack;
    }
}
