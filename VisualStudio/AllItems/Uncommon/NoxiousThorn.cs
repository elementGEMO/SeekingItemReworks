using BepInEx.Configuration;
using System;
using static SeekingItemReworks.ColorCode;

namespace SeekerItems
{
    internal class NoxiousThorn : ItemTemplate
    {
        public NoxiousThorn(int descType)
        {
            ItemInternal = "TRIGGERENEMYDEBUFFS";

            string sequenceDoT = "bleed ".Style(FontColor.cIsDamage) + "enemies ";
            if (Inflict_Type.Value == 1) sequenceDoT = "apply " + "poison ".Style(FontColor.cIsDamage);
            if (Inflict_Type.Value == 2) sequenceDoT = "apply " + "blight ".Style(FontColor.cIsDamage);

            if (descType == 1)
            {
                ItemInfo = "Taking damage surrounds yourself in a ring of thorns.";
                ItemDesc = string.Format(
                    "Getting hit surrounds you in a " + "ring of thorns ".Style(FontColor.cIsDamage) + "that {0}" + "for " + "{1}% ".Style(FontColor.cIsDamage) + "(+{2}% per stack) ".Style(FontColor.cStack) + "damage. The ring " + "grows when taking damage".Style(FontColor.cIsDamage) + ", increasing its radius by " + "{3}m".Style(FontColor.cIsDamage) + ". Stacks up to " + "{4}m ".Style(FontColor.cIsDamage) + "(+{5}m per stack)".Style(FontColor.cStack) + ".",
                    sequenceDoT,
                    RoundVal(Damage_Base.Value), RoundVal(Damage_Stack.Value),
                    RoundVal(Range_Increment.Value), RoundVal(Base_Cap.Value),
                    RoundVal(Stack_Cap.Value)
                );
            }
        }

        public static string StaticName = "Noxious Thorn";

        public static ConfigEntry<int> Rework;

        public static ConfigEntry<int> Inflict_Type;
        public static ConfigEntry<float> Refresh;
        public static ConfigEntry<float> Damage_Frequency;

        public static ConfigEntry<float> Damage_Base;
        public static ConfigEntry<float> Damage_Stack;
        public static ConfigEntry<float> Initial_Range;
        public static ConfigEntry<float> Range_Increment;
        public static ConfigEntry<float> Base_Cap;
        public static ConfigEntry<float> Stack_Cap;
    }
}
