using BepInEx;
using R2API;
using RoR2;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using EntityStates;
using UnityEngine.Networking;

[assembly: HG.Reflection.SearchableAttribute.OptIn]

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
        public const string PluginVersion = "1.4.0";

        public void Awake()
        {
            Log.Init(Logger);

            MainConfig.SetUpConfigs(this);
            new ItemInfo();

            SetUpCommonItems();
            SetUpUncommonItems();
            SetUpLegendaryItems();
        }

        private void SetUpCommonItems()
        {
            // -- Seekers of the Storm Content -- \\

            // Warped Echo #1
            if (WarpedEcho.Rework.Value == 1)
            {
                // Delay timer
                IL.RoR2.CharacterBody.SecondHalfOfDelayedDamage += il =>
                {
                    var cursor = new ILCursor(il);
                    var damageIndex = -1;

                    if (cursor.TryGotoNext(
                            x => x.MatchLdloc(out damageIndex),
                            x => x.MatchLdarg(1),
                            x => x.MatchStfld(typeof(CharacterBody.DelayedDamageInfo), nameof(CharacterBody.DelayedDamageInfo.halfDamage))
                        ))
                    {
                        cursor.Emit(OpCodes.Ldarg_0);
                        cursor.Emit(OpCodes.Ldloc, damageIndex);
                        cursor.EmitDelegate<Action<CharacterBody, CharacterBody.DelayedDamageInfo>>((body, damageInfo) =>
                        {
                            damageInfo.timeUntilDamage = WarpedEcho.Delay_Base.Value;
                        });
                    }
                    else
                    {
                        Logger.LogWarning(WarpedEcho.StaticName + " #1 - IL Fail #1");
                    }
                };

                // Refresh immediately
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
                    else
                    {
                        Logger.LogWarning(WarpedEcho.StaticName + " #1 - IL Fail #2");
                    }
                };

                // New logic behind losing and gaining Warped Echos.
                On.RoR2.CharacterBody.UpdateDelayedDamage += (orig, self, deltaTime) =>
                {
                    if (!NetworkServer.active) return;

                    int itemCount = self.inventory ? self.inventory.GetItemCount(DLC2Content.Items.DelayedDamage) : 0;
                    if (itemCount > 0)
                    {
                        itemCount = 1 + WarpedEcho.Instance_Stack.Value * (itemCount - 1);
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

            // Chronic Expansion #1
            if (ChronicExpansion.Rework.Value == 1)
            {
                IL.RoR2.CharacterBody.RecalculateStats += il =>
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
                        Logger.LogWarning(ChronicExpansion.StaticName + " - IL Fail #1");
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

                            if (body.oldComboMeter < buffCount)
                            {
                                body.oldComboMeter = buffCount;
                            }
                        });
                        var skipLabel = cursor.DefineLabel();
                        cursor.Emit(OpCodes.Br_S, skipLabel);

                        if (cursor.TryGotoNext(
                            x => x.MatchLdarg(0),
                            x => x.MatchLdloc(out _),
                            x => x.MatchStfld<CharacterBody>(nameof(CharacterBody.oldComboMeter))
                        ))
                        {
                            cursor.MarkLabel(skipLabel);
                        }
                    }
                    else
                    {
                        Logger.LogWarning(ChronicExpansion.StaticName + " #1 - IL Fail #2");
                    }
                };
            }

            // Knockback Fin #1
            if (KnockbackFin.Rework.Value == 1)
            {
                // Disable vanilla Knockback Fin
                IL.RoR2.GlobalEventManager.ProcessHitEnemy += il =>
                {
                    var cursor = new ILCursor(il);
                    var itemIndex = -1;

                    if (cursor.TryGotoNext(
                        x => x.MatchLdloc(out itemIndex),
                        x => x.MatchLdcI4(out _),
                        x => x.MatchBle(out _),
                        x => x.MatchLdloc(out _),
                        x => x.MatchCallOrCallvirt<CharacterBody>("get_isBoss")
                    ))
                    {
                        cursor.Emit(OpCodes.Ldloc, itemIndex);
                        cursor.EmitDelegate<Func<int, int>>(itemCount => { return -1; });
                        cursor.Emit(OpCodes.Stloc, itemIndex);
                    }
                    else
                    {
                        Logger.LogWarning(KnockbackFin.StaticName + " #1 - IL Fail #1");
                    }
                };

                // New Knockback on hit effect chance
                On.RoR2.GlobalEventManager.ProcessHitEnemy += (orig, self, damageInfo, victim) =>
                {
                    orig(self, damageInfo, victim);
                    if (damageInfo.attacker && damageInfo.procCoefficient > 0)
                    {
                        CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                        CharacterBody victimBody = victim.GetComponent<CharacterBody>();
                        int itemCount = attackerBody.inventory ? attackerBody.inventory.GetItemCount(DLC2Content.Items.KnockBackHitEnemies) : 0;

                        if (victimBody && itemCount > 0)
                        {
                            CharacterMotor victimMotor = victim.GetComponent<CharacterMotor>(); victim.GetComponent<RigidbodyMotor>();
                            float procChance = (float)(KnockbackFin.Chance_Base.Value + KnockbackFin.Chance_Stack.Value * (itemCount - 1)) * damageInfo.procCoefficient;

                            if (KnockbackFin.IsHyperbolic.Value) procChance = Util.ConvertAmplificationPercentageIntoReductionPercentage(procChance);
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

                                Util.PlaySound("Play_item_proc_knockBackHitEnemies", attackerBody.gameObject);
                                EffectManager.SpawnEffect(GlobalEventManager.CommonAssets.knockbackFinEffect, new EffectData
                                {
                                    origin = victimBody.gameObject.transform.position,
                                    scale = scale
                                }, true);

                                Vector3 upHeight = new(0, 1f, 0);
                                float victimMass = victimMotor.mass * 25f;
                                victimMotor.ApplyForce(victimMass * upHeight, false, false);
                            }
                        }
                    }
                };

                // Damage to flying target
                IL.RoR2.HealthComponent.TakeDamageProcess += il =>
                {
                    var cursor = new ILCursor(il);
                    var damageIndex = -1;

                    if (cursor.TryGotoNext(
                        x => x.MatchStloc(out damageIndex),
                        x => x.MatchLdloc(out _),
                        x => x.MatchLdarg(0),
                        x => x.MatchLdfld<HealthComponent>(nameof(HealthComponent.body)),
                        x => x.MatchCallOrCallvirt<CharacterBody>("get_teamComponent")
                    )) { }
                    else
                    {
                        Logger.LogWarning(KnockbackFin.StaticName + " #1 - IL Fail #2");
                    }

                    Logger.LogDebug("DamageIndex: " + damageIndex);

                    if (cursor.TryGotoNext(
                        x => x.MatchLdloc(out _),
                        x => x.MatchLdarg(0),
                        x => x.MatchCallOrCallvirt<HealthComponent>("get_fullCombinedHealth")
                    ))
                    {
                        Logger.LogDebug("1");
                        cursor.Emit(OpCodes.Ldarg_0);
                        Logger.LogDebug("2");
                        cursor.Emit(OpCodes.Ldarg_1);
                        Logger.LogDebug("3");
                        cursor.Emit(OpCodes.Ldloc, damageIndex);
                        Logger.LogDebug("4");

                        cursor.EmitDelegate<Func<HealthComponent, DamageInfo, float, float>>((healthComponent, damageInfo, damage) =>
                        {
                            float damageMod = damage;
                            CharacterBody characterBody = damageInfo.attacker ? damageInfo.attacker.GetComponent<CharacterBody>() : null;
                            int itemCount = (characterBody && characterBody.inventory) ? characterBody.inventory.GetItemCount(DLC2Content.Items.KnockBackHitEnemies) : 0;
                            if (itemCount > 0)
                            {
                                if (healthComponent.body.isFlying || (healthComponent.body.characterMotor != null && !healthComponent.body.characterMotor.isGrounded))
                                {
                                    damageMod *= (float) 1f + (KnockbackFin.Damage_Base.Value + KnockbackFin.Damage_Stack.Value * (itemCount - 1)) / 100f;
                                    EffectManager.SimpleImpactEffect(HealthComponent.AssetReferences.mercExposeConsumeEffectPrefab, damageInfo.position, Vector3.up, true);
                                }
                            }
                            return damageMod;
                        });

                        Logger.LogDebug("5");
                        cursor.Emit(OpCodes.Stloc, damageIndex);
                        Logger.LogDebug("6");
                    }
                    else
                    {
                        Logger.LogWarning(KnockbackFin.StaticName + " #1 - IL Fail #3");
                    }
                };
            }

            // Bolstering Lantern #0
            if (BolsteringLantern.Rework.Value > 0)
            {
                // Skip Lantern's damage increase
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
                    else
                    {
                        Logger.LogWarning(BolsteringLantern.StaticName + " #0 - IL Fail #1");
                    }
                };
            }
            // Bolstering Lantern #1
            if (BolsteringLantern.Rework.Value == 1)
            {
                // New health function
                On.RoR2.CharacterBody.UpdateLowerHealthHigherDamage += (orig, body) =>
                {
                    if (!NetworkServer.active) return;

                    int itemCount = body.inventory ? body.inventory.GetItemCount(DLC2Content.Items.LowerHealthHigherDamage) : 0;
                    if (itemCount > 0)
                    {
                        float percentHealth = body.healthComponent.GetNormalizedHealth();
                        if (percentHealth <= (BolsteringLantern.LowHealth.Value / 100f) && !body.HasBuff(DLC2Content.Buffs.LowerHealthHigherDamageBuff))
                        {
                            body.AddBuff(DLC2Content.Buffs.LowerHealthHigherDamageBuff);
                            Util.PlaySound("Play_item_proc_lowerHealthHigherDamage_proc", body.gameObject);
                            return;
                        }
                        else if (percentHealth >= (BolsteringLantern.HighHealth.Value / 100f) && body.HasBuff(DLC2Content.Buffs.LowerHealthHigherDamageBuff))
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
                    if (damageInfo.attacker && damageInfo.procCoefficient > 0)
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
                    else
                    {
                        Logger.LogWarning(KnockbackFin.StaticName + " #1 - IL Fail #1");
                    }
                };
            }
            // Bolstering Lantern #2
            if (BolsteringLantern.Rework.Value == 2)
            {
                // Add buff for counting stacks on minions
                new LanternCountBuff();

                // Remove original Lantern's health functionality
                On.RoR2.CharacterBody.UpdateLowerHealthHigherDamage += (orig, body) => { return; };

                // Apply counting buffs to minions
                On.RoR2.CharacterBody.FixedUpdate += (orig, body) =>
                {
                    orig(body);
                    int lanternCount = (body && body.inventory) ? body.inventory.GetItemCount(DLC2Content.Items.LowerHealthHigherDamage) : 0;
                    if (lanternCount > 0)
                    {
                        MinionOwnership.MinionGroup allyList = MinionOwnership.MinionGroup.FindGroup(body.masterObjectId);
                        body.SetBuffCount(LanternCountBuff.LanternCounter.buffIndex, lanternCount);

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
                };

                // Trigger fire damage chance instead when buffed
                On.RoR2.GlobalEventManager.ProcessHitEnemy += (orig, self, damageInfo, victim) =>
                {
                    orig(self, damageInfo, victim);
                    if (damageInfo.attacker && damageInfo.procCoefficient > 0f)
                    {
                        CharacterBody allyBody = damageInfo.attacker.GetComponent<CharacterBody>();
                        int countBuffs = allyBody.GetBuffCount(LanternCountBuff.LanternCounter);

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
                };
            }

            // Antler Shield #1
            if (AntlerShield.Rework.Value == 1)
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
                            int itemCount = body.inventory ? body.inventory.GetItemCount(DLC2Content.Items.NegateAttack) : 0;
                            if (itemCount > 0)
                            {
                                body.armor += (AntlerShield.Armor_Percent_Base.Value + AntlerShield.Armor_Percent_Stack.Value * (itemCount - 1)) / 100f * body.moveSpeed;
                                body.moveSpeed *= 1 + (AntlerShield.Movement_Base.Value + AntlerShield.Movement_Stack.Value * (itemCount - 1)) / 100f;
                            }
                        });
                    }
                    else
                    {
                        Logger.LogWarning(AntlerShield.StaticName + " #1 - IL Fail #1");
                    }
                };

                // Disable vanilla effect
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
                        Logger.LogWarning(AntlerShield.StaticName + " #2 - IL Fail #1");
                    }
                };
            }
        }
        private void SetUpUncommonItems()
        {
            // -- Seekers of the Storm Content -- \\

            // Sale Star #1
            if (SaleStar.Rework.Value == 1)
            {
                // Remove vanilla effect
                IL.RoR2.PurchaseInteraction.OnInteractionBegin += il =>
                {
                    var cursor = new ILCursor(il);
                    if (cursor.TryGotoNext(
                        x => x.MatchLdsfld(typeof(DLC2Content.Items), nameof(DLC2Content.Items.LowerPricedChests)),
                        x => x.MatchCallOrCallvirt<Inventory>(nameof(Inventory.GetItemCount)),
                        x => x.MatchLdcI4(out _)
                    ))
                    {
                        cursor.Index += 3;
                        cursor.EmitDelegate<Func<int, int>>(stack => int.MaxValue);
                    }
                    else
                    {
                        Logger.LogWarning(SaleStar.StaticName + " #1 - IL Fail #1");
                    }
                };

                // Implement new purchase stack
                On.RoR2.PurchaseInteraction.OnInteractionBegin += (orig, self, activator) =>
                {
                    orig(self, activator);
                    CharacterBody body = activator.GetComponent<CharacterBody>();
                    int itemCount = body.inventory ? body.inventory.GetItemCount(DLC2Content.Items.LowerPricedChests) : 0;
                    if (itemCount > 0 && self.saleStarCompatible)
                    {
                        if (self.GetComponent<ChestBehavior>())
                        {
                            self.GetComponent<ChestBehavior>().dropCount++;
                        }
                        else if (self.GetComponent<RouletteChestController>())
                        {
                            self.GetComponent<RouletteChestController>().dropCount++;
                        }

                        float percentConvert = SaleStar.IsHyperbolic.Value ? Util.ConvertAmplificationPercentageIntoReductionPercentage(SaleStar.Consume_Stack.Value * (itemCount - 1)) : SaleStar.Consume_Stack.Value * (itemCount - 1);

                        if (Util.CheckRoll(SaleStar.Consume_Base.Value - percentConvert, body.master))
                        {
                            body.inventory.RemoveItem(DLC2Content.Items.LowerPricedChests, itemCount);
                            body.inventory.GiveItem(DLC2Content.Items.LowerPricedChestsConsumed, itemCount);
                            CharacterMasterNotificationQueue.SendTransformNotification(body.master, DLC2Content.Items.LowerPricedChests.itemIndex, DLC2Content.Items.LowerPricedChestsConsumed.itemIndex, CharacterMasterNotificationQueue.TransformationType.SaleStarRegen);
                        }

                        Util.PlaySound("Play_item_proc_lowerPricedChest", self.gameObject);
                    }
                };
            }

            // Unstable Transmitter #1
            if (UnstableTransmitter.Rework.Value == 1)
            {
                // Bleed area effect, intangible skill state
                On.RoR2.CharacterBody.RpcTeleportCharacterToSafety += (orig, self) =>
                {
                    if (!self.hasEffectiveAuthority) return;
                    self.hasTeleported = true;
                    int itemCount = self.inventory.GetItemCount(DLC2Content.Items.TeleportOnLowHealth);
                    EntityStateMachine.FindByCustomName(self.gameObject, "Body").SetNextState(new IntangibleSkillState());
                    List<HurtBox> list = HG.CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
                    SphereSearch hitBox = new SphereSearch();
                    hitBox.mask = LayerIndex.entityPrecise.mask;
                    hitBox.origin = self.transform.position;
                    hitBox.radius = UnstableTransmitter.Range.Value;
                    hitBox.queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;
                    hitBox.RefreshCandidates();
                    hitBox.FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(self.teamComponent.teamIndex));
                    hitBox.OrderCandidatesByDistance();
                    hitBox.FilterCandidatesByDistinctHurtBoxEntities();
                    hitBox.GetHurtBoxes(list);
                    hitBox.ClearCandidates();
                    for (int i = 0; i < list.Count; i++)
                    {
                        HurtBox hurtBox = list[i];
                        CharacterBody victimBody = hurtBox.healthComponent.body;
                        if (hurtBox && hurtBox.healthComponent && hurtBox.healthComponent.alive)
                        {
                            InflictDotInfo bleedDot = new()
                            {
                                attackerObject = self.gameObject,
                                victimObject = victimBody.gameObject,
                                dotIndex = DotController.DotIndex.Bleed,
                                damageMultiplier = self.damage * ((UnstableTransmitter.Damage_Base.Value + UnstableTransmitter.Damage_Stack.Value * (itemCount - 1)) / 150f),
                                duration = 3.0f
                            };
                            DotController.InflictDot(ref bleedDot);
                        }
                    }
                };

                // Disabling original condition
                IL.RoR2.HealthComponent.UpdateLastHitTime += il =>
                {
                    var cursor = new ILCursor(il);

                    if (cursor.TryGotoNext(
                        x => x.MatchLdcI4(out _),
                        x => x.MatchBle(out _),
                        x => x.MatchLdarg(0),
                        x => x.MatchLdfld<HealthComponent>(nameof(HealthComponent.body)),
                        x => x.MatchLdsfld(typeof(DLC2Content.Buffs), nameof(DLC2Content.Buffs.TeleportOnLowHealth))
                    ))
                    {
                        cursor.EmitDelegate<Func<int, int>>(stack => int.MaxValue);
                    }
                    else
                    {
                        Logger.LogWarning(BolsteringLantern.StaticName + " #1 - IL Fail #1");
                    }
                };

                // New trigger functionality
                On.RoR2.HealthComponent.UpdateLastHitTime += (orig, self, damageValue, damagePosition, damageIsSilent, attacker) =>
                {
                    if (NetworkServer.active && self.body && damageValue > 0)
                    {
                        float healthPercent = (float)(self.health + self.shield) / self.combinedHealth;
                        int itemCount = self.body.inventory ? self.body.inventory.GetItemCount(DLC2Content.Items.TeleportOnLowHealth) : 0;
                        bool hasBuff = self.body ? self.body.HasBuff(DLC2Content.Buffs.TeleportOnLowHealth) : false;
                        if (healthPercent <= (UnstableTransmitter.LowHealth.Value / 100f) && itemCount > 0 && hasBuff)
                        {
                            self.body.hasTeleported = true;
                            self.body.RemoveBuff(DLC2Content.Buffs.TeleportOnLowHealth);
                            self.body.AddTimedBuff(DLC2Content.Buffs.TeleportOnLowHealthCooldown, UnstableTransmitter.Refresh.Value);
                            self.body.CallRpcTeleportCharacterToSafety();
                        }
                    }
                    orig(self, damageValue, damagePosition, damageIsSilent, attacker);
                };
            }

            // Noxious Thorn #1
            if (NoxiousThorn.Rework.Value == 1)
            {
                // Remove vanilla effect
                IL.RoR2.HealthComponent.TakeDamageProcess += il =>
                {
                    var cursor = new ILCursor(il);
                    if (cursor.TryGotoNext(
                        /*
                        x => x.MatchLdsfld(typeof(DLC2Content.Items), nameof(DLC2Content.Items.LowerPricedChests)),
                        x => x.MatchCallOrCallvirt<Inventory>(nameof(Inventory.GetItemCount)),
                        x => x.MatchLdcI4(out _)
                        */
                        //x => x.MatchLdcI4(out _),
                        x => x.MatchBle(out _),
                        x => x.MatchLdcR4(out _),
                        x => x.MatchLdcR4(out _),
                        x => x.MatchLdnull(),
                        x => x.MatchCallOrCallvirt(typeof(Util), nameof(Util.CheckRoll)),
                        x => x.MatchBrfalse(out _)
                    ))
                    {
                        cursor.EmitDelegate<Func<int, int>>(stack => int.MaxValue);
                    }
                    else
                    {
                        Logger.LogWarning(NoxiousThorn.StaticName + " #1 - IL Fail #1");
                    }
                };
            }

            // -- Risk of Rain 2 Content -- \\

            // Old War Stealthkit #1
            if (OldWarStealthKit.Rework.Value == 1)
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
                        Logger.LogWarning(OldWarStealthKit.StaticName + " #1 - IL Fail #1");
                    }
                };
            }
        }

        private void SetUpLegendaryItems()
        {
            // -- Seekers of the Storm Content -- \\

            // Growth Nectar
            
            // War Bonds #1
            if (WarBonds.Rework.Value == 1)
            {
                // Disable prior function
                IL.RoR2.CharacterBody.Start += il =>
                {
                    var cursor = new ILCursor(il);
                    var itemIndex = -1;

                    if (cursor.TryGotoNext(
                        x => x.MatchLdloc(out itemIndex),
                        x => x.MatchLdcI4(out _),
                        x => x.MatchBle(out _),
                        x => x.MatchLdarg(0),
                        x => x.MatchLdsfld(typeof(DLC2Content.Items), nameof(DLC2Content.Items.GoldOnStageStart))
                    ))
                    {
                        cursor.Emit(OpCodes.Ldloc, itemIndex);
                        cursor.EmitDelegate<Func<int, int>>(stack => { return 0; });
                        cursor.Emit(OpCodes.Stloc, itemIndex);
                    }
                    else
                    {
                        Logger.LogWarning(WarBonds.StaticName + " #1 - IL Fail #1");
                    }
                };

                // Replace money with free unlocks
                On.RoR2.CharacterBody.Start += (orig, self) =>
                {
                    orig(self);
                    int itemCount = (self.master && self.master.inventory && NetworkServer.active) ? self.master.inventory.GetItemCount(DLC2Content.Items.GoldOnStageStart) : 0;
                    if (itemCount > 0) self.SetBuffCount(DLC2Content.Buffs.FreeUnlocks.buffIndex, WarBonds.Purchase_Base.Value + WarBonds.Purchase_Stack.Value * (itemCount - 1));
                };

                // Experience gain on gold purchase
                IL.RoR2.PurchaseInteraction.OnInteractionBegin += il =>
                {
                    var cursor = new ILCursor(il);
                    var costIndex = -1;

                    if (cursor.TryGotoNext(
                        x => x.MatchLdarg(0),
                        x => x.MatchLdfld<PurchaseInteraction>(nameof(PurchaseInteraction.cost)),
                        x => x.MatchStloc(out costIndex)
                    )) { }

                    if (cursor.TryGotoNext(
                        x => x.MatchLdarg(0)
                    ))
                    {
                        cursor.Emit(OpCodes.Ldarg_0);
                        cursor.Emit(OpCodes.Ldarg_1);
                        cursor.EmitDelegate<Action<PurchaseInteraction, Interactor>>((interact, activator) =>
                        {
                            CharacterBody body = activator.GetComponent<CharacterBody>();
                            int warBondCount = body.inventory ? body.inventory.GetItemCount(DLC2Content.Items.GoldOnStageStart) : 0;
                            if (warBondCount > 0 && interact.costType == CostTypeIndex.Money) ExperienceManager.instance.AwardExperience(interact.transform.position, body, (ulong)(interact.cost * (WarBonds.Experience_Percent_Base.Value + WarBonds.Experience_Percent_Stack.Value * (warBondCount - 1)) / 100f));
                            if (body.HasBuff(DLC2Content.Buffs.FreeUnlocks))
                            {
                                Util.PlaySound("Play_item_proc_goldOnStageStart", body.gameObject);
                                EffectManager.SpawnEffect(HealthComponent.AssetReferences.gainCoinsImpactEffectPrefab, new EffectData
                                {
                                    origin = body.transform.position
                                }, false);
                            }
                        });
                    }
                };
            }

            // Growth Nectar #1
            if (GrowthNectar.Rework.Value == 1)
            {
                // Give buff when no equip or not on cooldown
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
                        else if ((nectarCount <= 0 || !nonEquip) && hasBuff)
                        {
                            equip.characterBody.RemoveBuff(DLC2Content.Buffs.BoostAllStatsBuff);
                        }
                    }
                };

                // Disable duration
                On.RoR2.CharacterBody.UpdateBoostAllStatsTimer += (orig, self, timer) => { return; };

                // Disable base trigger
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
                        Logger.LogWarning(GrowthNectar.StaticName + " #1 - IL Fail #1");
                    }
                };

                // Replacing with efficient stat increase
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
                        // Reprogram later
                        cursor.RemoveRange(78);
                        cursor.Emit(OpCodes.Ldarg_0);
                        cursor.Emit(OpCodes.Ldloc, numCountIndex);
                        cursor.EmitDelegate<Action<CharacterBody, int>>((body, nectarCount) =>
                        {
                            EquipmentSlot equip = body.equipmentSlot;
                            float itemMultiplier = (float) GrowthNectar.Stat_Base.Value + GrowthNectar.Stat_Stack.Value * (nectarCount - 1);
                            float chargeMultiplier = (float)Math.Min(GrowthNectar.Charge_Stat_Increase.Value * (equip.maxStock - 1), GrowthNectar.Charge_Cap_Base.Value + GrowthNectar.Charge_Cap_Stack.Value * (nectarCount - 1));
                            float multiplier = (itemMultiplier + chargeMultiplier) / 100f;

                            body.maxHealth += body.maxHealth * multiplier;
                            body.moveSpeed += body.moveSpeed * multiplier;
                            body.damage += body.damage * multiplier;
                            body.attackSpeed += body.attackSpeed * multiplier;
                            body.crit += body.crit * multiplier;
                            body.regen += body.regen * multiplier;
                        });
                    }
                    else
                    {
                        Logger.LogWarning(GrowthNectar.StaticName + " #1 - IL Fail #2");
                    }
                };
            }

            // -- Risk of Rain 2 Content -- \\

            // Ben's Raincoat #1
            if (BensRaincoat.Rework.Value == 1)
            {
                BuffDef disableSkill = Addressables.LoadAsset<BuffDef>("RoR2/DLC2/bdDisableAllSkills.asset").WaitForCompletion();
                disableSkill.isDebuff = true;
            }
        }
    }
}