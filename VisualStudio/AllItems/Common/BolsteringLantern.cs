using BepInEx.Configuration;
using System;
using static SeekingItemReworks.ColorCode;

namespace SeekerItems
{
    internal class BolsteringLantern : ItemTemplate
    {
        public BolsteringLantern(int descType)
        {
            ItemInternal = "LOWERHEALTHHIGHERDAMAGE";

            if (descType == 1)
            {
                ItemInfo = string.Format(
                    "Ignite enemies at low health until health is restored {0}%.",
                    HighHealth.Value
                );
                ItemDesc = string.Format(
                    "Falling below " + "{0}% health".Style(FontColor.cIsHealth) + " grants a buff that " + "ignites ".Style(FontColor.cIsDamage) + "enemies for " + "{1}% ".Style(FontColor.cIsDamage) + "(+{2}% per stack) ".Style(FontColor.cStack) + "base damage on hit, until " + "health ".Style(FontColor.cIsHealing) + "is restored to " + "{3}%".Style(FontColor.cIsHealing) + ".",
                    LowHealth.Value, Math.Round(Damage_Base.Value, roundVal),
                    Math.Round(Damage_Stack.Value, roundVal), HighHealth.Value
                );
            }
            else if (descType == 2)
            {
                ItemInfo = "Chance on hit to ignite. Inherited by allies.";
                ItemDesc = string.Format(
                    "{0}% ".Style(FontColor.cIsDamage) + "(+{1}% per stack) ".Style(FontColor.cStack) + "chance on hit to " + "ignite ".Style(FontColor.cIsDamage) + "enemies for " + "{2}% ".Style(FontColor.cIsDamage) + "(+{3}% per stack) ".Style(FontColor.cStack) + "base damage. This item is " + "inherited by allies ".Style(FontColor.cIsUtility) + "and is boosted with " + "Ignition Tank".Style(FontColor.cIsUtility) + ".",
                    Math.Round(Chance_Base.Value, roundVal), Math.Round(Chance_Stack.Value, roundVal),
                    Math.Round(Damage_Base.Value, roundVal), Math.Round(Damage_Stack.Value, roundVal)
                );
            }
        }

        public static string StaticName = "Bolstering Lantern";

        public static ConfigEntry<int> Rework;

        public static ConfigEntry<int> LowHealth;
        public static ConfigEntry<int> HighHealth;

        public static ConfigEntry<float> Damage_Base;
        public static ConfigEntry<float> Damage_Stack;
        public static ConfigEntry<float> Chance_Base;
        public static ConfigEntry<float> Chance_Stack;
    }
}
