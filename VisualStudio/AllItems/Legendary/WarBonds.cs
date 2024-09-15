using BepInEx.Configuration;
using System;
using static SeekingItemReworks.ColorCode;

namespace SeekerItems
{
    internal class WarBonds : ItemTemplate
    {
        public WarBonds(int descType)
        {
            ItemInternal = "GOLDONSTAGESTART";

            if (descType == 1)
            {
                ItemInfo = string.Format(
                    "Gain free purchases at the start of each stage.\nGain {0}% experience on cash spent on all purchases.",
                    Math.Round(Experience_Percent_Base.Value, roundVal)
                );
                ItemDesc = string.Format(
                    "Gain " + "{0} ".Style(Color.cIsUtility) + "(+{1} per stack) ".Style(Color.cStack) + "free purchases ".Style(Color.cIsUtility) + "at the " + "start of each stage".Style(Color.cIsUtility) + ". When making a gold purchase, get " + "{2}% ".Style(Color.cIsUtility) + "(+{3}% per stack) ".Style(Color.cStack) + "of spent gold as " + "experience".Style(Color.cIsUtility) + ".",
                    Purchase_Base.Value, Purchase_Stack.Value, Math.Round(Experience_Percent_Base.Value, roundVal), Math.Round(Experience_Percent_Stack.Value, roundVal)
                );
            }
        }

        public static string StaticName = "War Bonds";

        public static ConfigEntry<int> Rework;

        public static ConfigEntry<int> Purchase_Base;
        public static ConfigEntry<int> Purchase_Stack;
        public static ConfigEntry<float> Experience_Percent_Base;
        public static ConfigEntry<float> Experience_Percent_Stack;
    }
}
