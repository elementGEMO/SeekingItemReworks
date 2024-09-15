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
                ItemDesc = "Falling below " + "25% health ".Style(Color.cIsHealth) + "causes you to become " + "invisible ".Style(Color.cIsUtility) + "for " + "5s".Style(Color.cIsUtility) + ", boost " + "movement speed ".Style(Color.cIsUtility) + "by " + "40% ".Style(Color.cIsUtility) + "and " + "cleanse 2 ".Style(Color.cIsUtility) + "debuffs ".Style(Color.cIsDamage) + "(+1 per stack)".Style(Color.cStack) + ". Recharges every " + "30 ".Style(Color.cIsUtility) + "(-50% per stack) ".Style(Color.cStack) + "seconds.";
            }
        }

        public static string StaticName = "Old War Stealthkit";

        public static ConfigEntry<int> Rework;
    }
}
