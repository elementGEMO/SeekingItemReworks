using BepInEx.Configuration;
using System;
using static SeekingItemReworks.ColorCode;

namespace SeekerItems
{
    internal class ChronicExpansion : ItemTemplate
    {
        public ChronicExpansion(int descType)
        {
            ItemInternal = "INCREASEDAMAGEONMULTIKILL";
            if (descType == 1)
            {
                ItemInfo = "Gain stacking damage after killing 5 enemies while in combat.";
                ItemDesc = string.Format(
                    "Killing 5 enemies ".Style(Color.cIsDamage) + "in combat buffs damage by " + "{0}% ".Style(Color.cIsDamage) + "(+{1}% per stack) ".Style(Color.cStack) + "temporarily each time, until leaving combat.",
                    Math.Round(Damage_Base.Value, roundVal), Math.Round(Damage_Stack.Value, roundVal)
                );
            }
        }

        public static string StaticName = "Chronic Expansion";

        public static ConfigEntry<int> Rework;
        public static ConfigEntry<float> Damage_Base;
        public static ConfigEntry<float> Damage_Stack;
    }
}
