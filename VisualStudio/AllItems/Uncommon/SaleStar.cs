using MonoMod.Cil;
using Mono.Cecil.Cil;
using BepInEx.Configuration;
using System;
using RoR2;

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
                    RoundVal(Consume_Base.Value), RoundVal(Consume_Stack.Value)
                );
            }
        }

        public static string StaticName = "Sale Star";

        public static ConfigEntry<int> Rework;

        public static ConfigEntry<bool> IsHyperbolic;
        public static ConfigEntry<float> Consume_Base;
        public static ConfigEntry<float> Consume_Stack;
    }

    public static class SaleStarBehavior
    {
        public static void Init()
        {
            if (SaleStar.Rework.Value == 1)
            {
                IL.RoR2.PurchaseInteraction.OnInteractionBegin += ReplaceEffect;
                On.RoR2.PurchaseInteraction.OnInteractionBegin += ChanceConsume;
            }
        }

        private static void ReplaceEffect(ILContext il)
        {
            var cursor = new ILCursor(il);

            if (cursor.TryGotoNext(
                x => x.MatchBle(out _),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<PurchaseInteraction>(nameof(PurchaseInteraction.saleStarCompatible))
            ))
            {
                cursor.EmitDelegate<Func<int, int>>(stack => int.MaxValue);
            }
            else
            {
                Log.Warning(SaleStar.StaticName + " #1 - IL Fail #1");
            }
        }

        private static void ChanceConsume(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            orig(self, activator);

            CharacterBody body = activator.GetComponent<CharacterBody>();
            int itemCount = body.inventory ? body.inventory.GetItemCount(DLC2Content.Items.LowerPricedChests) : 0;

            if (itemCount > 0 && self.saleStarCompatible)
            {
                if (self.GetComponent<ChestBehavior>()) self.GetComponent<ChestBehavior>().dropCount++;
                else if (self.GetComponent<RouletteChestController>()) self.GetComponent<RouletteChestController>().dropCount++;

                float percentConvert = SaleStar.IsHyperbolic.Value ? Util.ConvertAmplificationPercentageIntoReductionPercentage(SaleStar.Consume_Stack.Value * (itemCount - 1)) : SaleStar.Consume_Stack.Value * (itemCount - 1);

                if (Util.CheckRoll(SaleStar.Consume_Base.Value - percentConvert, body.master))
                {
                    body.inventory.RemoveItem(DLC2Content.Items.LowerPricedChests, itemCount);
                    body.inventory.GiveItem(DLC2Content.Items.LowerPricedChestsConsumed, itemCount);
                    CharacterMasterNotificationQueue.SendTransformNotification(body.master, DLC2Content.Items.LowerPricedChests.itemIndex, DLC2Content.Items.LowerPricedChestsConsumed.itemIndex, CharacterMasterNotificationQueue.TransformationType.SaleStarRegen);
                }

                Util.PlaySound("Play_item_proc_lowerPricedChest", self.gameObject);
            }
        }
    }
}