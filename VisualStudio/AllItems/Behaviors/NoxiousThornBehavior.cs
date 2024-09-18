using UnityEngine;
using RoR2.Items;
using RoR2;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using R2API;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;

namespace SeekerItems
{
    public class NoxiousThornBehavior : BaseItemBodyBehavior
    {
        [ItemDefAssociation(useOnServer = true, useOnClient = true)]
        public static ItemDef GetItemDef()
        {
            return RoR2.DLC2Content.Items.TriggerEnemyDebuffs;
        }

        [InitDuringStartup]
        private static void Init()
        {
            Debug.Log("SeekingItemReworks: Instantiated");
            auraEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Icicle/IcicleAura.prefab").WaitForCompletion().InstantiateClone("NoxiousAura", true);
            var thornAuraController = auraEffect.AddComponent<ThornAuraController>();
            var icicleAuraController = auraEffect.GetComponent<IcicleAuraController>();
            thornAuraController.auraParticles = new ParticleSystem[icicleAuraController.auraParticles.Length];
            Array.Copy(icicleAuraController.auraParticles, thornAuraController.auraParticles, icicleAuraController.auraParticles.Length);
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
            if (damageInfo.damage > 0)
            {
                Debug.Log("Took Damage");
                clonedAura.ThornCount();
            }
        }

        private static GameObject auraEffect;
        private ThornAuraController clonedAura;
    }
    public class ThornAuraController : NetworkBehaviour
    {
        private int maxThornCount
        {
            get { return cachedOwnerInfo.characterBody ? cachedOwnerInfo.characterBody.inventory.GetItemCount(DLC2Content.Items.TriggerEnemyDebuffs) : 0; }
        }
        /*
        private void Awake()
        {
            transform = base.transform;
        }*/
        private void FixedUpdate()
        {
            //Debug.Log("Is updating Noxious");
            if (cachedOwnerInfo.gameObject != owner)
            {
                cachedOwnerInfo = new OwnerInfo(owner);
            }
            actualRadius = 10f;
            if (cachedOwnerInfo.characterBody && currentThornCount > 0)
            {
                timerDeplete -= Time.fixedDeltaTime;
                if (timerDeplete < 0)
                {
                    currentThornCount = 0;
                    timerDeplete = setDuration;
                }
                ToggleThorns(true);
            }
            else
            {
                ToggleThorns(false);
            }
        }

        private void ToggleThorns(bool isActive)
        {
            if (isActive)
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

        public void ThornCount()
        {
            currentThornCount = Math.Max(Math.Min(currentThornCount + 1, maxThornCount), 0);
            timerDeplete = setDuration;
            Debug.Log("SeekingItemReworks: " +  currentThornCount);
        }

        private void UpdateVisuals()
        {
            /*
            if (cachedOwnerInfo.gameObject)
            {
                transform.position = (cachedOwnerInfo.characterBody ? cachedOwnerInfo.characterBody.corePosition : cachedOwnerInfo.transform.position);
            }
            */
            float num = Mathf.SmoothDamp(transform.localScale.x, actualRadius * currentThornCount, ref scaleVelocity, 0.5f);
            transform.localScale = new Vector3(num, num, num);
        }

        private void LateUpdate()
        {
            //Debug.Log("Late Update");
            UpdateVisuals();
        }

        public BuffWard buffWard;
        private float actualRadius;
        private float scaleVelocity;
        //private new Transform transform;
        public ParticleSystem[] auraParticles;
        public int currentThornCount;

        private static float setDuration = 7.5f;
        private float timerDeplete;

        [SyncVar]
        public GameObject owner;
        private OwnerInfo cachedOwnerInfo;

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