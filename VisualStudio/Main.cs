using BepInEx;
using R2API;
using RoR2;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

using System.Threading.Tasks;
using System.Collections.Generic;
using EntityStates;
using UnityEngine.Networking;
using System.Collections;
using System.Linq;
using static UnityEngine.UI.GridLayoutGroup;

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
                // Set Delayed Damage delay timer.
                IL.RoR2.CharacterBody.SecondHalfOfDelayedDamage += il =>
                {
                    var cursor = new ILCursor(il);
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
                            damageInfo.timeUntilDamage = MainConfig.CWE_Delay.Value;
                        });
                    }
                    else
                    {
                        Logger.LogWarning("Warped Echo - Failure to hook timer");
                    }
                };

                // Set refresh immediately
                IL.RoR2.CharacterBody.UpdateSecondHalfOfDamage += il =>
                {
                    var cursor = new ILCursor(il);
                    if (cursor.TryGotoNext(
                        x => x.MatchCall<CharacterBody>(nameof(CharacterBody.RemoveBuff))
                    ))
                    {
                        cursor.Index++;
                        cursor.Emit(OpCodes.Ldarg_0);
                        cursor.EmitDelegate<Action<CharacterBody>>(body =>
                        {
                            body.AddBuff(DLC2Content.Buffs.DelayedDamageBuff);
                        });
                    }
                };
                
                // New logic behind losing and gaining Warped Echos.
                On.RoR2.CharacterBody.UpdateDelayedDamage += (orig, self, deltaTime) =>
                {
                    if (!NetworkServer.active)
                    {
                        Debug.LogWarning("[Server] function 'System.Void RoR2.CharacterBody::UpdateDelayedDamage(System.Single)' called on client");
                        return;
                    }

                    int itemCount = self.inventory ? self.inventory.GetItemCount(DLC2Content.Items.DelayedDamage) : 0;
                    if (itemCount > 0)
                    {
                        itemCount = 1 + MainConfig.CWE_Stack.Value * (itemCount - 1);
                        int buffCount = self.GetBuffCount(DLC2Content.Buffs.DelayedDamageBuff);
                        if (self.oldDelayedDamageCount != itemCount)
                        {
                            int newDiff = itemCount - self.oldDelayedDamageCount;
                            if (newDiff > 0)
                            {
                                for (int i = 0; i < Math.Abs(newDiff); i++)
                                {
                                    self.AddBuff(DLC2Content.Buffs.DelayedDamageBuff);
                                }
                            }
                            else
                            {
                                for (int i = 0; i < Math.Abs(newDiff); i++)
                                {
                                    self.RemoveBuff(DLC2Content.Buffs.DelayedDamageBuff);
                                }
                            }
                        }
                        self.oldDelayedDamageCount = itemCount;
                    }
                    else
                    {
                        self.oldDelayedDamageCount = 0;
                        self.RemoveBuff(DLC2Content.Buffs.DelayedDamageBuff);
                        self.RemoveBuff(DLC2Content.Buffs.DelayedDamageDebuff);
                        self.RemoveOldestTimedBuff(DLC2Content.Buffs.DelayedDamageDebuff);
                    }
                };
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
                // Changing proc chance
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
                            float procChance = (float)(MainConfig.CKF_KBase.Value + MainConfig.CKF_KStack.Value * (itemCount - 1)) * damageInfo.procCoefficient;

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

                // Damage to airborne
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
                                    modDamage *= (float) 1.0f + (MainConfig.CKF_DBase.Value + (finCount - 1) * MainConfig.CKF_DStack.Value) / 100.0f;
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

            // Bolstering Lantern Rework
            if (MainConfig.BolsteringLanternReworkEnabled.Value || MainConfig.BolsteringLanternReworkEnabled.Value)
            {
                // Skip Lanter's damage increase
                IL.RoR2.CharacterBody.RecalculateStats += il =>
                {
                    var cursor = new ILCursor(il);

                    if (cursor.TryGotoNext(
                        x => x.MatchLdarg(0),
                        x => x.MatchLdsfld(typeof(DLC2Content.Buffs), nameof(DLC2Content.Buffs.LowerHealthHigherDamageBuff)),
                        x => x.MatchCallOrCallvirt<CharacterBody>(nameof(CharacterBody.HasBuff))
                    ))
                    {
                        cursor.GotoNext(MoveType.After, x => x.MatchBrfalse(out _));
                        cursor.Emit(OpCodes.Br, cursor.Next);
                    }
                };
            }
            if (MainConfig.BolsteringLanternAltReworkEnabled.Value && !MainConfig.BolsteringLanternAltReworkEnabled.Value)
            {
                // Replaces Lantern's health functionality
                On.RoR2.CharacterBody.UpdateLowerHealthHigherDamage += (orig, body) =>
                {
                    if (!NetworkServer.active)
                    {
                        Debug.LogWarning("[Server] function 'System.Void RoR2.CharacterBody::UpdateLowerHealthHigherDamage(System.Single)' called on client");
                        return;
                    }
                    int itemCount = body.inventory ? body.inventory.GetItemCount(DLC2Content.Items.LowerHealthHigherDamage) : 0;
                    if (itemCount > 0)
                    {
                        float percentHealth = body.healthComponent.GetNormalizedHealth();
                        if (percentHealth <= 0.5f && !body.HasBuff(DLC2Content.Buffs.LowerHealthHigherDamageBuff))
                        {
                            body.AddBuff(DLC2Content.Buffs.LowerHealthHigherDamageBuff);
                            Util.PlaySound("Play_item_proc_lowerHealthHigherDamage_proc", body.gameObject);
                            //this.TransmitItemBehavior(new CharacterBody.NetworkItemBehaviorData(DLC2Content.Items.LowerHealthHigherDamage.itemIndex, 1f));
                            return;
                        }
                        else if (percentHealth >= 0.9f && body.HasBuff(DLC2Content.Buffs.LowerHealthHigherDamageBuff))
                        {
                            body.RemoveBuff(DLC2Content.Buffs.LowerHealthHigherDamageBuff);
                            return;
                        }
                    }
                    else if (itemCount <= 0 && body.HasBuff(DLC2Content.Buffs.LowerHealthHigherDamageBuff))
                    {
                        body.RemoveBuff(DLC2Content.Buffs.LowerHealthHigherDamageBuff);
                    }
                };

                // Trigger fire damage when buffed
                On.RoR2.GlobalEventManager.ProcessHitEnemy += (orig, self, damageInfo, victim) =>
                {
                    orig(self, damageInfo, victim);
                    if (damageInfo.attacker && damageInfo.procCoefficient > 0f)
                    {
                        CharacterBody characterBody = damageInfo.attacker.GetComponent<CharacterBody>();
                        if (characterBody && characterBody.HasBuff(DLC2Content.Buffs.LowerHealthHigherDamageBuff))
                        {
                            int itemCount = characterBody.inventory.GetItemCount(DLC2Content.Items.LowerHealthHigherDamage);
                            InflictDotInfo burnDot = new()
                            {
                                attackerObject = damageInfo.attacker,
                                victimObject = victim,
                                totalDamage = new float?(damageInfo.damage * (MainConfig.CBL_BBase.Value + MainConfig.CBL_BStack.Value * (itemCount - 1)) / 100f),
                                damageMultiplier = 1.0f,
                                duration = 0.1f,
                                dotIndex = DotController.DotIndex.Burn,
                            };
                            StrengthenBurnUtils.CheckDotForUpgrade(characterBody.inventory, ref burnDot);
                            DotController.InflictDot(ref burnDot);
                        }
                    }
                };
            }
            else if (MainConfig.BolsteringLanternAltReworkEnabled.Value)
            {
                new LanternCountBuff();

                // Remove original Lantern's health functionality
                On.RoR2.CharacterBody.UpdateLowerHealthHigherDamage += (orig, body) =>
                {
                    return;
                };

                On.RoR2.CharacterBody.FixedUpdate += (orig, body) =>
                {
                    orig(body);
                    int lanternCount = body.inventory ? body.inventory.GetItemCount(DLC2Content.Items.LowerHealthHigherDamage) : 0;
                    //setLanternCount(body, lanternCount);
                    //if (body) setLanternCount(body, lanternCount);

                    MinionOwnership.MinionGroup allies = MinionOwnership.MinionGroup.FindGroup(body.master.netId);
                    if (allies == null) return;
                    foreach (MinionOwnership specificAlly in allies.members)
                    {
                        if (specificAlly == null) continue;
                        CharacterMaster allyMaster = specificAlly.GetComponent<CharacterMaster>();
                        if (allyMaster && allyMaster.inventory)
                        {
                            if (allyMaster.GetBody()) setLanternCount(allyMaster.GetBody(), lanternCount);
                        }
                    }
                };

                // Trigger fire damage chance instead when buffed
                On.RoR2.GlobalEventManager.ProcessHitEnemy += (orig, self, damageInfo, victim) =>
                {
                    orig(self, damageInfo, victim);
                    if (damageInfo.attacker && damageInfo.procCoefficient > 0f)
                    {
                        CharacterBody allyBody = damageInfo.attacker.GetComponent<CharacterBody>();
                        int countBuffs = allyBody.GetBuffCount(LanternCountBuff.LanternCounter);
                        if (allyBody && countBuffs > 0 && Util.CheckRoll(MainConfig.CBL_ProcBase.Value + MainConfig.CBL_ProcStack.Value * (countBuffs - 1), 0f, null))
                        {
                            Inventory targetInventory = (allyBody.master.minionOwnership.group != null && allyBody.master.minionOwnership.group.resolvedOwnerMaster) ? allyBody.master.minionOwnership.group.resolvedOwnerMaster.inventory : allyBody.inventory;
                            InflictDotInfo burnDot = new()
                            {
                                attackerObject = damageInfo.attacker,
                                victimObject = victim,
                                totalDamage = new float?(damageInfo.damage * (MainConfig.CBL_BBase.Value + MainConfig.CBL_BStack.Value * (countBuffs - 1)) / 100f),
                                damageMultiplier = 1.0f,
                                duration = 0.1f,
                                dotIndex = DotController.DotIndex.Burn,
                            };
                            StrengthenBurnUtils.CheckDotForUpgrade(targetInventory, ref burnDot);
                            DotController.InflictDot(ref burnDot);
                        }

                        int countItems = allyBody.inventory ? allyBody.inventory.GetItemCount(DLC2Content.Items.LowerHealthHigherDamage) : 0;
                        if (countItems > 0 && Util.CheckRoll(MainConfig.CBL_ProcBase.Value + MainConfig.CBL_ProcStack.Value * (countItems - 1), 0f, null))
                        {
                            InflictDotInfo burnDot = new()
                            {
                                attackerObject = damageInfo.attacker,
                                victimObject = victim,
                                totalDamage = new float?(damageInfo.damage * (MainConfig.CBL_BBase.Value + MainConfig.CBL_BStack.Value * (countItems - 1)) / 100f),
                                damageMultiplier = 1.0f,
                                duration = 0.1f,
                                dotIndex = DotController.DotIndex.Burn,
                            };
                            StrengthenBurnUtils.CheckDotForUpgrade(allyBody.inventory, ref burnDot);
                            DotController.InflictDot(ref burnDot);
                        }
                    }
                };

                static void setLanternCount(CharacterBody ally, int itemCount)
                {
                    if (ally.GetBuffCount(LanternCountBuff.LanternCounter) != itemCount)
                    {
                        ally.SetBuffCount(LanternCountBuff.LanternCounter.buffIndex, itemCount);
                    } 
                    else if (ally.HasBuff(LanternCountBuff.LanternCounter) && itemCount <= 0)
                    {
                        ally.SetBuffCount(LanternCountBuff.LanternCounter.buffIndex, 0);
                    }
                }
            }

            // Antler Shield Rework
            if (MainConfig.AntlerShieldReworkEnabled.Value)
            {
                // Scale armor with speed, and gain speed
                IL.RoR2.CharacterBody.RecalculateStats += il =>
                {
                    var cursor = new ILCursor(il);
                    if (cursor.TryGotoNext(
                        x => x.MatchCall<CharacterBody>("set_armor"),
                        x => x.MatchLdarg(0),
                        x => x.MatchLdsfld(typeof(RoR2Content.Buffs), nameof(RoR2Content.Buffs.Cripple))
                    ))
                    {
                        cursor.Index++;
                        cursor.Emit(OpCodes.Ldarg_0);
                        cursor.EmitDelegate<Action<CharacterBody>>(body =>
                        {
                            if (body.inventory)
                            {
                                int antlerCount = body.inventory.GetItemCount(DLC2Content.Items.NegateAttack);
                                if (antlerCount > 0)
                                {
                                    body.armor += (float)(MainConfig.CAS_ABase.Value + MainConfig.CAS_AStack.Value * (antlerCount - 1)) / 100f * body.moveSpeed;
                                    body.moveSpeed *= 1 + (MainConfig.CAS_MBase.Value + MainConfig.CAS_MStack.Value * (antlerCount - 1)) / 100f;
                                }
                            }
                        });
                    }
                    else
                    {
                        Logger.LogWarning("Antler Shield - Failure to hook calculate stats");
                    }
                };

                // Remove old Antler reflect
                IL.RoR2.HealthComponent.TakeDamageProcess += il =>
                {
                    var cursor = new ILCursor(il);
                    if (cursor.TryGotoNext(
                        x => x.MatchLdfld(typeof(HealthComponent.ItemCounts), nameof(HealthComponent.itemCounts.antlerShield)),
                        x => x.MatchLdcI4(0)
                    ))
                    {
                        cursor.Index += 2;
                        cursor.EmitDelegate<Func<int, int>>(stack => int.MaxValue);
                    }
                    else
                    {
                        Logger.LogWarning("Antler Shield - Failure to hook condition removal");
                    }
                };
            }
        }
        private void SetUpUncommonItems()
        {
            // Unstable Transmitter Rework
            if (MainConfig.UnstableTransmitterReworkEnabled.Value)
            {
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
                            int unstableCount = body.inventory.GetItemCount(DLC2Content.Items.TeleportOnLowHealth);
                            EntityStateMachine.FindByCustomName(body.gameObject, "Body").SetNextState(new IntangibleSkillState());
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
                                    DotController.InflictDot
                                    (
                                        victimBody.gameObject,
                                        body.gameObject,
                                        DotController.DotIndex.Bleed,
                                        3.0f,
                                        body.damage * (MainConfig.UUT_BBase.Value + MainConfig.UUT_BStack.Value * (unstableCount - 1)) / 100,
                                        null
                                    );
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

                // Change refresh duration & remove invincibility
                IL.RoR2.HealthComponent.UpdateLastHitTime += il =>
                {
                    var durationIndex = -1;
                    var cursor = new ILCursor(il);
                    if (cursor.TryGotoNext(
                        x => x.MatchStloc(out durationIndex),
                        x => x.MatchLdarg(0),
                        x => x.MatchLdfld<HealthComponent>(nameof(HealthComponent.body)),
                        x => x.MatchLdsfld(typeof(DLC2Content.Buffs), nameof(DLC2Content.Buffs.TeleportOnLowHealthCooldown)),
                        x => x.MatchLdloc(out _)
                    ))
                    {
                        cursor.Index++;
                        cursor.Emit(OpCodes.Ldloc_S, (byte)durationIndex);
                        cursor.EmitDelegate<Func<float, float>>(duration => MainConfig.UUT_Refresh.Value);
                        cursor.Emit(OpCodes.Stloc_S, (byte)durationIndex);
                    }

                    if (cursor.TryGotoNext(
                        x => x.MatchLdsfld(typeof(RoR2Content.Buffs), nameof(RoR2Content.Buffs.HiddenInvincibility)),
                        x => x.MatchLdcR4(2),
                        x => x.MatchCallvirt<CharacterBody>(nameof(CharacterBody.AddTimedBuff))
                    ))
                    {
                        cursor.Index += 2;
                        cursor.EmitDelegate<Func<float, float>>(duration => 0f);
                    }
                };
            }
        }   
        private void SetUpLegendaryItems()
        {
            // War Bonds Rework & VFX Replace
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

            // Growth Nectar Rework
            if (MainConfig.GrowthNectarReworkEnabled.Value)
            {
                On.RoR2.EquipmentSlot.MyFixedUpdate += (orig, equip, deltaTime) =>
                {
                    orig(equip, deltaTime);
                    if (NetworkServer.active && equip.characterBody.inventory)
                    {
                        int nectarCount = equip.characterBody.inventory.GetItemCount(DLC2Content.Items.BoostAllStats);
                        bool nonEquip = equip.cooldownTimer == float.PositiveInfinity || equip.cooldownTimer == float.NegativeInfinity;
                        bool hasBuff = equip.characterBody.HasBuff(DLC2Content.Buffs.BoostAllStatsBuff);
                        if (nectarCount > 0 && !hasBuff && (equip.cooldownTimer <= 0 || nonEquip))
                        {
                            equip.characterBody.AddBuff(DLC2Content.Buffs.BoostAllStatsBuff);
                        }
                        else if (nectarCount <= 0 || !nonEquip)
                        {
                            equip.characterBody.RemoveBuff(DLC2Content.Buffs.BoostAllStatsBuff);
                        }
                    }
                };

                IL.RoR2.CharacterBody.UpdateBoostAllStatsTimer += il =>
                {
                    var cursor = new ILCursor(il);
                    cursor.Emit(OpCodes.Ret);
                };

                IL.RoR2.CharacterBody.RecalculateStats += il =>
                {
                    var cursor = new ILCursor(il);
                    if (cursor.TryGotoNext(
                        x => x.MatchLdcI4(0),
                        x => x.MatchBle(out _),
                        x => x.MatchLdarg(0),
                        x => x.MatchLdsfld(typeof(DLC2Content.Buffs), nameof(DLC2Content.Buffs.BoostAllStatsBuff))
                    ))
                    {
                        cursor.Index++;
                        cursor.EmitDelegate<Func<int, int>>(stack => int.MaxValue);
                    }
                    else
                    {
                        Logger.LogWarning("Growth Nectar - Failure to hook condition removal");
                    }
                };

                IL.RoR2.CharacterBody.RecalculateStats += il =>
                {
                    var numCountIndex = -1;
                    var cursor = new ILCursor(il);
                    if (cursor.TryGotoNext(
                        x => x.MatchLdloc(out numCountIndex), // Index of 2328
                        x => x.MatchConvR4(),
                        x => x.MatchLdarg(0),
                        x => x.MatchLdfld<CharacterBody>(nameof(CharacterBody.boostAllStatsMultiplier)) // Remove @ 2405
                    ))
                    {
                        cursor.RemoveRange(78);
                        cursor.Emit(OpCodes.Ldarg_0);
                        cursor.Emit(OpCodes.Ldloc, numCountIndex);
                        cursor.EmitDelegate<Action<CharacterBody, int>>((body, nectarCount) =>
                        {
                            EquipmentSlot equip = body.equipmentSlot;
                            float multiplier = (float) (MainConfig.LGN_SBase.Value + MainConfig.LGN_SStack.Value * (nectarCount - 1) + Math.Min(MainConfig.LGN_Charge.Value * (equip.maxStock - 1), MainConfig.LGN_CBase.Value + MainConfig.LGN_CStack.Value * (nectarCount - 1))) / 100;

                            body.maxHealth += body.maxHealth * multiplier;
                            body.moveSpeed += body.moveSpeed * multiplier;
                            body.damage += body.damage * multiplier;
                            body.attackSpeed += body.attackSpeed * multiplier;
                            body.crit += body.crit * multiplier;
                            body.regen += body.regen * multiplier;
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
                //disableSkill.isHidden = true;
            }

            if (MainConfig.StealthKitCleanse.Value)
            {
                IL.RoR2.Items.PhasingBodyBehavior.FixedUpdate += il =>
                {
                    var cursor = new ILCursor(il);
                    if (cursor.TryGotoNext(
                        x => x.MatchLdarg(0),
                        x => x.MatchCall(typeof(RoR2.Items.BaseItemBodyBehavior), "get_body"),
                        x => x.MatchLdsfld(typeof(RoR2Content.Buffs), nameof(RoR2Content.Buffs.Cloak))
                    ))
                    {
                        //cursor.Index++;
                        cursor.Emit(OpCodes.Ldarg_0);
                        cursor.EmitDelegate<Action<RoR2.Items.PhasingBodyBehavior>>(itemBase =>
                        {
                            int numBuffs = itemBase.body.activeBuffsListCount;
                            int itemCount = itemBase.body.inventory.GetItemCount(RoR2Content.Items.Phasing);
                            int numCleansed = 1 + itemCount;
                            for (int i = 0; i < numBuffs; i++)
                            {
                                BuffDef foundBuff = BuffCatalog.GetBuffDef(itemBase.body.activeBuffsList[i]);
                                if (foundBuff.isDebuff && numCleansed > 0)
                                {
                                    numCleansed--; i--;
                                    itemBase.body.RemoveBuff(foundBuff);
                                }
                                else if (numCleansed <= 0) { break; }
                            }
                            if (numCleansed < 1 + itemCount)
                            {
                                EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/CleanseEffect"), new EffectData
                                {
                                    origin = itemBase.body.transform.position
                                }, true);
                            }
                        });
                    }
                    else
                    {
                        Logger.LogWarning("Old War Stealth Kit - Failure to hook new effect");
                    }
                };
            }
        }
    }
}
