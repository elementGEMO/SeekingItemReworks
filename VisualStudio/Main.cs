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
using SeekerItems;
using BepInEx.Configuration;

[assembly: HG.Reflection.SearchableAttribute.OptIn]
namespace SeekingItemReworks
{
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "noodlegemo";
        public const string PluginName = "SeekingItemReworks";
        public const string PluginVersion = "1.5.0";
        public void Awake()
        {
            //On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (s, u, t) => { }; - Used for Solo Multiplayer testing

            Log.Init(Logger);
            MainConfig.SetUp(this);
            ItemInfo.SetUp();

            SetUpCommon();
            //SetUpCommonItems();
            //SetUpUncommonItems();
            //SetUpLegendaryItems();
        }

        public void SetUpCommon()
        {
            // -- Seekers of the Storm Content -- \\
            WarpedEchoBehavior.Init();
            ChronicExpansionBehavior.Init();
            KnockbackFinBehavior.Init();
        }
    }
}

/*

            // -- Seekers of the Storm Content -- \\

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

            // Chance Doll #1
            /*
            if (true)
            {
                PickupDropTable upgradeTable = new();
                upgradeTable.Add(new List<PickupIndex>
                {

                });
            }*/

// Chance Doll #0
/*
if (ChanceDoll.Rework.Value > 0)
{
    IL.RoR2.ShrineChanceBehavior.AddShrineStack += il =>
    {
        var cursor = new ILCursor(il);

        if (cursor.TryGotoNext(
            x => x.MatchRet(),
            x => x.MatchLdarg(0),
            x => x.MatchLdfld<ShrineChanceBehavior>(nameof(ShrineChanceBehavior.dropTable)),
            x => x.MatchCallOrCallvirt(typeof(UnityEngine.Object), "op_Implicit"),
            x => x.MatchBrfalse(out _)
        ))
        {
            Logger.LogDebug(cursor.Index);
            var skipLabel = cursor.DefineLabel();
            cursor.Emit(OpCodes.Br, skipLabel);

            if (cursor.TryGotoNext(
                x => x.MatchStloc(out _),
                x => x.MatchLdloc(out _),
                x => x.MatchLdsfld<PickupIndex>(nameof(PickupIndex.none)),
                x => x.MatchCallOrCallvirt<PickupIndex>("op_Equality")
            ))
            {
                //cursor.Index--;
                Logger.LogDebug(cursor.Index);
                cursor.MarkLabel(skipLabel);
            }
        }
        else
        {
            Logger.LogWarning(ChanceDoll.StaticName + " #0 - IL Fail #1");
        }
    };
}
if (ChanceDoll.Rework.Value == 1)
{
    IL.RoR2.ShrineChanceBehavior.AddShrineStack += il =>
    {
        var cursor = new ILCursor(il);
        var pickupIndex = -1;

        if (cursor.TryGotoNext(
            x => x.MatchStloc(out pickupIndex)
        )) { }
        else
        {
            Logger.LogWarning(ChanceDoll.StaticName + " #1 - IL Fail #1");
        }

        //Logger.LogDebug("Index: " + pickupIndex);

        if (cursor.TryGotoNext(
            x => x.MatchLdloc(out _),
            x => x.MatchLdsfld<PickupIndex>(nameof(PickupIndex.none)),
            x => x.MatchCallOrCallvirt<PickupIndex>("op_Equality")
        ))
        {
            //Logger.LogDebug("1");
            cursor.Emit(OpCodes.Ldarg_0);
            //Logger.LogDebug("2");
            cursor.Emit(OpCodes.Ldloc, pickupIndex);
            //Logger.LogDebug("3");

            cursor.EmitDelegate<Func<ShrineChanceBehavior, PickupIndex, PickupIndex>>((self, pickupInd) =>
            {
                PickupIndex emptyTier = PickupIndex.none;
                PickupIndex commonTier = self.rng.NextElementUniform<PickupIndex>(Run.instance.availableTier1DropList);
                PickupIndex uncommonTier = self.rng.NextElementUniform<PickupIndex>(Run.instance.availableTier2DropList);
                PickupIndex legendaryTier = self.rng.NextElementUniform<PickupIndex>(Run.instance.availableTier3DropList);
                PickupIndex equipTier = self.rng.NextElementUniform<PickupIndex>(Run.instance.availableEquipmentDropList);
                WeightedSelection<PickupIndex> weightedSelection = new WeightedSelection<PickupIndex>(8);
                weightedSelection.AddChoice(emptyTier, self.failureWeight);
                weightedSelection.AddChoice(commonTier, self.tier1Weight);
                weightedSelection.AddChoice(uncommonTier, self.tier2Weight);
                weightedSelection.AddChoice(legendaryTier, self.tier3Weight);
                weightedSelection.AddChoice(equipTier, self.equipmentWeight);

                self.chanceDollWin = false;
                Logger.LogDebug("Apparently override");

                return weightedSelection.Evaluate(self.rng.nextNormalizedFloat);
            });
            //Logger.LogDebug("4");
            cursor.Emit(OpCodes.Stloc, pickupIndex);
            //Logger.LogDebug("5");
        }
        else
        {
            Logger.LogWarning(ChanceDoll.StaticName + " #1 - IL Fail #1");
        }
    };
}
*/

/*
/*
if (ChanceDoll.Rework.Value > 0)
{
    // Invalidate Base Effect
    IL.RoR2.ShrineChanceBehavior.AddShrineStack += il =>
    {
        var cursor = new ILCursor(il);

        if (cursor.TryGotoNext(
            x => x.MatchLdcI4(out _),
            x => x.MatchBle(out _),
            x => x.MatchLdloc(out _),
            x => x.MatchLdsfld(typeof(DLC2Content.Items), nameof(DLC2Content.Items.ExtraShrineItem))
        ))
        {
            cursor.EmitDelegate<Func<int, int>>(self => int.MaxValue);
        }
        else
        {
            Logger.LogWarning(ChanceDoll.StaticName + " #0 - IL Fail #1");
        }
    };
}
if (ChanceDoll.Rework.Value == 1)
{
    IL.RoR2.ShrineChanceBehavior.AddShrineStack += il =>
    {
        var cursor = new ILCursor(il);
        var pickupIndex = -1;

        if (cursor.TryGotoNext(
            x => x.MatchStloc(out pickupIndex)
        )) { }
        else
        {
            Logger.LogWarning(ChanceDoll.StaticName + " #1 - IL Fail #1");
        }

        if (cursor.TryGotoNext(
            x => x.MatchBr(out _),
            x => x.MatchLdarg(0),
            x => x.MatchLdcI4(out _),
            x => x.MatchStfld<ShrineChanceBehavior>(nameof(ShrineChanceBehavior.chanceDollWin))
        ))
        {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<ShrineChanceBehavior>>(self =>
            {
                Logger.LogDebug("Activated");
            });
        }
    };
}
*/
/*
if (ChanceDoll.Rework.Value == 1)
            {
                // Add a new lang token, for English
                LanguageAPI.Add("SHRINE_CHANCE_DOLL_2P", "<style=cShrine>Your Chance Doll upgrades the shrine, granting a reward!</color>");
                LanguageAPI.Add("SHRINE_CHANCE_DOLL", "<style=cShrine>{0}'s Chance Doll upgrades the shrine, granting a reward!</color>");

                // Add Chance Shrine component
                ChanceDollSetup.Awake();

                // Replace Shrine functionality
                On.RoR2.ShrineChanceBehavior.AddShrineStack += (orig, self, activator) =>
                {
                    if (!NetworkServer.active) return;
                    if (!activator.GetComponent<HealthComponent>()) return;

                    CharacterBody characterBody = activator.GetComponent<CharacterBody>();
                    PickupIndex itemIndex = PickupIndex.none;

                    if (!characterBody.inventory) return;

                    self.chanceDollWin = false;

                    if (self.dropTable)
                    {
                        ChanceDollBehavior counter = self.gameObject.GetComponent<ChanceDollBehavior>();
                        int itemCount = characterBody.inventory.GetItemCount(DLC2Content.Items.ExtraShrineItem);

                        if (self.rng.nextNormalizedFloat > self.failureChance)
                        {
                            itemIndex = self.dropTable.GenerateDrop(self.rng);
                            float rollChance = (itemCount > 0) ? (ChanceDoll.Chance_Base.Value + ChanceDoll.Chance_Stack.Value * (itemCount - 1)) * counter.FailCount : 0;

                            if (Util.CheckRoll(Math.Max(rollChance, ChanceDoll.Hidden_Chance.Value), characterBody.master))
                            {
                                ItemTier minimumTier;
                                ItemTier foundTier = itemIndex.pickupDef.itemTier;
                                int tierExtraChance = (Util.CheckRoll(rollChance % 100)) ? 1 : 0;
                                double tierAmounts = Math.Round(rollChance / 100);

                                if (tierAmounts > 1 || tierAmounts + tierExtraChance > 1) minimumTier = ItemTier.Tier3;
                                else if (tierAmounts <= 1) minimumTier = ItemTier.Tier2;
                                else minimumTier = ItemTier.Tier1;

                                List<PickupIndex> commonList = new(Run.instance.availableTier1DropList);
                                List<PickupIndex> uncommonList = new(Run.instance.availableTier2DropList);
                                List<PickupIndex> legendaryList = new(Run.instance.availableTier3DropList);
                                List<PickupIndex> selectList;

                                if (minimumTier == ItemTier.Tier1) selectList = commonList;
                                if (minimumTier == ItemTier.Tier2) selectList = uncommonList;
                                else selectList = legendaryList;

                                Util.ShuffleList<PickupIndex>(selectList);
                                itemIndex = selectList[0];

                                self.chanceDollWin = true;
                            }
                        }
                        else
                        {
                            counter.FailCount++;

                            if (itemCount > 0)
                            {
                                EffectManager.SpawnEffect(self.effectPrefabShrineRewardNormal, new EffectData
                                {
                                    origin = self.gameObject.transform.position,
                                    rotation = Quaternion.identity,
                                    scale = 3f,
                                    color = self.colorShrineRewardJackpot
                                }, true);
                            }
                        }
                    }

                    string baseToken;
                    if (itemIndex == PickupIndex.none) baseToken = "SHRINE_CHANCE_FAIL_MESSAGE";
                    else
                    {
                        if (self.chanceDollWin) baseToken = "SHRINE_CHANCE_DOLL";
                        else baseToken = "SHRINE_CHANCE_SUCCESS_MESSAGE";
                        self.successfulPurchaseCount++;
                        PickupDropletController.CreatePickupDroplet(itemIndex, self.dropletOrigin.position, self.dropletOrigin.forward * 20f);
                    }
                    Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                    {
                        subjectAsCharacterBody = characterBody,
                        baseToken = baseToken
                    });
                    self.waitingForRefresh = true;
                    self.refreshTimer = 2f;
                    if (self.chanceDollWin)
                    {
                        EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ExtraStatsOnLevelUpEffect"), new EffectData
                        {
                            origin = self.gameObject.transform.position,
                            rotation = Quaternion.identity,
                            scale = 3f,
                            color = self.colorShrineRewardJackpot
                        }, true);
                    }
                    else
                    {
                        EffectManager.SpawnEffect(self.effectPrefabShrineRewardNormal, new EffectData
                        {
                            origin = self.gameObject.transform.position,
                            rotation = Quaternion.identity,
                            scale = 1.5f,
                            color = self.colorShrineRewardNormal
                        }, true);
                    }
                    if (self.successfulPurchaseCount >= self.maxPurchaseCount)
                    {
                        self.symbolTransform.gameObject.SetActive(false);
                        self.CallRpcSetPingable(false);
                    }
                };
            }
            if (ChanceDoll.Rework.Value == 2)
            {
                // Remove Chance Doll's effect
                On.RoR2.ShrineChanceBehavior.AddShrineStack += (orig, self, activator) =>
                {
                    if (!NetworkServer.active) return;
                    if (!activator.GetComponent<HealthComponent>()) return;

                    CharacterBody characterBody = activator.GetComponent<CharacterBody>();
                    PickupIndex itemIndex = PickupIndex.none;
                    string baseToken;

                    if (!characterBody.inventory) return;
                    if (self.dropTable && self.rng.nextNormalizedFloat > self.failureChance) itemIndex = self.dropTable.GenerateDrop(self.rng);

                    if (itemIndex == PickupIndex.none) baseToken = "SHRINE_CHANCE_FAIL_MESSAGE";
                    else
                    {
                        baseToken = "SHRINE_CHANCE_SUCCESS_MESSAGE";
                        self.successfulPurchaseCount++;
                        PickupDropletController.CreatePickupDroplet(itemIndex, self.dropletOrigin.position, self.dropletOrigin.forward * 20f);
                    }

                    Chat.SendBroadcastChat(new Chat.SubjectFormatChatMessage
                    {
                        subjectAsCharacterBody = characterBody,
                        baseToken = baseToken
                    });

                    self.waitingForRefresh = true;
                    self.refreshTimer = 2f;

                    EffectManager.SpawnEffect(self.effectPrefabShrineRewardNormal, new EffectData
                    {
                        origin = self.gameObject.transform.position,
                        rotation = Quaternion.identity,
                        scale = 1.5f,
                        color = self.colorShrineRewardNormal
                    }, true);

                    if (self.successfulPurchaseCount >= self.maxPurchaseCount)
                    {
                        self.symbolTransform.gameObject.SetActive(false);
                        self.CallRpcSetPingable(false);
                    }
                };

                // Add on interact function
                On.RoR2.GlobalEventManager.OnInteractionBegin += (orig, self, interactor, interactable, interactableObject) =>
                {
                    orig(self, interactor, interactable, interactableObject);

                    PurchaseInteraction interactType = interactableObject ? interactableObject.GetComponent<PurchaseInteraction>() : null;
                    if (interactable != null && interactType)
                    {
                        CharacterBody characterBody = interactor.GetComponent<CharacterBody>();
                        int itemCount = characterBody.inventory ? characterBody.inventory.GetItemCount(DLC2Content.Items.ExtraShrineItem) : 0;

                        if (characterBody && itemCount > 0 && interactType.isShrine)
                        {
                            PlayerCharacterMasterController masterController = characterBody.master.playerCharacterMasterController;
                            KarmaDollBehavior karmaDollBehavior = masterController.gameObject.GetComponent<KarmaDollBehavior>();
                            if (!karmaDollBehavior)
                            {
                                karmaDollBehavior = masterController.gameObject.AddComponent<KarmaDollBehavior>();
                                karmaDollBehavior.Awake();
                                karmaDollBehavior.owner = characterBody;
                            }

                            karmaDollBehavior.IncreaseKarma(itemCount, characterBody, interactableObject);
                            Debug.Log("SeekingItemReworks: Activated a shrine");
                        }
                    }
                };

                // Add luck
                IL.RoR2.CharacterBody.RecalculateStats += il =>
                {
                    var cursor = new ILCursor(il);

                    if (cursor.TryGotoNext(
                        x => x.MatchLdloc(out _),
                        x => x.MatchLdloc(out _),
                        x => x.MatchConvR4(),
                        x => x.MatchLdcR4(out _),
                        x => x.MatchMul(),
                        x => x.MatchLdarg(0),
                        x => x.MatchCallOrCallvirt<CharacterBody>("get_maxHealth")
                    ))
                    {
                        cursor.Emit(OpCodes.Ldarg, 0);
                        cursor.EmitDelegate<Action<CharacterBody>>(self =>
                        {
                            Logger.LogDebug("Is updating stats...");
                            int itemCount = self.inventory ? self.inventory.GetItemCount(DLC2Content.Items.ExtraShrineItem) : 0;
                            KarmaDollBehavior karmaDollBehavior = self.master.playerCharacterMasterController.gameObject.GetComponent<KarmaDollBehavior>();
                            if (itemCount > 0 && karmaDollBehavior)
                            {
                                self.master.luck += karmaDollBehavior.karmaLuck;
                                Logger.LogDebug("Is updating luck...");
                            }
                        });
                    }
                    else
                    {
                        Logger.LogWarning(ChanceDoll.StaticName + " #2 - IL Fail #1");
                    }
                };
            }

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
*/