using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using RoR2;
using UnityEngine.Networking;

using static SeekingItemReworks.ColorCode;

namespace SeekerItems
{
    internal class GrowthNectar : ItemTemplate
    {
        public GrowthNectar(int descType)
        {
            ItemInternal = "BOOSTALLSTATS";

            if (descType == 1)
            {
                ItemInfo = "Not using an equipment increases ALL of your stats. Scales with equipment charges.";
                ItemDesc = string.Format(
                    "When your Equipment is off " + "cooldown".Style(FontColor.cIsUtility) + ", increase " + "ALL stats ".Style(FontColor.cIsUtility) + "by " + "{0}% ".Style(FontColor.cIsUtility) + "(+{1}% per stack)".Style(FontColor.cStack) + ", and by " + "{2}% ".Style(FontColor.cIsUtility) + "per " + "Equipment charge".Style(FontColor.cIsUtility) + ", up to a maximum of " + "{3}% ".Style(FontColor.cIsUtility) + "(+{4}% per stack)".Style(FontColor.cStack) + ".",
                    RoundVal(Stat_Base.Value), RoundVal(Stat_Stack.Value),
                    RoundVal(Charge_Stat_Increase.Value),
                    RoundVal(Charge_Cap_Base.Value), RoundVal(Charge_Cap_Stack.Value)
                );
            }
        }

        public static string StaticName = "Growth Nectar";

        public static ConfigEntry<int> Rework;

        public static ConfigEntry<float> Stat_Base;
        public static ConfigEntry<float> Stat_Stack;
        public static ConfigEntry<float> Charge_Stat_Increase;
        public static ConfigEntry<float> Charge_Cap_Base;
        public static ConfigEntry<float> Charge_Cap_Stack;
    }

    public static class GrowthNectarBehavior
    {
        public static void Init()
        {
            if (GrowthNectar.Rework.Value == 1)
            {
                On.RoR2.EquipmentSlot.MyFixedUpdate += NonEquipUse;
                On.RoR2.CharacterBody.UpdateBoostAllStatsTimer += ReplaceEffect;
                IL.RoR2.CharacterBody.RecalculateStats += ReplaceTrigger;
                IL.RoR2.CharacterBody.RecalculateStats += ReplaceStats;
            }
        }

        private static void NonEquipUse(On.RoR2.EquipmentSlot.orig_MyFixedUpdate orig, EquipmentSlot self, float deltaTime)
        {
            orig(self, deltaTime);

            if (NetworkServer.active && self.characterBody.inventory)
            {
                int nectarCount = self.characterBody.inventory.GetItemCount(DLC2Content.Items.BoostAllStats);
                bool nonEquip = self.cooldownTimer == float.PositiveInfinity || self.cooldownTimer == float.NegativeInfinity;
                bool hasBuff = self.characterBody.HasBuff(DLC2Content.Buffs.BoostAllStatsBuff);

                if (nectarCount > 0 && !hasBuff && (self.cooldownTimer <= 0 || nonEquip))
                {
                    self.characterBody.AddBuff(DLC2Content.Buffs.BoostAllStatsBuff);
                }
                else if ((nectarCount <= 0 || !nonEquip) && hasBuff)
                {
                    self.characterBody.RemoveBuff(DLC2Content.Buffs.BoostAllStatsBuff);
                }
            }
        }
        private static void ReplaceEffect(On.RoR2.CharacterBody.orig_UpdateBoostAllStatsTimer orig, CharacterBody self, float timer) { return; }
        private static void ReplaceTrigger(ILContext il)
        {
            var cursor = new ILCursor(il);

            if (cursor.TryGotoNext(
                x => x.MatchBle(out _),
                x => x.MatchLdarg(0),
                x => x.MatchLdsfld(typeof(DLC2Content.Buffs), nameof(DLC2Content.Buffs.BoostAllStatsBuff))
            ))
            {
                cursor.EmitDelegate<Func<int, int>>(stack => int.MaxValue);
            }
            else
            {
                Log.Warning(GrowthNectar.StaticName + " #1 - IL Fail #1");
            }
        }
        private static void ReplaceStats(ILContext il)
        {
            var cursor = new ILCursor(il);

            if (cursor.TryGotoNext(
                x => x.MatchLdloc(out _),
                x => x.MatchConvR4(),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<CharacterBody>(nameof(CharacterBody.boostAllStatsMultiplier))
            ))
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Action<CharacterBody>>(self =>
                {
                    EquipmentSlot equip = self.equipmentSlot;

                    int itemCount = self.inventory ? self.inventory.GetItemCount(DLC2Content.Items.BoostAllStats) : 0;
                    float itemMultiplier = (float)GrowthNectar.Stat_Base.Value + GrowthNectar.Stat_Stack.Value * (itemCount - 1);
                    float chargeMultiplier = (float)Math.Min(GrowthNectar.Charge_Stat_Increase.Value * (equip.maxStock - 1), GrowthNectar.Charge_Cap_Base.Value + GrowthNectar.Charge_Cap_Stack.Value * (itemCount - 1));
                    float multiplier = (itemMultiplier + chargeMultiplier) / 100f;

                    self.armor += self.armor * multiplier;
                    self.maxHealth += self.maxHealth * multiplier;
                    self.regen += self.regen * multiplier;
                    self.moveSpeed += self.moveSpeed * multiplier;
                    self.attackSpeed += self.attackSpeed * multiplier;
                    self.damage += self.damage * multiplier;
                    self.crit += self.crit * multiplier;
                });

                var previousIndex = cursor.Index;

                if (cursor.TryGotoNext(
                    x => x.MatchLdarg(0),
                    x => x.MatchLdarg(0),
                    x => x.MatchCallOrCallvirt<CharacterBody>("get_maxHealth"),
                    x => x.MatchCallOrCallvirt<CharacterBody>("set_maxBonusHealth")
                ))
                {
                    var skipLabel = cursor.MarkLabel();
                    cursor.Goto(previousIndex);
                    cursor.Emit(OpCodes.Br, skipLabel.Target);
                }
                else
                {
                    Log.Warning(GrowthNectar.StaticName + " #1 - IL Fail #3");
                }
            }
            else
            {
                Log.Warning(GrowthNectar.StaticName + " #1 - IL Fail #2");
            }
        }
    }
}
