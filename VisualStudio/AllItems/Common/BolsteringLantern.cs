using MonoMod.Cil;
using Mono.Cecil.Cil;
using RoR2;
using System;
using BepInEx.Configuration;
using UnityEngine.AddressableAssets;
using UnityEngine;
using R2API;
using UnityEngine.Networking;

using static SeekingItemReworks.ColorCode;

namespace SeekerItems
{
    internal class BolsteringLantern : ItemTemplate
    {
        public BolsteringLantern(int descType)
        {
            ItemInternal = "LOWERHEALTHHIGHERDAMAGE";

            if (descType == 1)
            {
                ItemInfo = string.Format(
                    "Ignite enemies at low health until health is restored {0}%.",
                    HighHealth.Value
                );
                ItemDesc = string.Format(
                    "Falling below " + "{0}% health".Style(FontColor.cIsHealth) + " grants a buff that " + "ignites ".Style(FontColor.cIsDamage) + "enemies for " + "{1}% ".Style(FontColor.cIsDamage) + "(+{2}% per stack) ".Style(FontColor.cStack) + "base damage on hit, until " + "health ".Style(FontColor.cIsHealing) + "is restored to " + "{3}%".Style(FontColor.cIsHealing) + ".",
                    LowHealth.Value, RoundVal(Damage_Base.Value),
                    RoundVal(Damage_Stack.Value), HighHealth.Value
                );
            }
            else if (descType == 2)
            {
                ItemInfo = "Chance on hit to ignite. Inherited by allies.";
                ItemDesc = string.Format(
                    "{0}% ".Style(FontColor.cIsDamage) + "(+{1}% per stack) ".Style(FontColor.cStack) + "chance on hit to " + "ignite ".Style(FontColor.cIsDamage) + "enemies for " + "{2}% ".Style(FontColor.cIsDamage) + "(+{3}% per stack) ".Style(FontColor.cStack) + "base damage. This item is " + "inherited by allies ".Style(FontColor.cIsUtility) + "and is boosted with " + "Ignition Tank".Style(FontColor.cIsUtility) + ".",
                    RoundVal(Chance_Base.Value), RoundVal(Chance_Stack.Value),
                    RoundVal(Damage_Base.Value), RoundVal(Damage_Stack.Value)
                );
            }
        }

        public static string StaticName = "Bolstering Lantern";

        public static ConfigEntry<int> Rework;

        public static ConfigEntry<int> LowHealth;
        public static ConfigEntry<int> HighHealth;

        public static ConfigEntry<float> Damage_Base;
        public static ConfigEntry<float> Damage_Stack;
        public static ConfigEntry<float> Chance_Base;
        public static ConfigEntry<float> Chance_Stack;
    }

    public static class BolsteringLanternBehavior
    {
        public static void Init()
        {
            if (BolsteringLantern.Rework.Value != 0)
            {
                IL.RoR2.CharacterBody.RecalculateStats += RemoveEffect;
                IL.RoR2.HealthComponent.TakeDamageProcess += RemoveDamage;
            }
            if (BolsteringLantern.Rework.Value == 1)
            {
                On.RoR2.CharacterBody.UpdateLowerHealthHigherDamage += HealthTrigger;
                On.RoR2.GlobalEventManager.ProcessHitEnemy += InflictFire;
            }
            else if (BolsteringLantern.Rework.Value == 2)
            {
                new LanternCountBuff();
                IL.RoR2.CharacterBody.UpdateLowerHealthHigherDamage += RemoveBuff;
                On.RoR2.CharacterBody.FixedUpdate += MinionApply;
                On.RoR2.GlobalEventManager.ProcessHitEnemy += FireProc;
            }
        }

        private static void RemoveEffect(ILContext il)
        {
            var cursor = new ILCursor(il);

            if (cursor.TryGotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdsfld(typeof(DLC2Content.Buffs), nameof(DLC2Content.Buffs.LowerHealthHigherDamageBuff)),
                x => x.MatchCallOrCallvirt<CharacterBody>(nameof(CharacterBody.HasBuff))
            ))
            {
                var previousIndex = cursor.Index;

                if (cursor.TryGotoNext(
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<CharacterBody>(nameof(CharacterBody.tamperedHeartActive)),
                    x => x.MatchPop()
                ))
                {
                    var skipLabel = cursor.MarkLabel();
                    cursor.Goto(previousIndex);
                    cursor.Emit(OpCodes.Br, skipLabel);
                }
                else
                {
                    Log.Warning(BolsteringLantern.StaticName + " #0 - IL Fail #2");
                }
            }
            else
            {
                Log.Warning(BolsteringLantern.StaticName + " #0 - IL Fail #1");
            }
        }
        private static void RemoveDamage(ILContext il)
        {
            var cursor = new ILCursor(il);

            if (cursor.TryGotoNext(
                x => x.MatchLdloc(0),
                x => x.MatchCallOrCallvirt<CharacterMaster>(nameof(CharacterMaster.GetBody)),
                x => x.MatchLdsfld(typeof(DLC2Content.Buffs), nameof(DLC2Content.Buffs.LowerHealthHigherDamageBuff)),
                x => x.MatchCallOrCallvirt<CharacterBody>(nameof(CharacterBody.HasBuff))
            ))
            {
                var previousIndex = cursor.Index;

                if (cursor.TryGotoNext(
                    x => x.MatchLdarg(1),
                    x => x.MatchLdfld<DamageInfo>(nameof(DamageInfo.procCoefficient)),
                    x => x.MatchLdcR4(out _)
                ))
                {
                    var skipLabel = cursor.MarkLabel();
                    cursor.Goto(previousIndex);
                    cursor.MoveAfterLabels();
                    cursor.Emit(OpCodes.Br, skipLabel);
                }
                else
                {
                    Log.Warning(BolsteringLantern.StaticName + " #0 - IL Fail #4");
                }
            }
            else
            {
                Log.Warning(BolsteringLantern.StaticName + " #0 - IL Fail #3");
            }
        }
        private static void RemoveBuff(ILContext il)
        {
            var cursor = new ILCursor(il);
            var itemIndex = -1;

            if (cursor.TryGotoNext(x => x.MatchStloc(out itemIndex)))
            {
                cursor.EmitDelegate<Func<int, int>>(self => 0);
            }
        }
        private static void HealthTrigger(On.RoR2.CharacterBody.orig_UpdateLowerHealthHigherDamage orig, CharacterBody self)
        {
            if (!NetworkServer.active) return;

            int itemCount = self.inventory ? self.inventory.GetItemCount(DLC2Content.Items.LowerHealthHigherDamage) : 0;
            if (itemCount > 0)
            {
                float percentHealth = self.healthComponent.GetNormalizedHealth();
                if (percentHealth <= (BolsteringLantern.LowHealth.Value / 100f) && !self.HasBuff(DLC2Content.Buffs.LowerHealthHigherDamageBuff))
                {
                    self.AddBuff(DLC2Content.Buffs.LowerHealthHigherDamageBuff);
                    Util.PlaySound("Play_item_proc_lowerHealthHigherDamage_proc", self.gameObject);
                    return;
                }
                else if (percentHealth >= (BolsteringLantern.HighHealth.Value / 100f) && self.HasBuff(DLC2Content.Buffs.LowerHealthHigherDamageBuff))
                {
                    self.RemoveBuff(DLC2Content.Buffs.LowerHealthHigherDamageBuff);
                    return;
                }
            }
            else if (itemCount <= 0 && self.HasBuff(DLC2Content.Buffs.LowerHealthHigherDamageBuff))
            {
                self.RemoveBuff(DLC2Content.Buffs.LowerHealthHigherDamageBuff);
            }
        }
        private static void InflictFire(On.RoR2.GlobalEventManager.orig_ProcessHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);

            if (damageInfo.attacker && damageInfo.damage > 0)
            {
                CharacterBody characterBody = damageInfo.attacker.GetComponent<CharacterBody>();
                if (characterBody && characterBody.HasBuff(DLC2Content.Buffs.LowerHealthHigherDamageBuff))
                {
                    int itemCount = characterBody.inventory.GetItemCount(DLC2Content.Items.LowerHealthHigherDamage);
                    InflictDotInfo burnDot = new()
                    {
                        attackerObject = damageInfo.attacker,
                        victimObject = victim,
                        totalDamage = new float?(damageInfo.damage * (BolsteringLantern.Damage_Base.Value + BolsteringLantern.Damage_Stack.Value * (itemCount - 1)) / 100f),
                        damageMultiplier = 1.0f,
                        duration = 0.1f,
                        dotIndex = DotController.DotIndex.Burn,
                    };
                    StrengthenBurnUtils.CheckDotForUpgrade(characterBody.inventory, ref burnDot);
                    DotController.InflictDot(ref burnDot);
                }
            }
        }
        private static void MinionApply(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
        {
            orig(self);

            int lanternCount = self.inventory ? self.inventory.GetItemCount(DLC2Content.Items.LowerHealthHigherDamage) : 0;
            if (lanternCount > 0)
            {
                MinionOwnership.MinionGroup allyList = MinionOwnership.MinionGroup.FindGroup(self.masterObjectId);
                self.SetBuffCount(LanternCountBuff.LanternCounter.buffIndex, lanternCount);

                if (allyList != null)
                {
                    foreach (MinionOwnership ally in allyList.members)
                    {
                        if (ally == null) continue;
                        CharacterMaster allyMaster = ally.GetComponent<CharacterMaster>();
                        if (allyMaster && allyMaster.inventory)
                        {
                            if (allyMaster.GetBody()) allyMaster.GetBody().SetBuffCount(LanternCountBuff.LanternCounter.buffIndex, lanternCount);
                        }
                    }
                }
            }
        }
        private static void FireProc(On.RoR2.GlobalEventManager.orig_ProcessHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);

            if (damageInfo.attacker && damageInfo.procCoefficient > 0f && damageInfo.damage > 0)
            {
                CharacterBody allyBody = damageInfo.attacker.GetComponent<CharacterBody>();
                int countBuffs = allyBody ? allyBody.GetBuffCount(LanternCountBuff.LanternCounter) : 0;

                if (allyBody && countBuffs > 0 && Util.CheckRoll(BolsteringLantern.Chance_Base.Value + BolsteringLantern.Chance_Stack.Value * (countBuffs - 1), 0f, null))
                {
                    Inventory targetInventory = (allyBody.master.minionOwnership.group != null && allyBody.master.minionOwnership.group.resolvedOwnerMaster) ? allyBody.master.minionOwnership.group.resolvedOwnerMaster.inventory : allyBody.inventory;
                    InflictDotInfo burnDot = new()
                    {
                        attackerObject = damageInfo.attacker,
                        victimObject = victim,
                        totalDamage = new float?(damageInfo.damage * (BolsteringLantern.Damage_Base.Value + BolsteringLantern.Damage_Stack.Value * (countBuffs - 1)) / 100f),
                        damageMultiplier = 1.0f,
                        duration = 0.1f,
                        dotIndex = DotController.DotIndex.Burn,
                    };
                    StrengthenBurnUtils.CheckDotForUpgrade(targetInventory, ref burnDot);
                    DotController.InflictDot(ref burnDot);
                }
            }
        }
    }

    public class LanternCountBuff
    {
        public static BuffDef LanternCounter;
        private static readonly Sprite spriteIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/DLC2/Items/LowerHealthHigherDamage/texBuffLowerHealthHigherDamageIcon.png").WaitForCompletion();

        public LanternCountBuff()
        {
            LanternCounter = ScriptableObject.CreateInstance<BuffDef>();
            LanternCounter.name = "AllyLanternCount";
            LanternCounter.canStack = true;
            LanternCounter.isCooldown = false;
            LanternCounter.isDebuff = false;
            LanternCounter.ignoreGrowthNectar = false;
            LanternCounter.iconSprite = spriteIcon;
            ContentAddition.AddBuffDef(LanternCounter);
        }
    }
}
