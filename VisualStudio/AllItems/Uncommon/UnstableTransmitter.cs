using BepInEx.Configuration;
using EntityStates;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using static SeekingItemReworks.ColorCode;

namespace SeekerItems
{
    internal class UnstableTransmitter : ItemTemplate
    {
        public UnstableTransmitter(int descType)
        {
            ItemInternal = "TELEPORTONLOWHEALTH";

            string sequenceDoT = "hemorrhaging";
            if (Inflict_Type.Value == 1) sequenceDoT = "bleeding";
            string sequenceDotColor = sequenceDoT.Style(FontColor.cIsHealth);
            if (Inflict_Type.Value == 1) sequenceDoT.Style(FontColor.cIsDamage);

            if (descType == 1)
            {
                ItemInfo = string.Format(
                    "Become intangible and explode, {0} nearby enemies at low health.",
                    sequenceDoT
                );
                ItemDesc = string.Format(
                    "Falling below " + "{0}% health ".Style(FontColor.cIsHealth) + "causes you to fade away, becoming " + "intangible ".Style(FontColor.cIsUtility) + "and exploding, {1} " + "enemies within " + "{2}m ".Style(FontColor.cIsDamage) + "for " + "{3}% ".Style(FontColor.cIsDamage) + "(+{4}% per stack) ".Style(FontColor.cStack) + "base damage. Lasts " + "{5}s ".Style(FontColor.cIsUtility) + "(+{6}s per stack)".Style(FontColor.cStack) + ". Recharges every " + "{7} ".Style(FontColor.cIsUtility) + "seconds.",
                    LowHealth.Value, sequenceDotColor, RoundVal(Range.Value),
                    RoundVal(Damage_Base.Value), RoundVal(Damage_Stack.Value),
                    RoundVal(Duration_Base.Value), RoundVal(Duration_Stack.Value),
                    RoundVal(Refresh.Value)
                );
            }
        }

        public static string StaticName = "Unstable Transmitter";

        public static ConfigEntry<int> Rework;

        public static ConfigEntry<int> Inflict_Type;
        public static ConfigEntry<int> LowHealth;
        public static ConfigEntry<float> Refresh;
        public static ConfigEntry<float> Range;
        public static ConfigEntry<bool> IsFloat;

        public static ConfigEntry<float> Damage_Base;
        public static ConfigEntry<float> Damage_Stack;
        public static ConfigEntry<float> Duration_Base;
        public static ConfigEntry<float> Duration_Stack;
    }

    public static class UnstableTransmitterBehavior
    {
        public static void Init()
        {
            if (UnstableTransmitter.Rework.Value == 1)
            {
                On.RoR2.CharacterBody.RpcTeleportCharacterToSafety += ReplaceEffect;
                On.RoR2.HealthComponent.UpdateLastHitTime += HealthTrigger;
                IL.RoR2.HealthComponent.UpdateLastHitTime += RemoveEffect;
            }
        }

        private static void ReplaceEffect(On.RoR2.CharacterBody.orig_RpcTeleportCharacterToSafety orig, CharacterBody self)
        {
            if (!self.hasEffectiveAuthority) return;

            EntityStateMachine.FindByCustomName(self.gameObject, "Body").SetNextState(new IntangibleSkillState());
            List<HurtBox> bodyList = HG.CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
            int itemCount = self.inventory.GetItemCount(DLC2Content.Items.TeleportOnLowHealth);

            SphereSearch hitBox = new()
            {
                mask = LayerIndex.entityPrecise.mask,
                origin = self.transform.position,
                radius = UnstableTransmitter.Range.Value,
                queryTriggerInteraction = QueryTriggerInteraction.UseGlobal
            };

            hitBox.RefreshCandidates();
            hitBox.FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(self.teamComponent.teamIndex));
            hitBox.OrderCandidatesByDistance();
            hitBox.FilterCandidatesByDistinctHurtBoxEntities();
            hitBox.GetHurtBoxes(bodyList);
            hitBox.ClearCandidates();

            foreach (HurtBox hurtBox in bodyList)
            {
                CharacterBody victim = hurtBox.healthComponent.body;
                if (hurtBox && hurtBox.healthComponent && hurtBox.healthComponent.alive && victim)
                {
                    DotController.DotIndex dotType = DotController.DotIndex.SuperBleed;
                    if (UnstableTransmitter.Inflict_Type.Value == 2) dotType = DotController.DotIndex.Bleed;

                    InflictDotInfo bleedDot = new()
                    {
                        attackerObject = self.gameObject,
                        victimObject = victim.gameObject,
                        dotIndex = dotType,
                        damageMultiplier = self.damage * (UnstableTransmitter.Damage_Base.Value + UnstableTransmitter.Damage_Stack.Value * (itemCount - 1)) / 100f,
                        duration = 2.5f
                    };
                    DotController.InflictDot(ref bleedDot);
                }
            }
        }
        private static void HealthTrigger(On.RoR2.HealthComponent.orig_UpdateLastHitTime orig, HealthComponent self, float damage, Vector3 damagePos, bool silentDamage, GameObject attacker)
        {
            if (NetworkServer.active && self.body && damage > 0)
            {
                int itemCount = self.body.inventory ? self.body.inventory.GetItemCount(DLC2Content.Items.TeleportOnLowHealth) : 0;
                bool hasBuff = self.body.HasBuff(DLC2Content.Buffs.TeleportOnLowHealth);

                if (self.combinedHealthFraction <= (UnstableTransmitter.LowHealth.Value / 100f) && itemCount > 0 && hasBuff)
                {
                    self.body.hasTeleported = true;
                    self.body.RemoveBuff(DLC2Content.Buffs.TeleportOnLowHealth);
                    self.body.AddTimedBuff(DLC2Content.Buffs.TeleportOnLowHealthCooldown, UnstableTransmitter.Refresh.Value);
                    self.body.CallRpcTeleportCharacterToSafety();
                }
            }

            orig(self, damage, damagePos, silentDamage, attacker);
        }
        private static void RemoveEffect(ILContext il)
        {
            var cursor = new ILCursor(il);

            if (cursor.TryGotoNext(
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
                Log.Warning(UnstableTransmitter.StaticName + " #1 - IL Fail #1");
            }
        }
    }
}
