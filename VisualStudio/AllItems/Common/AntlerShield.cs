﻿using BepInEx.Configuration;
using System;
using static SeekingItemReworks.ColorCode;

namespace SeekerItems
{
    internal class AntlerShield : ItemTemplate
    {
        public AntlerShield(int descType)
        {
            ItemInternal = "NEGATEATTACK";

            if (descType == 1)
            {
                ItemInfo = "Gain armor from movement speed, and increase movement speed.";
                ItemDesc = string.Format(
                    "Increase " + "armor ".Style(Color.cIsHealing) + "by " + "{0}% ".Style(Color.cIsHealing) + "(+{1}% per stack) ".Style(Color.cStack) + "of " + "current movement speed".Style(Color.cIsUtility) + ". Increase " + "movement speed ".Style(Color.cIsUtility) + "by " + "{2}% ".Style(Color.cIsUtility) + "(+{3}% per stack)".Style(Color.cStack) + ".",
                    Math.Round(Armor_Percent_Base.Value, roundVal), Math.Round(Armor_Percent_Stack.Value, roundVal),
                    Math.Round(Movement_Base.Value, roundVal), Math.Round(Movement_Stack.Value, roundVal)
                );
            }
        }

        public static string StaticName = "Antler Shield";

        public static ConfigEntry<int> Rework;

        public static ConfigEntry<float> Armor_Percent_Base;
        public static ConfigEntry<float> Armor_Percent_Stack;
        public static ConfigEntry<float> Movement_Base;
        public static ConfigEntry<float> Movement_Stack;
    }
}
