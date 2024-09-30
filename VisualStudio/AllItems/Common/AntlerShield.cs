using MonoMod.Cil;
using Mono.Cecil.Cil;
using RoR2;
using BepInEx.Configuration;
using R2API;

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
                    "Increase " + "armor ".Style(FontColor.cIsHealing) + "by " + "{0}% ".Style(FontColor.cIsHealing) + "(+{1}% per stack) ".Style(FontColor.cStack) + "of " + "current movement speed".Style(FontColor.cIsUtility) + ". Increase " + "movement speed ".Style(FontColor.cIsUtility) + "by " + "{2}% ".Style(FontColor.cIsUtility) + "(+{3}% per stack)".Style(FontColor.cStack) + ".",
                    RoundVal(Armor_Percent_Base.Value), RoundVal(Armor_Percent_Stack.Value),
                    RoundVal(Movement_Base.Value), RoundVal(Movement_Stack.Value)
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

    public static class AntlerShieldBehavior
    {
        public static void Init()
        {
            if (AntlerShield.Rework.Value == 1)
            {
                IL.RoR2.HealthComponent.TakeDamageProcess += RemoveEffect;
                RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            }
        }

        private static void RemoveEffect(ILContext il)
        {
            var cursor = new ILCursor(il);

            if (cursor.TryGotoNext(
                x => x.MatchLdloc(out _),
                x => x.MatchBrtrue(out _),
                x => x.MatchLdarg(0),
                x => x.MatchLdflda<HealthComponent>(nameof(HealthComponent.itemCounts)),
                x => x.MatchLdfld(typeof(HealthComponent.ItemCounts), nameof(HealthComponent.ItemCounts.antlerShield))
            ))
            {
                var previousIndex = cursor.Index;

                if (cursor.TryGotoNext(
                    x => x.MatchLdloc(out _),
                    x => x.MatchBrtrue(out _),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdflda<HealthComponent>(nameof(HealthComponent.itemCounts)),
                    x => x.MatchLdfld(typeof(HealthComponent.ItemCounts), nameof(HealthComponent.ItemCounts.noxiousThorn))
                ))
                {
                    var skipLabel = cursor.MarkLabel();
                    cursor.Goto(previousIndex);
                    cursor.MoveAfterLabels();
                    cursor.Emit(OpCodes.Br, skipLabel);
                }
                else
                {
                    Log.Warning(AntlerShield.StaticName + " #1 - IL Fail #2");
                }
            }
            else
            {
                Log.Warning(AntlerShield.StaticName + " #1 - IL Fail #1");
            }
        }
        private static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender && sender.master && sender.master.inventory)
            {
                int itemCount = sender.inventory.GetItemCount(DLC2Content.Items.NegateAttack);
                if (itemCount > 0)
                {
                    args.armorAdd += (AntlerShield.Armor_Percent_Base.Value + AntlerShield.Armor_Percent_Stack.Value * (itemCount - 1)) / 100f * sender.moveSpeed;
                    args.moveSpeedMultAdd += (AntlerShield.Movement_Base.Value + AntlerShield.Movement_Stack.Value * (itemCount - 1)) / 100f;
                }
            }
        }
    }
}
