using BepInEx.Configuration;
using EntityStates;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using RoR2.Items;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using R2API;

using static SeekingItemReworks.ColorCode;

namespace SeekerItems
{
    internal class NoxiousThorn : ItemTemplate
    {
        public NoxiousThorn(int descType)
        {
            ItemInternal = "TRIGGERENEMYDEBUFFS";

            string sequenceDoT = "bleed ".Style(FontColor.cIsDamage) + "enemies ";
            if (Inflict_Type.Value == 1) sequenceDoT = "apply " + "poison ".Style(FontColor.cIsDamage);
            if (Inflict_Type.Value == 2) sequenceDoT = "apply " + "blight ".Style(FontColor.cIsDamage);

            if (descType == 1)
            {
                ItemInfo = "Taking damage surrounds yourself in a ring of thorns.";
                ItemDesc = string.Format(
                    "Getting hit surrounds you in a " + "ring of thorns ".Style(FontColor.cIsDamage) + "that {0}" + "for " + "{1}% ".Style(FontColor.cIsDamage) + "(+{2}% per stack) ".Style(FontColor.cStack) + "damage. The ring " + "grows when taking damage".Style(FontColor.cIsDamage) + ", increasing its radius by " + "{3}m".Style(FontColor.cIsDamage) + ". Stacks up to " + "{4}m ".Style(FontColor.cIsDamage) + "(+{5}m per stack)".Style(FontColor.cStack) + ".",
                    sequenceDoT,
                    RoundVal(Damage_Base.Value), RoundVal(Damage_Stack.Value),
                    RoundVal(Range_Increment.Value), RoundVal(Base_Cap.Value),
                    RoundVal(Stack_Cap.Value)
                );
            }
        }

        public static string StaticName = "Noxious Thorn";

        public static ConfigEntry<int> Rework;

        public static ConfigEntry<int> Inflict_Type;
        public static ConfigEntry<float> Refresh;
        public static ConfigEntry<float> Damage_Frequency;

        public static ConfigEntry<float> Damage_Base;
        public static ConfigEntry<float> Damage_Stack;
        public static ConfigEntry<float> Initial_Range;
        public static ConfigEntry<float> Range_Increment;
        public static ConfigEntry<float> Base_Cap;
        public static ConfigEntry<float> Stack_Cap;
    }

    public static class NoxiousThornBehavior
    {
        public static void Init()
        {
            if (NoxiousThorn.Rework.Value == 1) IL.RoR2.HealthComponent.TakeDamageProcess += ReplaceEffect;
        }

        private static void ReplaceEffect(ILContext il)
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
                Log.Warning(NoxiousThorn.StaticName + " #1 - IL Fail #1");
            }
        }
    }

    public class NoxiousAuraItemBehavior : BaseItemBodyBehavior
    {
        [ItemDefAssociation(useOnServer = true, useOnClient = false)]
        public static ItemDef GetItemDef() => (NoxiousThorn.Rework.Value == 1) ? DLC2Content.Items.TriggerEnemyDebuffs : null;

        [InitDuringStartup]
        private static void Init()
        {
            auraEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Icicle/IcicleAura.prefab").WaitForCompletion().InstantiateClone("NoxiousAura", true);

            var thornController = auraEffect.AddComponent<ThornAuraController>();
            var frostController = auraEffect.GetComponent<IcicleAuraController>();

            thornController.auraParticles = new ParticleSystem[frostController.auraParticles.Length];
            Array.Copy(frostController.auraParticles, thornController.auraParticles, frostController.auraParticles.Length);

            foreach (ParticleSystem particleSystem in thornController.auraParticles)
            {
                ParticleSystemRenderer renderParticle = (particleSystem) ? particleSystem.GetComponent<ParticleSystemRenderer>() : null;
                if (renderParticle)
                {
                    if (renderParticle.name == "Chunks") renderParticle.gameObject.SetActive(false);
                    if (renderParticle.sharedMaterial)
                    {
                        var newMaterial = new Material(renderParticle.sharedMaterial);
                        Color newColor = renderParticle.name != "SpinningSharpChunks" ? new Color(0.72f, 0.75f, 0.02f) : new Color(1f, 0f, 0.95f);
                        newMaterial.SetColor("_Color", newColor);
                        newMaterial.SetColor("_TintColor", newColor);
                        newMaterial.SetFloat("_RimStrength", 1.15f);
                        newMaterial.SetFloat("_IntersectionStrength", 1f);
                        newMaterial.SetFloat("_BrightnessBoost", 2f);
                        newMaterial.SetFloat("_SoftFactor", 5f);
                        newMaterial.SetFloat("_AlphaBoost", 2.5f);
                        newMaterial.SetFloat("_SoftPower", 0.5f);
                        renderParticle.sharedMaterial = newMaterial;
                    }
                }
            }

            Destroy(frostController);
        }

        private void OnEnable()
        {
            On.RoR2.HealthComponent.TakeDamageProcess += IncreaseCount;
            GameObject gameObject = Instantiate(auraEffect, transform.position, Quaternion.identity);
            gameObject.transform.SetParent(transform);
            clonedAura = gameObject.GetComponent<ThornAuraController>();
            clonedAura.owner = base.gameObject;
            NetworkServer.Spawn(gameObject);
        }
        private void OnDisable()
        {
            On.RoR2.HealthComponent.TakeDamageProcess -= IncreaseCount;
            if (clonedAura)
            {
                Destroy(clonedAura);
                clonedAura = null;
            }
        }

        private void IncreaseCount(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            orig(self, damageInfo);
            if (clonedAura && self.body == body) clonedAura.TookDamage(damageInfo);
        }

        private static GameObject auraEffect;
        private ThornAuraController clonedAura;
    }
    public class ThornAuraController : NetworkBehaviour
    {
        private int ItemCount { get => cachedOwnerInfo.characterBody ? cachedOwnerInfo.characterBody.inventory.GetItemCount(DLC2Content.Items.TriggerEnemyDebuffs) : 0; }

        public void TookDamage(DamageInfo damageInfo)
        {
            if (damageInfo.damage > 0)
            {
                duration = durationRefresh;
                countHit++;
            }
        }
        private void FixedUpdate()
        {
            auraRadius = Math.Min((countHit > 0) ? NoxiousThorn.Initial_Range.Value + NoxiousThorn.Range_Increment.Value * (countHit - 1) : 0, NoxiousThorn.Base_Cap.Value + NoxiousThorn.Stack_Cap.Value * (ItemCount - 1));
            if (cachedOwnerInfo.gameObject != owner) cachedOwnerInfo = new OwnerInfo(owner);
            if (NetworkServer.active && cachedOwnerInfo.characterBody && countHit > 0)
            {
                duration -= Time.fixedDeltaTime;
                frequency -= Time.fixedDeltaTime;

                if (duration <= 0)
                {
                    countHit = 0;
                    duration = durationRefresh;
                }

                if (frequency <= 0)
                {
                    frequency = damageFrequency;
                    List<HurtBox> bodyList = HG.CollectionPool<HurtBox, List<HurtBox>>.RentCollection();

                    SphereSearch hitBox = new()
                    {
                        mask = LayerIndex.entityPrecise.mask,
                        origin = cachedOwnerInfo.characterBody.transform.position,
                        radius = auraRadius,
                        queryTriggerInteraction = QueryTriggerInteraction.UseGlobal
                    };

                    hitBox.RefreshCandidates();
                    hitBox.FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(cachedOwnerInfo.characterBody.teamComponent.teamIndex));
                    hitBox.OrderCandidatesByDistance();
                    hitBox.FilterCandidatesByDistinctHurtBoxEntities();
                    hitBox.GetHurtBoxes(bodyList);
                    hitBox.ClearCandidates();

                    foreach (HurtBox hurtBox in bodyList)
                    {
                        CharacterBody victim = hurtBox.healthComponent.body;
                        if (hurtBox && hurtBox.healthComponent && hurtBox.healthComponent.alive && victim)
                        {
                            DotController.DotIndex dotType = DotController.DotIndex.Bleed;
                            if (NoxiousThorn.Inflict_Type.Value == 1) dotType = DotController.DotIndex.Poison;
                            if (NoxiousThorn.Inflict_Type.Value == 2) dotType = DotController.DotIndex.Blight;

                            InflictDotInfo blightDot = new()
                            {
                                attackerObject = cachedOwnerInfo.gameObject,
                                victimObject = victim.gameObject,
                                dotIndex = dotType,
                                totalDamage = cachedOwnerInfo.characterBody.damage * ((NoxiousThorn.Damage_Base.Value + NoxiousThorn.Damage_Stack.Value * (ItemCount - 1)) / 100f)
                            };
                            DotController.InflictDot(ref blightDot);
                        }
                    }
                }
            }
        }
        private void LateUpdate()
        {
            UpdateVisuals();
            ToggleThorns();
        }
        private void UpdateVisuals(float radiusForce = 0)
        {
            if (cachedOwnerInfo.gameObject) transform.position = (cachedOwnerInfo.characterBody ? cachedOwnerInfo.characterBody.corePosition : cachedOwnerInfo.transform.position);
            float scale = Mathf.SmoothDamp(transform.localScale.x, auraRadius, ref scaleVelocity, 0.5f);
            if (radiusForce != 0) scale = Mathf.SmoothDamp(transform.localScale.x, radiusForce, ref scaleVelocity, 0.5f);
            transform.localScale = new Vector3(scale, scale, scale);
        }
        private void ToggleThorns()
        {
            if (countHit > 0)
            {
                foreach (ParticleSystem particleSystem in auraParticles)
                {
                    if (particleSystem == null) continue;
                    var particleMain = particleSystem.main;
                    particleMain.loop = true;
                    if (!particleSystem.isPlaying) particleSystem.Play();
                }
            }
            else
            {
                foreach (ParticleSystem particleSystem in auraParticles)
                {
                    if (particleSystem == null) continue;
                    var particleMain = particleSystem.main;
                    particleMain.loop = false;
                }
            }
        }

        [SyncVar]
        private float auraRadius;
        [SyncVar]
        private int countHit;

        private static readonly float durationRefresh = NoxiousThorn.Refresh.Value;
        private static readonly float damageFrequency = NoxiousThorn.Damage_Frequency.Value;
        private float duration;
        private float frequency;

        [SyncVar]
        public GameObject owner;
        private float scaleVelocity;
        public ParticleSystem[] auraParticles;
        private OwnerInfo cachedOwnerInfo;

        private readonly struct OwnerInfo
        {
            public OwnerInfo(GameObject gameObject)
            {
                this.gameObject = gameObject;
                if (gameObject)
                {
                    transform = gameObject.transform;
                    characterBody = gameObject.GetComponent<CharacterBody>();
                    return;
                }
                transform = null;
                characterBody = null;
            }

            public readonly GameObject gameObject;
            public readonly Transform transform;
            public readonly CharacterBody characterBody;
        }
    }
}
