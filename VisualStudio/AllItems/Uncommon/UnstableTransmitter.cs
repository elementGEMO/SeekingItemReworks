using BepInEx.Configuration;
using System;
using static SeekingItemReworks.ColorCode;

namespace SeekerItems
{
    internal class UnstableTransmitter : ItemTemplate
    {
        public UnstableTransmitter(int descType)
        {
            ItemInternal = "TELEPORTONLOWHEALTH";

            if (descType == 1)
            {
                ItemInfo = "Become intangible and explode, bleeding nearby enemies at low health.";
                ItemDesc = string.Format(
                    "Falling below " + "{0}% health ".Style(Color.cIsHealth) + "causes you to fade away, becoming " + "intangible ".Style(Color.cIsUtility) + "and exploding, inflicting " + "bleed ".Style(Color.cIsDamage) + "to enemies within " + "{1}m ".Style(Color.cIsDamage) + "for " + "{2}% ".Style(Color.cIsDamage) + "(+{3}% per stack) ".Style(Color.cStack) + "base damage. Lasts " + "{4}s ".Style(Color.cIsUtility) + "(+{5}s per stack)".Style(Color.cStack) + ". Recharges every " + "{6} ".Style(Color.cIsUtility) + "seconds.",
                    LowHealth.Value, Math.Round(Range.Value, roundVal),
                    Math.Round(Damage_Base.Value, roundVal), Math.Round(Damage_Stack.Value, roundVal),
                    Math.Round(Duration_Base.Value, roundVal), Math.Round(Duration_Stack.Value, roundVal),
                    Math.Round(Refresh.Value)
                );
            }
        }

        public static string StaticName = "Unstable Transmitter";

        public static ConfigEntry<int> Rework;

        public static ConfigEntry<bool> IsFloat;
        public static ConfigEntry<int> LowHealth;
        public static ConfigEntry<float> Refresh;
        public static ConfigEntry<float> Range;

        public static ConfigEntry<float> Damage_Base;
        public static ConfigEntry<float> Damage_Stack;
        public static ConfigEntry<float> Duration_Base;
        public static ConfigEntry<float> Duration_Stack;
    }
}
