using MonoMod.Cil;
using Mono.Cecil.Cil;
using RoR2;
using System;
using BepInEx.Configuration;

using static SeekingItemReworks.ColorCode;

namespace SeekerItems
{
    internal class ChronicExpansion : ItemTemplate
    {
        public ChronicExpansion(int descType)
        {
            ItemInternal = "INCREASEDAMAGEONMULTIKILL";
            if (descType == 1)
            {
                ItemInfo = "Gain stacking damage after killing 5 enemies while in combat.";
                ItemDesc = string.Format(
                    "Killing 5 enemies ".Style(FontColor.cIsDamage) + "in combat buffs damage by " + "{0}% ".Style(FontColor.cIsDamage) + "(+{1}% per stack) ".Style(FontColor.cStack) + "temporarily each time, until leaving combat.",
                    RoundVal(Damage_Base.Value), RoundVal(Damage_Stack.Value)
                );
            }
        }

        public static string StaticName = "Chronic Expansion";

        public static ConfigEntry<int> Rework;
        public static ConfigEntry<bool> TallyKills;

        public static ConfigEntry<bool> IsHyperbolic;
        public static ConfigEntry<float> Damage_Base;
        public static ConfigEntry<float> Damage_Stack;
    }

    public static class ChronicExpansionBehavior
    {
        public static void Init()
        {
            if (ChronicExpansion.Rework.Value == 1)
            {
                IL.RoR2.CharacterBody.RecalculateStats += DamageFormula;
            }
            
            if (ChronicExpansion.TallyKills.Value)
            {
                IL.RoR2.CharacterBody.OnClientBuffsChanged += RemoveSync;
                On.RoR2.CharacterBody.UpdateMultiKill += AddCounterVFX;
            } 
        }
        private static void RemoveSync(ILContext il)
        {
            var cursor = new ILCursor(il);

            if (cursor.TryGotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<CharacterBody>(nameof(CharacterBody.increaseDamageOnMultiKillItemDisplayUpdater)),
                x => x.MatchLdloc(out _),
                x => x.MatchCallOrCallvirt<IncreaseDamageOnMultiKillItemDisplayUpdater>(nameof(IncreaseDamageOnMultiKillItemDisplayUpdater.UpdateKillCounterText))
            ))
            {
                var previousIndex = cursor.Index;

                if (cursor.TryGotoNext(x => x.MatchLdarg(0)))
                {
                    var skipLabel = cursor.MarkLabel();
                    cursor.Goto(previousIndex);
                    cursor.Emit(OpCodes.Br, skipLabel.Target);
                }
            }
            else
            {
                Log.Warning(ChronicExpansion.StaticName + " #1 - IL Fail #1");
            }
        }
        private static void DamageFormula(ILContext il)
        {
            var cursor = new ILCursor(il);
            var itemIndex = -1;

            if (cursor.TryGotoNext(
                x => x.MatchLdsfld(typeof(DLC2Content.Items), nameof(DLC2Content.Items.IncreaseDamageOnMultiKill)),
                x => x.MatchCallvirt<Inventory>(nameof(Inventory.GetItemCount)),
                x => x.MatchStloc(out itemIndex)
            )) { }
            else
            {
                Log.Warning(ChronicExpansion.StaticName + " #1 - IL Fail #2");
            }

            if (cursor.TryGotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdsfld(typeof(DLC2Content.Buffs), nameof(DLC2Content.Buffs.IncreaseDamageBuff)),
                x => x.MatchCallOrCallvirt<CharacterBody>(nameof(CharacterBody.GetBuffCount))
            ))
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldloc, itemIndex);

                cursor.EmitDelegate<Action<CharacterBody, int>>((body, itemCount) =>
                {
                    int buffCount = body.GetBuffCount(DLC2Content.Buffs.IncreaseDamageBuff);
                    float damageMod = buffCount * (ChronicExpansion.Damage_Base.Value + ChronicExpansion.Damage_Stack.Value * (itemCount - 1)) / 100f;
                    body.damage *= 1 + damageMod;

                    if (body.oldComboMeter < buffCount) body.oldComboMeter = buffCount;
                });

                var previousIndex = cursor.Index;

                if (cursor.TryGotoNext(
                    x => x.MatchLdarg(0),
                    x => x.MatchLdloc(out _),
                    x => x.MatchStfld<CharacterBody>(nameof(CharacterBody.oldComboMeter))
                ))
                {
                    var skipLabel = cursor.MarkLabel();
                    cursor.Goto(previousIndex);
                    cursor.Emit(OpCodes.Br, skipLabel.Target);
                }
            }
            else
            {
                Log.Warning(ChronicExpansion.StaticName + " #1 - IL Fail #3");
            }
        }
        private static void AddCounterVFX(On.RoR2.CharacterBody.orig_UpdateMultiKill orig, CharacterBody self, float deltaTime)
        {
            orig(self, deltaTime);
            int itemCount = self.inventory ? self.inventory.GetItemCount(DLC2Content.Items.IncreaseDamageOnMultiKill) : 0;
            if (self.increaseDamageOnMultiKillItemDisplayUpdater && itemCount > 0) self.increaseDamageOnMultiKillItemDisplayUpdater.UpdateKillCounterText(self.increasedDamageKillCount);
        }

    }
}
