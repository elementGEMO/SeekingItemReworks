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
                    RoundVal(Experience_Percent_Base.Value)
                );
                ItemDesc = string.Format(
                    "Gain " + "{0} ".Style(FontColor.cIsUtility) + "(+{1} per stack) ".Style(FontColor.cStack) + "free purchases ".Style(FontColor.cIsUtility) + "at the " + "start of each stage".Style(FontColor.cIsUtility) + ". When making a gold purchase, get " + "{2}% ".Style(FontColor.cIsUtility) + "(+{3}% per stack) ".Style(FontColor.cStack) + "of spent gold as " + "experience".Style(FontColor.cIsUtility) + ".",
                    Purchase_Base.Value, Purchase_Stack.Value, RoundVal(Experience_Percent_Base.Value), RoundVal(Experience_Percent_Stack.Value)
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
