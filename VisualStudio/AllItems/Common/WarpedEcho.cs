using BepInEx.Configuration;
using System;
using static SeekingItemReworks.ColorCode;

namespace SeekerItems
{
    internal class WarpedEcho : ItemTemplate
    {
        public WarpedEcho(int descType)
        {
            ItemInternal = "DELAYEDDAMAGE";
            if (descType == 1)
            {
                ItemInfo = "Half of incoming damage is delayed.";
                ItemDesc = string.Format(
                    "Incoming damage ".Style(Color.cIsDamage) + "is split " + "50%".Style(Color.cIsDamage) + ", with the other " + "50% ".Style(Color.cIsDamage) + "delayed for " + "{0}s ".Style(Color.cIsDamage) + "(+{1} instances per stack)".Style(Color.cStack) + ". Recharges one instance after delayed damage.",
                    Math.Round(Delay_Base.Value, roundVal), Instance_Stack.Value
                );
            }
        }

        public static string StaticName = "Warped Echo";

        public static ConfigEntry<int> Rework;
        public static ConfigEntry<float> Delay_Base;
        public static ConfigEntry<int> Instance_Stack;
    }
}
