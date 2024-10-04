using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;

using static SeekingItemReworks.ColorCode;

namespace SeekerItems
{
    internal class SeedOfLifeConsumed : ItemTemplate
    {
        public SeedOfLifeConsumed(int descType)
        {
            ItemInternal = "HEALANDREVIVECONSUMED";

            if (descType == 1)
            {
                ItemInfo = "Spawn healing orbs temporarily.";
                ItemDesc = "Place down a sprout that drops " + "10 healing orbs ".Style(FontColor.cIsHealing) + "overtime.";
            }
        }

        public static string StaticName = "Seed of Life Consumed";

        public static ConfigEntry<int> Rework;
    }
}
