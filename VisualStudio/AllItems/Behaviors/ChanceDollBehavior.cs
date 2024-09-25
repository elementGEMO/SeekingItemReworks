using UnityEngine.Networking;
using RoR2.Items;
using RoR2.Orbs;
using RoR2;
using UnityEngine;
using System.Numerics;
using UnityEngine.AddressableAssets;
using R2API;

namespace SeekerItems
{
    internal class ChanceDollSetup
    {
        public static void Awake()
        {
            On.RoR2.ShrineChanceBehavior.Start += (orig, self) =>
            {
                orig(self);
                ChanceDollBehavior shrineCount = self.gameObject.AddComponent<ChanceDollBehavior>();
                shrineCount.FailCount = 0;
            };
        }
    }

    public class ChanceDollBehavior : NetworkBehaviour
    {
        public int FailCount;
    }

    public class KarmaDollBehavior : MonoBehaviour
    {
        public void Awake()
        {
            karmaLuck = 0;
            partialKarma = 0;
        }
        public void IncreaseKarma(int itemCount, CharacterBody body, GameObject interactableObject)
        {
            maxLuck = itemCount * 1;
            if (karmaLuck < maxLuck)
            {
                KarmaOrb karmaOrb = new()
                {
                    origin = interactableObject.transform.position,
                    target = body.mainHurtBox,
                    arrivalTime = 15f,

                };
                OrbManager.instance.AddOrb(karmaOrb);
                if (partialKarma >= karmaRefresh)
                {
                    partialKarma = 0;
                    karmaLuck++;
                }
            }
        }

        private int maxLuck;
        public int karmaLuck;
        public int partialKarma;
        private int karmaRefresh = 5;

        public CharacterBody owner;

        public void IncreasePartialKarma(int value)
        {
            partialKarma += value;
            if (owner) owner.statsDirty = true;
        }
    }
    public class KarmaOrb : Orb
    {
        [InitDuringStartup]
        private static void Init()
        {
            /*
            orbEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Infusion/InfusionOrbFlash.prefab").WaitForCompletion().InstantiateClone("KarmaOrb", false);
            var destroyParticle = orbEffect.GetComponent<DestroyOnParticleEnd>();
            var particle = destroyParticle.trackedParticleSystem;
            var particleRender = particle.GetComponent<ParticleSystemRenderer>();
            var newMaterial = new Material(particleRender.sharedMaterial);
            newMaterial.SetColor("_Emission", new Color(0, 0.43f, 0.82f));
            particleRender.sharedMaterial = newMaterial;
            */
        }
        public override void Begin()
        {
            duration = distanceToTarget / arrivalTime;
            EffectData effectData = new()
            {
                origin = origin,
                genericFloat = duration
            };
            effectData.SetHurtBoxReference(target);
            //EffectManager.SpawnEffect(orbEffect, effectData, true);
            EffectManager.SpawnEffect(OrbStorageUtility.Get("Prefabs/Effects/OrbEffects/InfusionOrbEffect"), effectData, true);
            HurtBox hurtBox = target.GetComponent<HurtBox>();
            CharacterBody characterBody = (hurtBox != null) ? hurtBox.healthComponent.GetComponent<CharacterBody>() : null;
            if (characterBody) karmaDollBehavior = characterBody.master.playerCharacterMasterController.gameObject.gameObject.GetComponent<KarmaDollBehavior>();
        }
        public override void OnArrival()
        {
            if (karmaDollBehavior) karmaDollBehavior.IncreasePartialKarma(1) ;
        }

        private KarmaDollBehavior karmaDollBehavior;
        private static GameObject orbEffect;
    }
}
