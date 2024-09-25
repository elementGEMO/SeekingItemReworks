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
                ItemDesc = "Returns ".Style(FontColor.cIsHealing) + "the user " + "to life ".Style(FontColor.cIsHealing) + "upon death ".Style(FontColor.cIsUtility) + "or dead allies on use. Equipment is " + "consumed ".Style(FontColor.cIsUtility) + "on use.";
            }
        }

        public static string StaticName = "Seed of Life";

        public static ConfigEntry<int> Rework;
    }
}
