using BepInEx;
using R2API;
using RoR2;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

using SeekingItemReworks;
using HarmonyLib;
using System.Linq;

namespace SeekerItems
{
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "noodlegemo";
        public const string PluginName = "SeekingItemReworks";
        public const string PluginVersion = "1.0.0";

        public void Awake()
        {
            Log.Init(Logger);

            MainConfig.SetUpConfigs(this);

            Items.Init();

            SetUpCommonItems();
        }

        private void SetUpCommonItems()
        {
            // Warped Echo Rework & Bug Fix
            if (MainConfig.WarpedEchoReworkEnabled.Value)
            {
                // Sets Delayed Damage stacking false.
                BuffDef delayDmgBuff = Addressables.LoadAsset<BuffDef>("RoR2/DLC2/Items/DelayedDamage/bdDelayedDamageBuff.asset").WaitForCompletion();
                delayDmgBuff.canStack = false;

                // Makes Delayed Damage delay duration stack instead.
                IL.RoR2.CharacterBody.SecondHalfOfDelayedDamage += il =>
                {
                    var cursor = new ILCursor(il);

                    if (cursor.TryGotoNext(
                        x => x.MatchLdarg(0),
                        x => x.MatchLdsfld(typeof(DLC2Content.Buffs), nameof(DLC2Content.Buffs.DelayedDamageBuff)),
                        x => x.MatchCall<CharacterBody>(nameof(CharacterBody.RemoveBuff))
                    ))
                    {
                        cursor.Emit(OpCodes.Ldarg_0);
                        cursor.EmitDelegate<Action<CharacterBody>>(body => body.SetBuffCount(DLC2Content.Buffs.DelayedDamageBuff.buffIndex, 0));

                        if (cursor.TryGotoNext(
                            x => x.MatchLdloc(0),
                            x => x.MatchLdarg(1),
                            x => x.MatchStfld(typeof(CharacterBody.DelayedDamageInfo), nameof(CharacterBody.DelayedDamageInfo.halfDamage))
                        ))
                        {
                            cursor.Emit(OpCodes.Ldarg_0);
                            cursor.Emit(OpCodes.Ldloc_0);
                            cursor.EmitDelegate<Action<CharacterBody, CharacterBody.DelayedDamageInfo>>((body, damageInfo) =>
                            {
                                int itemNum = body.inventory ? body.inventory.GetItemCount(DLC2Content.Items.DelayedDamage) : 0;
                                damageInfo.timeUntilDamage = MainConfig.CWE_Delay.Value + (itemNum - 1) * MainConfig.CWE_Stack.Value;
                            });
                        }
                    }
                    else
                    {
                        Logger.LogWarning("Warped Echo - Failure to hook trigger");
                    }
                };

                // Makes Delayed Damage refresh when done.
                IL.RoR2.CharacterBody.UpdateDelayedDamage += il =>
                {
                    var cursor = new ILCursor(il);

                    if (cursor.TryGotoNext(
                        x => x.MatchLdarg(0),
                        x => x.MatchLdarg(0),
                        x => x.MatchLdfld<CharacterBody>(nameof(CharacterBody.delayedDamageRefreshTime)),
                        x => x.MatchLdarg(1),
                        x => x.MatchSub(),
                        x => x.MatchStfld<CharacterBody>(nameof(CharacterBody.delayedDamageRefreshTime))
                    ))
                    {
                        cursor.RemoveRange(6);
                        cursor.Emit(OpCodes.Ldarg_0);
                        cursor.EmitDelegate<Action<CharacterBody>>(body =>
                        {
                            if (body.GetBuffCount(DLC2Content.Buffs.DelayedDamageDebuff) <= 0)
                            {
                                body.delayedDamageRefreshTime = 0;
                            }
                        });
                    }
                    else
                    {
                        Logger.LogWarning("Warped Echo - Failure to hook delayed damage");
                    }
                };
            }
            if (MainConfig.WarpedEchoFixEnabled.Value || MainConfig.WarpedEchoReworkEnabled.Value)
            {
                // Makes Delayed Damage not trigger on lethal hits
                IL.RoR2.HealthComponent.TakeDamageProcess += il =>
                {
                    var cursor = new ILCursor(il);

                    if (cursor.TryGotoNext(
                        x => x.MatchLdfld<HealthComponent>(nameof(HealthComponent.body)),
                        x => x.MatchLdsfld(typeof(DLC2Content.Buffs), nameof(DLC2Content.Buffs.DelayedDamageBuff)),
                        x => x.MatchCallvirt<CharacterBody>(nameof(CharacterBody.HasBuff))
                    ))
                    {
                        cursor.Index += 6;
                        cursor.RemoveRange(2);
                        cursor.Emit(OpCodes.Ldarg_0);
                        cursor.EmitDelegate<Func<HealthComponent, float>>(self => self.health * MainConfig.CWE_Cap.Value);
                        var label = il.DefineLabel();
                        cursor.Emit(OpCodes.Bgt_Un, label);

                        if (cursor.TryGotoNext(
                            x => x.MatchLdarg(1),
                            x => x.MatchLdfld<DamageInfo>(nameof(DamageInfo.damageType)),
                            x => x.MatchLdcI4(0x10000),
                            x => x.MatchCall<DamageTypeCombo>("op_Implicit"),
                            x => x.MatchCall<DamageTypeCombo>("op_BitwiseAnd")
                        ))
                        {
                            cursor.MarkLabel(label);
                        }
                    }
                    else
                    {
                        Logger.LogWarning("Warped Echo - Failure to hook");
                    }
                };

                /* // Makes Delayed Damage not trigger during pause menu
                IL.RoR2.CharacterBody.DoItemUpdates += il =>
                {
                    var cursor = new ILCursor(il);

                    if (cursor.TryGotoNext(
                        x => x.MatchCall(typeof(UnityEngine.Time), "get_fixedDeltaTime"),
                        x => x.MatchCall<CharacterBody>(nameof(CharacterBody.UpdateDelayedDamage)),
                        x => x.MatchLdarg(0)
                    ))
                    {
                        cursor.Remove();
                        cursor.Emit(OpCodes.Ldarg_1);

                        if (cursor.TryGotoNext(
                            x => x.MatchCall(typeof(UnityEngine.Time), "get_fixedDeltaTime"),
                            x => x.MatchCall<CharacterBody>(nameof(CharacterBody.UpdateSecondHalfOfDamage)),
                            x => x.MatchLdarg(0)
                        ))
                        {
                            cursor.Remove();
                            cursor.Emit(OpCodes.Ldarg_1);
                        }
                    }
                    else
                    {
                        Logger.LogWarning("Warped Echo - Failure to hook update");
                    }
                };*/
            }

            // Chronic Expansion Rework
            if (MainConfig.ChronicExpansionReworkEnabled.Value)
            {
                IL.RoR2.CharacterBody.RecalculateStats += il =>
                {
                    var cursor = new ILCursor(il);

                    if (cursor.TryGotoNext(
                        x => x.MatchLdloc(92), // V_92 (92)
                        x => x.MatchLdcI4(2), // .2
                        x => x.MatchBlt(out _) // 1556 (1042) ldloc.s V_92 (92)
                    ))
                    {
                        cursor.RemoveRange(27);
                        cursor.Emit(OpCodes.Ldarg_0);
                        cursor.Emit(OpCodes.Ldloc_S, (byte)54);
                        cursor.Emit(OpCodes.Ldloc_S, (byte)92);
                        cursor.Emit(OpCodes.Ldloc_S, (byte)93);
                        cursor.EmitDelegate<Func<CharacterBody, int, int, float, float>>((body, stack, buffs, dmgMod) =>
                        {
                            if (buffs > 0)
                            {
                                dmgMod = (float)buffs * (MainConfig.CCE_Base.Value + (stack - 1) * MainConfig.CCE_Stack.Value) * 0.01f;
                            }
                            return dmgMod;
                        });
                        cursor.Emit(OpCodes.Stloc_S, (byte)93);
                    }
                    else
                    {
                        Logger.LogWarning("Chronic Expansion - Failure to hook scaling");
                    }
                };
            }

            // Knockback Fin Rework
            if (MainConfig.KnockbackFinReworkEnabled.Value)
            {
                //DamageColorIndex colorIndex = (DamageColorIndex) DamageColor.colors.Length;
                //DamageColor.colors = DamageColor.colors.AddItem(new Color(0.4f, 0.65f, 1)).ToArray();
                
                IL.RoR2.HealthComponent.TakeDamageProcess += il =>
                {
                    var cursor = new ILCursor(il);
                    if (cursor.TryGotoNext(
                        x => x.MatchBrfalse(out _),
                        x => x.MatchLdloc(out _),
                        x => x.MatchLdarg(0),
                        x => x.MatchCallOrCallvirt<HealthComponent>("get_fullCombinedHealth")
                    ))
                    {
                        cursor.Index++;
                        cursor.Emit(OpCodes.Ldloc_1); // characterMaster
                        cursor.Emit(OpCodes.Ldarg_0); // this (HealthComponent)
                        cursor.Emit(OpCodes.Ldarg_1); // damageInfo
                        cursor.Emit(OpCodes.Ldloc_S, (byte)7); // num3 (damageInfo.damage)

                        cursor.EmitDelegate<Func<CharacterBody, HealthComponent, DamageInfo, float, float>>((characterBody, healthComponent, damageInfo, damage) =>
                        {
                            
                            float modDamage = damage;
                            int finCount = characterBody.inventory.GetItemCount(DLC2Content.Items.KnockBackHitEnemies);
                            if (finCount > 0)
                            {
                                if (healthComponent.body.characterMotor == null || (healthComponent.body.characterMotor != null && !healthComponent.body.characterMotor.isGrounded))
                                {
                                    modDamage *= (float) 1.0f + (MainConfig.CKF_KBase.Value + (finCount - 1) * MainConfig.CKF_KStack.Value) / 100.0f;
                                    EffectManager.SimpleImpactEffect(HealthComponent.AssetReferences.mercExposeConsumeEffectPrefab, damageInfo.position, Vector3.up, true);
                                }
                            }
                            return modDamage;
                        });
                        cursor.Emit(OpCodes.Stloc_S, (byte)7);
                    }
                    else
                    {
                        Logger.LogWarning("Knockback Fin - Failure to hook damage");
                    }
                };
            }
        }
    }
}
