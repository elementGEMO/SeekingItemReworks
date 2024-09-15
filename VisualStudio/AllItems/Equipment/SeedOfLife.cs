using BepInEx.Configuration;
using System;
using static SeekingItemReworks.ColorCode;

namespace SeekerItems
{
    internal class SeedOfLife : ItemTemplate
    {
        public SeedOfLife(int descType)
        {
            ItemInternal = "HEALANDREVIVE";

            if (descType == 1)
            {
                ItemInfo = "Revives user or allies. Consume on use.";
                ItemDesc = "Returns ".Style(Color.cIsHealing) + "the user " + "to life ".Style(Color.cIsHealing) + "upon death ".Style(Color.cIsUtility) + "or dead allies on use. Equipment is " + "consumed ".Style(Color.cIsUtility) + "on use.";
            }
        }

        public static string StaticName = "Seed of Life";

        public static ConfigEntry<int> Rework;
    }
}
