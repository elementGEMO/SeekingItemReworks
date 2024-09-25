using BepInEx.Configuration;
using System;
using static SeekingItemReworks.ColorCode;

namespace SeekerItems
{
    internal class SaleStar : ItemTemplate
    {
        public SaleStar(int descType)
        {
            ItemInternal = "LOWERPRICEDCHESTS";

            Consume_Base.Value = Math.Max(Math.Min(Consume_Base.Value, 100), 0);
            Consume_Stack.Value = Math.Max(Math.Min(Consume_Stack.Value, Consume_Base.Value), 0);

            if (descType == 1)
            {
                ItemInfo = "Gain an extra item from chests. Chance to be consumed on use once per stage.";
                ItemDesc = string.Format(
                    "Gain an " + "extra item ".Style(FontColor.cIsUtility) + "when purchasing a " + "chest ".Style(FontColor.cIsUtility) + "with a " + "{0}% ".Style(FontColor.cIsUtility) + "(-{1}% per stack) ".Style(FontColor.cStack) + "chance to be " + "consumed ".Style(FontColor.cIsUtility) + "on use. At the start of each stage, it regenerates.",
                    Math.Round(Consume_Base.Value, roundVal), Math.Round(Consume_Stack.Value, roundVal)
                );
            }
        }

        public static string StaticName = "Sale Star";

        public static ConfigEntry<int> Rework;
        public static ConfigEntry<bool> IsHyperbolic;
        public static ConfigEntry<float> Consume_Base;
        public static ConfigEntry<float> Consume_Stack;
    }
}
