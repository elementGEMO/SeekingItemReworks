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
            // -- Seekers of the Storm Content -- \\
            ChanceDollBehavior.Init();
            SaleStarBehavior.Init();
            UnstableTransmitterBehavior.Init();
            NoxiousThornBehavior.Init();

            // -- Risk of Rain 2 Content -- \\
            OldWarStealthkitBehavior.Init();
        }
    }
}

/*

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