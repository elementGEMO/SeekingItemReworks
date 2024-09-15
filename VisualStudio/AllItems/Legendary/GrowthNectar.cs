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
                    "When your Equipment is off " + "cooldown".Style(Color.cIsUtility) + ", increase " + "ALL stats ".Style(Color.cIsUtility) + "by " + "{0}% ".Style(Color.cIsUtility) + "(+{1}% per stack)".Style(Color.cStack) + ", and by " + "{2}% ".Style(Color.cIsUtility) + "per " + "Equipment charge".Style(Color.cIsUtility) + ", up to a maximum of " + "{3}% ".Style(Color.cIsUtility) + "(+{4}% per stack)".Style(Color.cStack) + ".",
                    Math.Round(Stat_Base.Value, roundVal), Math.Round(Stat_Stack.Value, roundVal),
                    Math.Round(Charge_Stat_Increase.Value, roundVal),
                    Math.Round(Charge_Cap_Base.Value, roundVal), Math.Round(Charge_Cap_Stack.Value, roundVal)
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
