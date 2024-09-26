using MonoMod.Cil;
using Mono.Cecil.Cil;
using RoR2;
using System;
using BepInEx.Configuration;
using UnityEngine.AddressableAssets;
using UnityEngine;
using static SeekingItemReworks.ColorCode;
using R2API;

namespace SeekerItems
{
    internal class KnockbackFin : ItemTemplate
    {
        public KnockbackFin(int descType)
        {
            ItemInternal = "KNOCKBACKHITENEMIES";
            if (descType == 1)
            {
                ItemInfo = "Chance on hit to knock enemies into the air.\nDeal bonus damage to airborne enemies.";
                ItemDesc = string.Format(
                    "{0}% ".Style(FontColor.cIsUtility) + "(+{1}% per stack) ".Style(FontColor.cStack) + "chance on hit to " + "knock enemies into the air".Style(FontColor.cIsUtility) + ". Deal " + "+{2}% ".Style(FontColor.cIsDamage) + "(+{3}% per stack) ".Style(FontColor.cStack) + "damage to " + "airborne ".Style(FontColor.cIsUtility) + "enemies.",
                    Math.Round(Chance_Base.Value, roundVal), Math.Round(Chance_Stack.Value, roundVal),
                    Math.Round(Damage_Base.Value, roundVal), Math.Round(Damage_Stack.Value, roundVal)
                );
            }
            else if (descType == -1)
            {
                ItemInfo = "Chance on hit to knock up enemies.\nDeal bonus damage to airborne enemies.";
                ItemDesc = string.Format(
                    "{0}% ".Style(FontColor.cIsUtility) + "(+{1}% per stack) ".Style(FontColor.cStack) + "chance on hit to " + "knock up enemies".Style(FontColor.cIsUtility) + ". Deal " + "+{2}% ".Style(FontColor.cIsDamage) + "(+{3}% per stack) ".Style(FontColor.cStack) + "damage to " + "airborne ".Style(FontColor.cIsUtility) + "enemies.",
                    Math.Round(Chance_Base.Value, roundVal), Math.Round(Chance_Stack.Value, roundVal),
                    Math.Round(Damage_Base.Value, roundVal), Math.Round(Damage_Stack.Value, roundVal)
                );
            }
        }

        public static string StaticName = "Knockback Fin";

        public static ConfigEntry<int> Rework;
        public static ConfigEntry<bool> DamageColor;
        public static ConfigEntry<bool> IsHyperbolic;

        public static ConfigEntry<float> Chance_Base;
        public static ConfigEntry<float> Chance_Stack;
        public static ConfigEntry<float> Damage_Base;
        public static ConfigEntry<float> Damage_Stack;
    }
    public static class KnockbackFinBehavior
    {
        private static EffectDef airborneVFX;
        private static DamageColorIndex airborneDamage;
        public static void Init()
        {
            if (KnockbackFin.Rework.Value == 1)
            {
                ExposeModifyEffect();
                IL.RoR2.HealthComponent.TakeDamageProcess += DamageAirborne;
                IL.RoR2.GlobalEventManager.ProcessHitEnemy += RemoveEffect;
                On.RoR2.GlobalEventManager.ProcessHitEnemy += KnockEffect;
            }
        }

        private static void ExposeModifyEffect()
        {
            var tempVFX = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercExposeConsumeEffect.prefab").WaitForCompletion().InstantiateClone("KnockbackFinAirborne", true);
            var effectComponent = tempVFX.GetComponent<EffectComponent>();

            effectComponent.soundName = "";

            airborneVFX = new()
            {
                prefab = tempVFX,
                prefabName = "KnockbackFinAirborne",
                prefabEffectComponent = effectComponent
            };

            if (KnockbackFin.DamageColor.Value)
            {
                ContentAddition.AddEffect(airborneVFX.prefab);
                airborneDamage = ColorsAPI.RegisterDamageColor(new Color(0.3f, 0.65f, 1));
            }
        }

        private static void RemoveEffect(ILContext il)
        {
            var cursor = new ILCursor(il);

            if (cursor.TryGotoNext(
                x => x.MatchLdarg(2),
                x => x.MatchCallOrCallvirt(typeof(UnityEngine.GameObject), nameof(UnityEngine.GameObject.GetComponent)),
                x => x.MatchStloc(out _),
                x => x.MatchLdarg(2),
                x => x.MatchCallOrCallvirt(typeof(UnityEngine.GameObject), nameof(UnityEngine.GameObject.GetComponent))
            ))
            {
                var previousIndex = cursor.Index;

                if (cursor.TryGotoNext(
                    x => x.MatchPop(),
                    x => x.MatchLdloc(out _),
                    x => x.MatchLdsfld(typeof(DLC2Content.Items), nameof(DLC2Content.Items.StunAndPierce))
                ))
                {
                    var skipLabel = cursor.MarkLabel();
                    cursor.Goto(previousIndex);
                    cursor.Emit(OpCodes.Br, skipLabel.Target);
                }
            }
            else
            {
                Log.Warning(KnockbackFin.StaticName + " #1 - IL Fail #1");
            }
        }
        private static void KnockEffect(On.RoR2.GlobalEventManager.orig_ProcessHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);

            if (damageInfo.attacker && victim && damageInfo.procCoefficient > 0)
            {
                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                CharacterBody victimBody = victim.GetComponent<CharacterBody>();

                if (attackerBody && victimBody)
                {
                    int itemCount = attackerBody.inventory ? attackerBody.inventory.GetItemCount(DLC2Content.Items.KnockBackHitEnemies) : 0;
                    if (itemCount > 0)
                    {
                        CharacterMotor victimMotor = victim.GetComponent<CharacterMotor>(); victim.GetComponent<RigidbodyMotor>();
                        float procChance = (float)(KnockbackFin.Chance_Base.Value + KnockbackFin.Chance_Stack.Value * (itemCount - 1)) * damageInfo.procCoefficient;

                        if (KnockbackFin.IsHyperbolic.Value) procChance = Util.ConvertAmplificationPercentageIntoReductionPercentage(procChance);
                        if (victimMotor && victimMotor.isGrounded && !victimBody.isChampion && (victimBody.bodyFlags & CharacterBody.BodyFlags.IgnoreFallDamage) == CharacterBody.BodyFlags.None && Util.CheckRoll(procChance, 0f, null))
                        {
                            float scale = victimBody.hullClassification == HullClassification.Human ? 1f :
                                victimBody.hullClassification == HullClassification.Golem ? 5f :
                                victimBody.hullClassification == HullClassification.BeetleQueen ? 10f : 1f;

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
            }
        }
        private static void DamageAirborne(ILContext il)
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
                Log.Warning(KnockbackFin.StaticName + " #1 - IL Fail #2");
            }

            if (cursor.TryGotoNext(
                x => x.MatchLdloc(out _),
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<HealthComponent>("get_fullCombinedHealth")
            ))
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.Emit(OpCodes.Ldloc, damageIndex);

                cursor.EmitDelegate<Func<HealthComponent, DamageInfo, float, float>>((healthComponent, damageInfo, damage) =>
                {
                    float damageMod = damage;
                    CharacterBody characterBody = damageInfo.attacker ? damageInfo.attacker.GetComponent<CharacterBody>() : null;
                    int itemCount = (characterBody && characterBody.inventory) ? characterBody.inventory.GetItemCount(DLC2Content.Items.KnockBackHitEnemies) : 0;
                    if (itemCount > 0)
                    {
                        if (healthComponent.body.isFlying || (healthComponent.body.characterMotor != null && !healthComponent.body.characterMotor.isGrounded))
                        {
                            if (KnockbackFin.DamageColor.Value) damageInfo.damageColorIndex = airborneDamage;
                            EffectManager.SimpleImpactEffect(airborneVFX.prefab, damageInfo.position, Vector3.up, true);
                            damageMod *= (float)1f + (KnockbackFin.Damage_Base.Value + KnockbackFin.Damage_Stack.Value * (itemCount - 1)) / 100f;
                        }
                    }
                    return damageMod;
                });

                cursor.Emit(OpCodes.Stloc, damageIndex);
            }
            else
            {
                Log.Warning(KnockbackFin.StaticName + " #1 - IL Fail #3");
            }
        }
    }
}
