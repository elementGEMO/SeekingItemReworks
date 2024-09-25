using BepInEx.Configuration;
using System;
using static SeekingItemReworks.ColorCode;

namespace SeekerItems
{
    internal class KnockbackFin : ItemTemplate
    {
        public KnockbackFin(int descType)
        {
            ItemInternal = "KNOCKBACKHITENEMIES";
            if (descType == 1)
            {
                ItemInfo = "Chance on hit to knock enemies into the air.\nDeal bonus damage to airborne enemies.";
                ItemDesc = string.Format(
                    "{0}% ".Style(FontColor.cIsUtility) + "(+{1}% per stack) ".Style(FontColor.cStack) + "chance on hit to " + "knock enemies into the air".Style(FontColor.cIsUtility) + ". Deal " + "+{2}% ".Style(FontColor.cIsDamage) + "(+{3}% per stack) ".Style(FontColor.cStack) + "damage to " + "airborne ".Style(FontColor.cIsUtility) + "enemies.",
                    Math.Round(Chance_Base.Value, roundVal), Math.Round(Chance_Stack.Value, roundVal),
                    Math.Round(Damage_Base.Value, roundVal), Math.Round(Damage_Stack.Value, roundVal)
                );
            }
            else if (descType == -1)
            {
                ItemInfo = "Chance on hit to knock up enemies.\nDeal bonus damage to airborne enemies.";
                ItemDesc = string.Format(
                    "{0}% ".Style(FontColor.cIsUtility) + "(+{1}% per stack) ".Style(FontColor.cStack) + "chance on hit to " + "knock up enemies".Style(FontColor.cIsUtility) + ". Deal " + "+{2}% ".Style(FontColor.cIsDamage) + "(+{3}% per stack) ".Style(FontColor.cStack) + "damage to " + "airborne ".Style(FontColor.cIsUtility) + "enemies.",
                    Math.Round(Chance_Base.Value, roundVal), Math.Round(Chance_Stack.Value, roundVal),
                    Math.Round(Damage_Base.Value, roundVal), Math.Round(Damage_Stack.Value, roundVal)
                );
            }
        }

        public static string StaticName = "Knockback Fin";

        public static ConfigEntry<int> Rework;
        public static ConfigEntry<bool> IsHyperbolic;

        public static ConfigEntry<float> Chance_Base;
        public static ConfigEntry<float> Chance_Stack;
        public static ConfigEntry<float> Damage_Base;
        public static ConfigEntry<float> Damage_Stack;
    }
    public static class KnockbackFinBehavior
    {
        public static void Init()
        {
            if (KnockbackFin.Rework.Value == 1)
            {

            }
        }
    }
}
