using UnityEngine;
using RoR2.Items;
using RoR2;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using R2API;
using System.Collections.Generic;
using System;

namespace SeekerItems
{
    public class NoxiousThornBehavior : BaseItemBodyBehavior
    {
        [ItemDefAssociation(useOnServer = true, useOnClient = false)]
        public static ItemDef GetItemDef()
        {
            if (NoxiousThorn.Rework.Value == 1)
            {
                return RoR2.DLC2Content.Items.TriggerEnemyDebuffs;
            }
            else return null;
        }

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
                    //ParticleSystemRenderer renderParticle = particleSystem.GetComponent<ParticleSystemRenderer>();
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
            On.RoR2.HealthComponent.TakeDamageProcess += increaseCount;
            GameObject gameObject = Instantiate(auraEffect, transform.position, Quaternion.identity);
            gameObject.transform.SetParent(transform);
            clonedAura = gameObject.GetComponent<ThornAuraController>();
            clonedAura.owner = base.gameObject;
            NetworkServer.Spawn(gameObject);
        }
        private void OnDisable()
        {
            On.RoR2.HealthComponent.TakeDamageProcess -= increaseCount;
            if (clonedAura)
            {
                Destroy(clonedAura);
                clonedAura = null;
            }
        }

        private void increaseCount(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            orig(self, damageInfo);
            if (clonedAura && self.body == body) clonedAura.TookDamage(damageInfo);
        }

        private static GameObject auraEffect;
        private ThornAuraController clonedAura;
    }
    public class ThornAuraController : NetworkBehaviour
    {
        private int itemCount
        {
            get { return cachedOwnerInfo.characterBody ? cachedOwnerInfo.characterBody.inventory.GetItemCount(DLC2Content.Items.TriggerEnemyDebuffs) : 0; }
        }

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
            auraRadius = Math.Min((countHit > 0) ? NoxiousThorn.Initial_Range.Value + NoxiousThorn.Range_Increment.Value * (countHit - 1) : 0, NoxiousThorn.Base_Cap.Value + NoxiousThorn.Stack_Cap.Value * (itemCount - 1));
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
                    List<HurtBox> list = HG.CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
                    SphereSearch hitBox = new();
                    hitBox.mask = LayerIndex.entityPrecise.mask;
                    hitBox.origin = cachedOwnerInfo.characterBody.transform.position;
                    hitBox.radius = auraRadius;
                    hitBox.queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;
                    hitBox.RefreshCandidates();
                    hitBox.FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(cachedOwnerInfo.characterBody.teamComponent.teamIndex));
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
                            DotController.DotIndex dotType = DotController.DotIndex.Bleed;
                            if (NoxiousThorn.Inflict_Type.Value == 1) dotType = DotController.DotIndex.Poison;
                            if (NoxiousThorn.Inflict_Type.Value == 2) dotType = DotController.DotIndex.Blight;
                            InflictDotInfo blightDot = new()
                            {
                                attackerObject = cachedOwnerInfo.gameObject,
                                victimObject = victimBody.gameObject,
                                dotIndex = dotType,
                                totalDamage = cachedOwnerInfo.characterBody.damage * ((NoxiousThorn.Damage_Base.Value + NoxiousThorn.Damage_Stack.Value * (itemCount - 1)) / 100f)
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
        private OwnerInfo cachedOwnerInfo;
        public ParticleSystem[] auraParticles;
        private float scaleVelocity;

        private struct OwnerInfo
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