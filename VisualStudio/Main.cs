using BepInEx;
using R2API;
using SeekerItems;

[assembly: HG.Reflection.SearchableAttribute.OptIn]
namespace SeekingItemReworks
{
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency(RecalculateStatsAPI.PluginGUID)]
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(PrefabAPI.PluginGUID)]
    [BepInDependency(ColorsAPI.PluginGUID)]

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

            MainConfig.SetUp(this);
            if (MainConfig.EnableLogs.Value) Log.Init(Logger);
            ItemInfo.SetUp();

            SetUpCommon();
            SetUpUncommon();
            //SetUpUncommonItems();
            //SetUpLegendaryItems();
        }
        private void SetUpCommon()
        {
            // -- Seekers of the Storm Content -- \\
            WarpedEchoBehavior.Init();
            ChronicExpansionBehavior.Init();
            KnockbackFinBehavior.Init();
            BolsteringLanternBehavior.Init();
            AntlerShieldBehavior.Init();
        }
        private void SetUpUncommon()
        {
            ChanceDollBehavior.Init();
            SaleStarBehavior.Init();
        }
    }
}

/*

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