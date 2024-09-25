using BepInEx.Configuration;
using System;
using static SeekingItemReworks.ColorCode;

namespace SeekerItems
{
    internal class ChanceDoll : ItemTemplate
    {
        public ChanceDoll(int descType)
        {
            ItemInternal = "EXTRASHRINEITEM";

            if (descType == 1)
            {
                ItemInfo = "Chance to upgrade the item reward on failing a Shrine.";
                ItemDesc = string.Format(
                    "Failing a Shrine increases the chance for the item reward to " + "upgrade ".Style(FontColor.cIsUtility) + "by " + "{0}% ".Style(FontColor.cIsUtility) + "(+{1}% per stack)".Style(FontColor.cStack) + ", up to a " + "Legendary ".Style(FontColor.cIsHealth) + "item.",
                    RoundVal(Chance_Base.Value), RoundVal(Chance_Stack.Value)
                );
            }
            if (descType == 2)
            {
                ItemInfo = "Funny luck infusion.";
                ItemDesc = "Hi :3";
            }
        }
        public static string StaticName = "Chance Doll";

        public static ConfigEntry<int> Rework;

        public static ConfigEntry<float> Hidden_Chance;
        public static ConfigEntry<float> Chance_Base;
        public static ConfigEntry<float> Chance_Stack;
    }
}
