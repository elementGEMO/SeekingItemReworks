using MonoMod.Cil;
using Mono.Cecil.Cil;
using R2API;
using RoR2;
using System;
using BepInEx.Configuration;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using UnityEngine;
using static SeekingItemReworks.ColorCode;

namespace SeekerItems
{
    internal class WarpedEcho : ItemTemplate
    {
        public WarpedEcho(int descType)
        {
            ItemInternal = "DELAYEDDAMAGE";
            if (descType == 1)
            {
                ItemInfo = "Half of incoming damage is delayed.";
                ItemDesc = string.Format(
                    "Incoming damage ".Style(FontColor.cIsDamage) + "is split " + "50%".Style(FontColor.cIsDamage) + ", with the other " + "50% ".Style(FontColor.cIsDamage) + "delayed for " + "{0}s ".Style(FontColor.cIsDamage) + "(+{1} instances per stack)".Style(FontColor.cStack) + ". Recharges one instance after delayed damage.",
                    RoundVal(Delay_Base.Value), Instance_Stack.Value
                );
            }
        }

        public static string StaticName = "Warped Echo";

        public static ConfigEntry<int> Rework;

        public static ConfigEntry<float> Delay_Base;
        public static ConfigEntry<int> Instance_Stack;
    }

    public static class WarpedEchoBehavior
    {
        private static GameObject delayedVFX;
        public static void Init()
        {
            if (WarpedEcho.Rework.Value == 1)
            {
                SpeedModifyEffect();
                if (delayedVFX != null) IL.RoR2.HealthComponent.TakeDamageProcess += RemoveOriginalVFX;
                if (delayedVFX != null) On.RoR2.DelayedDamageEffectUpdater.SpawnDelayedDamageEffect += RemoveBehaviorVFX;
                IL.RoR2.CharacterBody.SecondHalfOfDelayedDamage += SetDelayTimer;
                IL.RoR2.CharacterBody.UpdateSecondHalfOfDamage += ImmediateRefresh;
                On.RoR2.CharacterBody.UpdateDelayedDamage += BuffLogic;
            }
        }

        private static void SpeedModifyEffect()
        {
            delayedVFX = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Items/DelayedDamage/DelayedDamageIndicator.prefab").WaitForCompletion().InstantiateClone("SlowDelayedDamageIndicator", true);

            var baseHolder = delayedVFX.gameObject.transform.Find("VFX");
            var particleHolder = baseHolder ? baseHolder.transform.Find("Ring") : null;

            if (particleHolder)
            {
                var particleComponent = particleHolder.GetComponent<ParticleSystem>();
                if (particleComponent != null)
                {
                    var particle = particleComponent.main;
                    particle.startLifetime = WarpedEcho.Delay_Base.Value;
                    particle.startSizeMultiplier = 3;
                }
            }

            for (int i = 0; i < particleHolder.childCount; i++)
            {
                var smallerParticle = particleHolder.GetChild(i);
                var smallerComponent = smallerParticle ? smallerParticle.GetComponent<ParticleSystem>() : null;
                if (smallerComponent)
                {
                    var particle = smallerComponent.main;
                    particle.startDelay = WarpedEcho.Delay_Base.Value;
                    particle.startSizeMultiplier = 3;
                }
            }
        }
        private static void SetDelayTimer(ILContext il)
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
                    if (delayedVFX && body)
                    {
                        UnityEngine.Object.Instantiate(delayedVFX, body.transform.position, Quaternion.identity, body.mainHurtBox.transform);
                        UnityEngine.Object.Instantiate(delayedVFX, body.transform.position, Quaternion.identity, body.mainHurtBox.transform);
                    }
                });
            }
            else
            {
                Log.Warning(WarpedEcho.StaticName + " #1 - IL Fail #1");
            }
        }
        private static void ImmediateRefresh(ILContext il)
        {
            var cursor = new ILCursor(il);

            if (cursor.TryGotoNext(x => x.MatchCall<CharacterBody>(nameof(CharacterBody.RemoveBuff))))
            { } else
            {
                Log.Warning(WarpedEcho.StaticName + " #1 - IL Fail #2");
            }

            if (cursor.TryGotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<CharacterBody>(nameof(CharacterBody.incomingDamageList)),
                x => x.MatchLdloc(out _)
            ))
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Action<CharacterBody>>(body =>
                {
                    body.AddBuff(DLC2Content.Buffs.DelayedDamageBuff);
                });
            }
            else
            {
                Log.Warning(WarpedEcho.StaticName + " #1 - IL Fail #3");
            }
        }
        private static void RemoveOriginalVFX(ILContext il)
        {
            var cursor = new ILCursor(il);

            if (cursor.TryGotoNext(x => x.MatchLdstr("Prefabs/Effects/DelayedDamageIndicator")))
            {
                var previousIndex = cursor.Index;

                if (cursor.TryGotoNext(x => x.MatchPop()))
                {
                    var skipLabel = cursor.MarkLabel();
                    cursor.Goto(previousIndex);
                    cursor.Emit(OpCodes.Br, skipLabel.Target);
                }
            }
            else
            {
                Log.Warning(WarpedEcho.StaticName + " #1 - IL Fail #4");
            }
        }
        private static void BuffLogic(On.RoR2.CharacterBody.orig_UpdateDelayedDamage orig, CharacterBody self, float deltaTime)
        {
            if (!NetworkServer.active) return;
            if (!self.inventory) return;

            int itemCount = self.inventory.GetItemCount(DLC2Content.Items.DelayedDamage);
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
        }
        private static void RemoveBehaviorVFX(On.RoR2.DelayedDamageEffectUpdater.orig_SpawnDelayedDamageEffect orig, DelayedDamageEffectUpdater self)
        {
            return;
        }
    }
}
