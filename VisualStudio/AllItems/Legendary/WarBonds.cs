using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using RoR2;

using static SeekingItemReworks.ColorCode;
using UnityEngine.Networking;

namespace SeekerItems
{
    internal class WarBonds : ItemTemplate
    {
        public WarBonds(int descType)
        {
            ItemInternal = "GOLDONSTAGESTART";

            if (descType == 1)
            {
                ItemInfo = string.Format(
                    "Gain free purchases at the start of each stage.\nGain {0}% experience on cash spent on all purchases.",
                    RoundVal(Experience_Percent_Base.Value)
                );
                ItemDesc = string.Format(
                    "Gain " + "{0} ".Style(FontColor.cIsUtility) + "(+{1} per stack) ".Style(FontColor.cStack) + "free purchases ".Style(FontColor.cIsUtility) + "at the " + "start of each stage".Style(FontColor.cIsUtility) + ". When making a gold purchase, get " + "{2}% ".Style(FontColor.cIsUtility) + "(+{3}% per stack) ".Style(FontColor.cStack) + "of spent gold as " + "experience".Style(FontColor.cIsUtility) + ".",
                    Purchase_Base.Value, Purchase_Stack.Value, RoundVal(Experience_Percent_Base.Value), RoundVal(Experience_Percent_Stack.Value)
                );
            }
        }

        public static string StaticName = "War Bonds";

        public static ConfigEntry<int> Rework;

        public static ConfigEntry<bool> Free_Purchase_VFX;

        public static ConfigEntry<int> Purchase_Base;
        public static ConfigEntry<int> Purchase_Stack;
        public static ConfigEntry<float> Experience_Percent_Base;
        public static ConfigEntry<float> Experience_Percent_Stack;
    }
    public static class WarBondsBehavior
    {
        public static void Init()
        {
            if (WarBonds.Rework.Value == 1)
            {
                IL.RoR2.PurchaseInteraction.OnInteractionBegin += InteractBegin;
                IL.RoR2.CharacterBody.Start += ReplaceEffect;
                On.RoR2.CharacterBody.Start += FreeUnlocks;
            }
        }

        private static void InteractBegin(ILContext il)
        {
            var cursor = new ILCursor(il);
            var costIndex = -1;

            if (cursor.TryGotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<PurchaseInteraction>(nameof(PurchaseInteraction.cost)),
                x => x.MatchStloc(out costIndex)
            )) { } else Log.Warning(WarBonds.StaticName + " #1 - IL Fail #1");

            if (cursor.TryGotoNext( x => x.MatchLdarg(0)))
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_1);

                cursor.EmitDelegate<Action<PurchaseInteraction, Interactor>>((interact, activator) =>
                {
                    if (activator && interact && interact.costType == CostTypeIndex.Money)
                    {
                        CharacterBody body = activator.GetComponent<CharacterBody>();

                        int warBondCount = body.inventory ? body.inventory.GetItemCount(DLC2Content.Items.GoldOnStageStart) : 0;
                        if (warBondCount > 0) ExperienceManager.instance.AwardExperience(interact.transform.position, body, (ulong)(interact.cost * (WarBonds.Experience_Percent_Base.Value + WarBonds.Experience_Percent_Stack.Value * (warBondCount - 1)) / 100f));

                        if (body.HasBuff(DLC2Content.Buffs.FreeUnlocks) && WarBonds.Free_Purchase_VFX.Value)
                        {
                            Util.PlaySound("Play_item_proc_goldOnStageStart", body.gameObject);
                            EffectManager.SpawnEffect(HealthComponent.AssetReferences.gainCoinsImpactEffectPrefab, new EffectData
                            {
                                origin = body.transform.position,
                                scale = 2f,
                            }, false);
                        }
                    }
                });
            } else Log.Warning(WarBonds.StaticName + " #1 - IL Fail #2");
        }
        private static void ReplaceEffect(ILContext il)
        {
            var cursor = new ILCursor(il);

            if (cursor.TryGotoNext(
                x => x.MatchBle(out _),
                x => x.MatchLdarg(0),
                x => x.MatchLdsfld(typeof(DLC2Content.Items), nameof(DLC2Content.Items.GoldOnStageStart))
            ))
            {
                cursor.EmitDelegate<Func<int, int>>(stack => int.MaxValue);
            }
            else Log.Warning(WarBonds.StaticName + " #1 - IL Fail #3");
        }
        private static void FreeUnlocks(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            orig(self);

            if (NetworkServer.active && self.master && self.master.inventory)
            {
                int itemCount = self.master.inventory.GetItemCount(DLC2Content.Items.GoldOnStageStart);
                if (itemCount > 0) self.SetBuffCount(DLC2Content.Buffs.FreeUnlocks.buffIndex, WarBonds.Purchase_Base.Value + WarBonds.Purchase_Stack.Value * (itemCount - 1));
            }
        }
    }
}
