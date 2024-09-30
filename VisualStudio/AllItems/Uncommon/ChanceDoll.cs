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

using static SeekingItemReworks.ColorCode;
using System.Collections.Generic;

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
    }
    public class ShrineFailCount : NetworkBehaviour { public int FailCount; }
}
