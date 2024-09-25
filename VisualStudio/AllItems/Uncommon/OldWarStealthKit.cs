using BepInEx.Configuration;
using System;
using static SeekingItemReworks.ColorCode;

namespace SeekerItems
{
    internal class OldWarStealthKit : ItemTemplate
    {
        public OldWarStealthKit(int descType)
        {
            ItemInternal = "PHASING";
            if (descType == 1)
            {
                ItemInfo = "Turn invisible and cleanse debuffs at low health.";
                ItemDesc = "Falling below " + "25% health ".Style(FontColor.cIsHealth) + "causes you to become " + "invisible ".Style(FontColor.cIsUtility) + "for " + "5s".Style(FontColor.cIsUtility) + ", boost " + "movement speed ".Style(FontColor.cIsUtility) + "by " + "40% ".Style(FontColor.cIsUtility) + "and " + "cleanse 2 ".Style(FontColor.cIsUtility) + "debuffs ".Style(FontColor.cIsDamage) + "(+1 per stack)".Style(FontColor.cStack) + ". Recharges every " + "30 ".Style(FontColor.cIsUtility) + "(-50% per stack) ".Style(FontColor.cStack) + "seconds.";
            }
        }

        public static string StaticName = "Old War Stealthkit";

        public static ConfigEntry<int> Rework;
    }
}
