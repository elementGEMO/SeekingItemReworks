using BepInEx;
using R2API;
using RoR2;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

using SeekingItemReworks;
using System.Threading.Tasks;
using System.Collections.Generic;

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
            SetUpUncommonItems();
            SetUpLegendaryItems();
            SetUpMisc();
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

                IL.RoR2.GlobalEventManager.ProcessHitEnemy += il =>
                {
                    var cursor = new ILCursor(il);
                    if (cursor.TryGotoNext(
                        x => x.MatchLdarg(2), // 2384 ldarg.2
                        x => x.MatchCallvirt(typeof(UnityEngine.GameObject), nameof(UnityEngine.GameObject.GetComponent)),
                        x => x.MatchStloc(out _),
                        x => x.MatchLdarg(2)
                    ))
                    {
                        // End to 2509
                        cursor.RemoveRange(126);
                        cursor.Emit(OpCodes.Ldarg_1);
                        cursor.Emit(OpCodes.Ldarg_2);

                        cursor.EmitDelegate<Action<DamageInfo, GameObject>>((damageInfo, victim) =>
                        {
                            CharacterBody victimBody = victim.GetComponent<CharacterBody>();
                            CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                            CharacterMotor victimMotor = victim.GetComponent<CharacterMotor>();
                            victim.GetComponent<RigidbodyMotor>();

                            int itemCount = attackerBody.inventory.GetItemCount(DLC2Content.Items.KnockBackHitEnemies);
                            float procChance = (float)MainConfig.CKF_KBase.Value + (MainConfig.CKF_KStack.Value * (itemCount - 1)) * damageInfo.procCoefficient;

                            if (MainConfig.CKF_Hyperbolic.Value)
                            {
                                procChance = Util.ConvertAmplificationPercentageIntoReductionPercentage(procChance);
                            }
                            if (victimMotor && victimMotor.isGrounded && !victimBody.isChampion && (victimBody.bodyFlags & CharacterBody.BodyFlags.IgnoreFallDamage) == CharacterBody.BodyFlags.None && !victimBody.HasBuff(DLC2Content.Buffs.KnockUpHitEnemies) && Util.CheckRoll(procChance, 0f, null))
                            {
                                victimBody.AddTimedBuff(DLC2Content.Buffs.KnockUpHitEnemies, 5f);
                                float scale = 1f;
                                switch (victimBody.hullClassification)
                                {
                                    case HullClassification.Human:
                                        scale = 1f;
                                        break;
                                    case HullClassification.Golem:
                                        scale = 5f;
                                        break;
                                    case HullClassification.BeetleQueen:
                                        scale = 10f;
                                        break;
                                }
                                EffectManager.SpawnEffect(GlobalEventManager.CommonAssets.knockbackFinEffect, new EffectData
                                {
                                    origin = victimBody.gameObject.transform.position,
                                    scale = scale
                                }, true);
                                if (!victimBody.mainHurtBox)
                                {
                                    Transform transform = victimBody.transform;
                                }
                                else
                                {
                                    Transform transform2 = victimBody.mainHurtBox.transform;
                                }
                                Vector3 a = new(0, 1f, 0);
                                float victimMass = victimMotor.mass * 25f;
                                victimMotor.ApplyForce(victimMass * a, false, false);
                                Util.PlaySound("Play_item_proc_knockBackHitEnemies", attackerBody.gameObject);
                            }
                        });
                    }
                };

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
                                if (healthComponent.body.isFlying || (healthComponent.body.characterMotor != null && !healthComponent.body.characterMotor.isGrounded))
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
        private void SetUpUncommonItems()
        {
            // Unstable Transmitter Rework
            if (MainConfig.UnstableTransmitterReworkEnabled.Value)
            {
                // Transmitter Effect Buff
                new TransmitterEffect();

                // Bleed area effect, and intangibility
                IL.RoR2.CharacterBody.RpcTeleportCharacterToSafety += il =>
                {
                    var cursor = new ILCursor(il);
                    if (cursor.TryGotoNext(
                        x => x.MatchLdarg(0), // 0
                        x => x.MatchCall<CharacterBody>("get_hasEffectiveAuthority")
                    ))
                    {
                        cursor.RemoveRange(176); // Remove @ 175
                        cursor.Emit(OpCodes.Ldarg_0);
                        cursor.EmitDelegate<Action<CharacterBody>>(body =>
                        {
                            if (!body.hasEffectiveAuthority)
                            {
                                return;
                            }
                            body.AddTimedBuff(TransmitterEffect.getTransmitterDef(), 2.0f);
                            EffectManager.SpawnEffect(CharacterBody.CommonAssets.teleportOnLowHealthExplosion, new EffectData
                            {
                                origin = body.coreTransform.position,
                                scale = 30,
                                rotation = Quaternion.identity
                            }, true);
                            SphereSearch sphereSearch = new SphereSearch();
                            List<HurtBox> list = HG.CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
                            sphereSearch.mask = LayerIndex.entityPrecise.mask;
                            sphereSearch.origin = body.coreTransform.position;
                            sphereSearch.radius = 30;
                            sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;
                            sphereSearch.RefreshCandidates();
                            sphereSearch.FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(body.teamComponent.teamIndex));
                            sphereSearch.OrderCandidatesByDistance();
                            sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
                            sphereSearch.GetHurtBoxes(list);
                            sphereSearch.ClearCandidates();
                            for (int i = 0; i < list.Count; i++)
                            {
                                HurtBox hurtBox = list[i];
                                CharacterBody victimBody = hurtBox.healthComponent.body;
                                if (hurtBox && hurtBox.healthComponent && hurtBox.healthComponent.alive)
                                {
                                    DotController.InflictDot(victimBody.gameObject, body.gameObject, DotController.DotIndex.Bleed, 3.0f, body.damage, null);
                                }
                            }
                            body.hasTeleported = true;
                        });
                    }
                    else
                    {
                        Logger.LogWarning("Unstable Transmitter - Failure to hook replacement");
                    }
                };
            }
        }
        private void SetUpLegendaryItems()
        {
            // Warped Echo Rework & Bug Fix
            if (MainConfig.WarBondsReworkEnabled.Value)
            {
                IL.RoR2.CharacterBody.Start += il =>
                {
                    var cursor = new ILCursor(il);
                    if (cursor.TryGotoNext(
                        x => x.MatchCall<Run>("get_instance"), // Index 130
                        x => x.MatchLdcR4(out _),
                        x => x.MatchLdarg(0),
                        x => x.MatchCall<CharacterBody>("get_level") // Cleanse to 146
                    ))
                    {
                        cursor.RemoveRange(17);
                        cursor.Emit(OpCodes.Ldarg_0);
                        cursor.EmitDelegate<Action<CharacterBody>>(body =>
                        {
                            int itemCount = body.inventory.GetItemCount(DLC2Content.Items.GoldOnStageStart);
                            for (int i = 0; i < (MainConfig.LWB_PBase.Value + MainConfig.LWB_PStack.Value * (itemCount - 1)); i++)
                            {
                                body.AddBuff(DLC2Content.Buffs.FreeUnlocks);
                            }
                        });
                    }
                };

                IL.RoR2.PurchaseInteraction.OnInteractionBegin += il =>
                {
                    var costIndex = -1;
                    var cursor = new ILCursor(il);
                    if (cursor.TryGotoNext(
                        x => x.MatchLdarg(0),
                        x => x.MatchLdfld<PurchaseInteraction>(nameof(PurchaseInteraction.cost)),
                        x => x.MatchStloc(out costIndex)
                    ))
                    {
                        cursor.Index += 3;
                        cursor.Emit(OpCodes.Ldarg_0);
                        cursor.Emit(OpCodes.Ldarg_1);
                        cursor.EmitDelegate<Action<PurchaseInteraction, Interactor>>((interact, activator) =>
                        {
                            CharacterBody body = activator.GetComponent<CharacterBody>();
                            int warBondCount = body.inventory.GetItemCount(DLC2Content.Items.GoldOnStageStart);
                            if (warBondCount > 0 && interact.costType == CostTypeIndex.Money)
                            {
                                ExperienceManager.instance.AwardExperience(interact.transform.position, body, (ulong)(interact.cost * (MainConfig.LWB_EBase.Value + MainConfig.LWB_EStack.Value * (warBondCount - 1)) / 100));
                            }
                        });
                    }
                };
            }

            if (MainConfig.WarBondsReplaceVFX.Value)
            {
                IL.RoR2.GoldOnStageStartBehaviour.GiveWarBondsGold += il =>
                {
                    var cursor = new ILCursor(il);
                    if (cursor.TryGotoNext(
                        x => x.MatchLdsfld(typeof(CharacterBody.CommonAssets), nameof(CharacterBody.CommonAssets.goldOnStageStartEffect)), // Index 21
                        x => x.MatchNewobj(out _),
                        x => x.MatchDup(),
                        x => x.MatchLdarg(0) // Cleanse to 29
                    ))
                    {
                        cursor.RemoveRange(9);
                        cursor.Emit(OpCodes.Ldarg_0);
                        cursor.EmitDelegate<Action<GoldOnStageStartBehaviour>>(async body =>
                        {
                            await Task.Delay(1500);
                            for (int i = 0; i < 20; i++)
                            {
                                await Task.Delay(75);
                                EffectManager.SpawnEffect(HealthComponent.AssetReferences.gainCoinsImpactEffectPrefab, new EffectData
                                {
                                    origin = body.transform.position
                                }, false);
                            }
                        });
                    }
                };
            }
        }
        private void SetUpMisc()
        {
            if (MainConfig.SkillDisableCleansable.Value)
            {
                BuffDef disableSkill = Addressables.LoadAsset<BuffDef>("RoR2/DLC2/bdDisableAllSkills.asset").WaitForCompletion();
                disableSkill.isDebuff = true;
                disableSkill.isHidden = true;
            }
        }

        private void MultiShopCardUtils_OnNonMoneyPurchase(ILContext il)
        {
            throw new NotImplementedException();
        }
    }
}
