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
        [ItemDefAssociation(useOnServer = true, useOnClient = true)]
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
            var thornAuraController = auraEffect.AddComponent<ThornAuraController>();
            var icicleAuraController = auraEffect.GetComponent<IcicleAuraController>();
            thornAuraController.auraParticles = new ParticleSystem[icicleAuraController.auraParticles.Length];
            Array.Copy(icicleAuraController.auraParticles, thornAuraController.auraParticles, icicleAuraController.auraParticles.Length);
            foreach (ParticleSystem particleSystem in thornAuraController.auraParticles)
            {
                ParticleSystemRenderer setParticle = particleSystem.GetComponent<ParticleSystemRenderer>();
                if (setParticle.name == "Chunks") setParticle.gameObject.SetActive(false);
                if (setParticle.sharedMaterial)
                {
                    var newSharedMaterial = new Material(setParticle.sharedMaterial);
                    var newColor = setParticle.name != "SpinningSharpChunks" ? new Color(0.72f, 0.75f, 0.02f) : new Color(1f, 0f, 0.95f);
                    newSharedMaterial.SetColor("_Color", newColor);
                    newSharedMaterial.SetColor("_TintColor", newColor);
                    newSharedMaterial.SetFloat("_RimStrength", 1.15f);
                    newSharedMaterial.SetFloat("_IntersectionStrength", 1f);
                    newSharedMaterial.SetFloat("_BrightnessBoost", 2f);
                    newSharedMaterial.SetFloat("_SoftFactor", 5f);
                    newSharedMaterial.SetFloat("_AlphaBoost", 2.5f);
                    newSharedMaterial.SetFloat("_SoftPower", 0.5f);
                    setParticle.sharedMaterial = newSharedMaterial;
                }
            }
            Destroy(icicleAuraController);
        }

        private void OnEnable()
        {
            On.RoR2.HealthComponent.TakeDamageProcess += increaseCount;
            GameObject gameObject = Instantiate(auraEffect, transform.position, Quaternion.identity);
            clonedAura = gameObject.GetComponent<ThornAuraController>();
            gameObject.transform.SetParent(transform);
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
                Debug.Log(countHit);
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
        private void UpdateVisuals()
        {
            if (cachedOwnerInfo.gameObject) transform.position = (cachedOwnerInfo.characterBody ? cachedOwnerInfo.characterBody.corePosition : cachedOwnerInfo.transform.position);
            float scale = Mathf.SmoothDamp(transform.localScale.x, auraRadius, ref scaleVelocity, 0.5f);
            transform.localScale = new Vector3(scale, scale, scale);
            Log.Debug(auraRadius);
        }
        private void ToggleThorns()
        {
            if (countHit > 0)
            {
                foreach (ParticleSystem particleSystem in auraParticles)
                {
                    var particleMain = particleSystem.main;
                    particleMain.loop = true;
                    if (!particleSystem.isPlaying) particleSystem.Play();
                }
            }
            else
            {
                foreach (ParticleSystem particleSystem in auraParticles)
                {
                    var particleMain = particleSystem.main;
                    particleMain.loop = false;
                }
            }
        }

        private int countHit;
        private float auraRadius;

        private static readonly float durationRefresh = NoxiousThorn.Refresh.Value;
        private static readonly float damageFrequency = NoxiousThorn.Damage_Frequency.Value;
        private float duration;
        private float frequency;

        /*
        //public BuffWard buffWard;
        private float actualRadius;
        private float scaleVelocity;
        //private new Transform transform;
        public int currentThornCount;

        private static float setDuration = 6f;
        private float timerDeplete;
        private static float dotDuration = 1f;
        private float dotDeplete;
        */

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