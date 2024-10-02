﻿using MonoMod.Cil;
using Mono.Cecil.Cil;
using BepInEx.Configuration;
using System;
using UnityEngine.Networking;
using RoR2.Items;
using RoR2.Orbs;
using RoR2;
using UnityEngine;
using System.Numerics;
using UnityEngine.AddressableAssets;
using R2API;
using System.Collections.Generic;

using static SeekingItemReworks.ColorCode;

namespace SeekerItems
{
    internal class ChanceDoll : ItemTemplate
    {
        public ChanceDoll(int descType)
        {
            ItemInternal = "EXTRASHRINEITEM";

            if (descType == 1)
            {
                ItemInfo = "Chance to upgrade the item reward on failing a Shrine.";
                ItemDesc = string.Format(
                    "Failing a Shrine increases the chance for the item reward to " + "upgrade ".Style(FontColor.cIsUtility) + "by " + "{0}% ".Style(FontColor.cIsUtility) + "(+{1}% per stack)".Style(FontColor.cStack) + ", up to a " + "Legendary ".Style(FontColor.cIsHealth) + "item.",
                    RoundVal(Chance_Base.Value), RoundVal(Chance_Stack.Value)
                );
            }
            if (descType == 2)
            {
                ItemInfo = "Funny luck infusion.";
                ItemDesc = "Hi :3";
            }
        }
        public static string StaticName = "Chance Doll";

        public static ConfigEntry<int> Rework;

        public static ConfigEntry<float> Hidden_Chance;
        public static ConfigEntry<float> Chance_Base;
        public static ConfigEntry<float> Chance_Stack;

        public static ConfigEntry<int> Karma_Required;
    }

    public static class ChanceDollBehavior
    {
        public static void Init()
        {
            if (ChanceDoll.Rework.Value == 1)
            {
                LanguageAPI.Add("SHRINE_CHANCE_DOLL_2P", "<style=cShrine>Your Chance Doll upgrades the shrine, granting a reward!</color>");
                LanguageAPI.Add("SHRINE_CHANCE_DOLL", "<style=cShrine>{0}'s Chance Doll upgrades the shrine, granting a reward!</color>");

                On.RoR2.ShrineChanceBehavior.Awake += AddCounter;
                On.RoR2.ShrineChanceBehavior.AddShrineStack += ReplaceBehavior;
            }
            else if (ChanceDoll.Rework.Value == 2)
            {
                IL.RoR2.ShrineChanceBehavior.AddShrineStack += RemoveBehavior;
                On.RoR2.GlobalEventManager.OnInteractionBegin += IncreaseKarma;
                On.RoR2.CharacterMaster.OnInventoryChanged += LuckCount;
            }
        }

        private static void AddCounter(On.RoR2.ShrineChanceBehavior.orig_Awake orig, RoR2.ShrineChanceBehavior self)
        {
            orig(self);

            ShrineFailCount shrineCount = self.gameObject.AddComponent<ShrineFailCount>();
            shrineCount.FailCount = 0;
        }
        private static void ReplaceBehavior(On.RoR2.ShrineChanceBehavior.orig_AddShrineStack orig, RoR2.ShrineChanceBehavior self, RoR2.Interactor activator)
        {
            if (!NetworkServer.active) return;
            if (!activator.GetComponent<HealthComponent>()) return;

            CharacterBody characterBody = activator.GetComponent<CharacterBody>();
            PickupIndex itemIndex = PickupIndex.none;

            if (!characterBody.inventory) return;

            self.chanceDollWin = false;

            if (self.dropTable)
            {
                ShrineFailCount counter = self.gameObject.GetComponent<ShrineFailCount>();
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
                            rotation = UnityEngine.Quaternion.identity,
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
                    rotation = UnityEngine.Quaternion.identity,
                    scale = 3f,
                    color = self.colorShrineRewardJackpot
                }, true);
            }
            else
            {
                EffectManager.SpawnEffect(self.effectPrefabShrineRewardNormal, new EffectData
                {
                    origin = self.gameObject.transform.position,
                    rotation = UnityEngine.Quaternion.identity,
                    scale = 1.5f,
                    color = self.colorShrineRewardNormal
                }, true);
            }
            if (self.successfulPurchaseCount >= self.maxPurchaseCount)
            {
                self.symbolTransform.gameObject.SetActive(false);
                self.CallRpcSetPingable(false);
            }
        }
        private static void RemoveBehavior(ILContext il)
        {
            var cursor = new ILCursor(il);

            if (cursor.TryGotoNext(
                x => x.MatchBle(out _),
                x => x.MatchLdloc(out _),
                x => x.MatchLdsfld(typeof(DLC2Content.Items), nameof(DLC2Content.Items.ExtraShrineItem))
            ))
            {
                cursor.EmitDelegate<Func<int, int>>(self => int.MaxValue);
            }
            else
            {
                Log.Warning(ChanceDoll.StaticName + " #2 - IL Fail #1");
            }
        }
        private static void IncreaseKarma(On.RoR2.GlobalEventManager.orig_OnInteractionBegin orig, GlobalEventManager self, Interactor interactor, IInteractable interactable, GameObject interactObject)
        {
            orig(self, interactor, interactable, interactObject);

            PurchaseInteraction interactType = interactObject ? interactObject.GetComponent<PurchaseInteraction>() : null;
            if (interactable != null && interactType)
            {
                CharacterBody characterBody = interactor.GetComponent<CharacterBody>();
                int itemCount = characterBody.inventory ? characterBody.inventory.GetItemCount(DLC2Content.Items.ExtraShrineItem) : 0;

                if (characterBody && itemCount > 0 && interactType.isShrine)
                {
                    PlayerCharacterMasterController masterController = characterBody.master.playerCharacterMasterController;
                    KarmaDollBehavior karmaBehavior = masterController.GetComponent<KarmaDollBehavior>();
                    if (!karmaBehavior) karmaBehavior = masterController.gameObject.AddComponent<KarmaDollBehavior>();

                    karmaBehavior.KarmaOrb(characterBody, interactObject);
                }
                else if (characterBody && itemCount <= 0)
                {
                    PlayerCharacterMasterController masterController = characterBody.master.playerCharacterMasterController;
                    KarmaDollBehavior karmaBehavior = masterController.GetComponent<KarmaDollBehavior>();
                    if (karmaBehavior) karmaBehavior.UpdateLuck();
                }
            }
        }
        private static void LuckCount(On.RoR2.CharacterMaster.orig_OnInventoryChanged orig, CharacterMaster self)
        {
            orig(self);

            if (self.playerCharacterMasterController)
            {
                KarmaDollBehavior karmaBehavior = self.playerCharacterMasterController.GetComponent<KarmaDollBehavior>();
                if (karmaBehavior) self.luck += karmaBehavior.luckStat;
            }
        }
    }
    public class ShrineFailCount : NetworkBehaviour { public int FailCount; }
    public class KarmaDollBehavior : MonoBehaviour
    {
        public CharacterBody owner;
        public int luckStat;
        private int karmaCount;
        private int ItemCount { get => (owner && owner.inventory) ? owner.inventory.GetItemCount(DLC2Content.Items.ExtraShrineItem) : 0; }

        private void Awake() => (luckStat, karmaCount, owner) = (0, 0, GetComponent<CharacterBody>());
        public void KarmaOrb(CharacterBody body, GameObject interactableObject)
        {
            if (!body && !interactableObject) return;
            if (luckStat < (ItemCount))
            {
                KarmaDollOrb karmaOrb = new()
                {
                    origin = interactableObject.transform.position,
                    target = body.mainHurtBox,
                    arrivalTime = 5f, // Add Config Value Here?

                };
                OrbManager.instance.AddOrb(karmaOrb);
            }
        }
        public void IncreaseKarma()
        {
            karmaCount += 1; // Add Config Value Here
            if (karmaCount >= ChanceDoll.Karma_Required.Value)
            {
                karmaCount = 0;
                UpdateLuck(1); // Add Config Value Here
                //owner.GetComponent<CharacterMaster>().OnInventoryChanged();
                owner.master.OnInventoryChanged();
            }
        }
        public void UpdateLuck(int modifier = 0) => luckStat = Math.Min(luckStat + modifier, ItemCount);
    }
    public class KarmaDollOrb : Orb
    {
        [InitDuringStartup]
        private static void Init()
        {
            orbEffect = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Effects/OrbEffects/InfusionOrbEffect"), "KarmaDollOrbEffect", true);

            Color yellowOrb = new(0.92f / 4f, 0.78f / 4f, 0.42f / 4f);
            Color yellowLightOrb = new(0.98f / 2f, 0.86f / 2f, 0.51f / 2f);

            ParticleSystemRenderer mainOrb = orbEffect.transform.Find("VFX").Find("Core").GetComponent<ParticleSystemRenderer>();
            if (mainOrb)
            {
                var newMaterial = new Material(mainOrb.sharedMaterial);
                newMaterial.DisableKeyword("VERTEXCOLOR");
                newMaterial.SetColor("_TintColor", yellowLightOrb);
                newMaterial.SetColor("_Color", yellowLightOrb);
                mainOrb.sharedMaterial = newMaterial;
            }

            ParticleSystem orbGlow = orbEffect.transform.Find("VFX").Find("PulseGlow").GetComponent<ParticleSystem>();
            if (orbGlow)
            {
                var mainGlow = orbGlow.main;
                mainGlow.startColor = yellowOrb;
                mainGlow.startSizeMultiplier = 0.01f;
            }

            TrailRenderer trail = orbEffect.transform.Find("TrailParent").Find("Trail").GetComponent<TrailRenderer>();
            if (trail)
            {
                var newMaterial = new Material(trail.sharedMaterial);
                newMaterial.DisableKeyword("VERTEXCOLOR");
                newMaterial.SetColor("_TintColor", yellowOrb);
                newMaterial.SetColor("_Color", yellowOrb);
                newMaterial.SetTexture("_RemapTex", mainOrb.sharedMaterial.GetTexture("_RemapTex"));
                trail.sharedMaterial = newMaterial;
            }

            var gameObj = orbEffect.GetComponent<AkGameObj>();
            if (gameObj) UnityEngine.Object.Destroy(gameObj);

            new EffectDef()
            {
                prefab = orbEffect,
                prefabName = "KarmaDollOrbEffect",
                prefabEffectComponent = orbEffect.GetComponent<EffectComponent>()
            };

            ContentAddition.AddEffect(orbEffect);
        }
        public override void Begin()
        {
            duration = distanceToTarget / arrivalTime;
            EffectData effectData = new()
            {
                origin = origin,
                genericFloat = duration
            };
            effectData.SetHurtBoxReference(target);
            EffectManager.SpawnEffect(orbEffect, effectData, true);
            HurtBox hurtBox = target.GetComponent<HurtBox>();
            CharacterBody characterBody = hurtBox?.healthComponent.GetComponent<CharacterBody>();
            if (characterBody) karmaBehavior = characterBody.master.playerCharacterMasterController.GetComponent<KarmaDollBehavior>();
        }
        public override void OnArrival()
        {
            if (karmaBehavior) karmaBehavior.IncreaseKarma();
        }

        private KarmaDollBehavior karmaBehavior;
        private static GameObject orbEffect;
    }
}
