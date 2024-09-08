using R2API;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace RoR2
{
    public class TransmitterEffect
    {
        static BuffDef inTransmitterDef;
        public TransmitterEffect()
        {
            SetupBuff();
            On.RoR2.CharacterBody.OnBuffFirstStackGained += OnEnter;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += OnExit;
            On.RoR2.CharacterBody.DoItemUpdates += SpecialEffect;
        }
        void SetupBuff()
        {
            inTransmitterDef = ScriptableObject.CreateInstance<BuffDef>();
            inTransmitterDef.name = "inTransmitterBuff";
            inTransmitterDef.canStack = false;
            inTransmitterDef.isCooldown = true;
            inTransmitterDef.ignoreGrowthNectar = true;
            inTransmitterDef.isHidden = true;
            inTransmitterDef.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/EliteIce/texBuffAffixWhite.tif").WaitForCompletion();
            ContentAddition.AddBuffDef(inTransmitterDef);
        }
        
        private void OnEnter(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (buffDef == inTransmitterDef)
            {
                CharacterModel characterModel = self.modelLocator.modelTransform.GetComponent<CharacterModel>();
                CharacterMotor characterMotor = self.characterMotor;
                characterMotor.walkSpeedPenaltyCoefficient = 2.0f;
                characterModel.invisibilityCount++;

                self.hurtBoxGroup = self.hurtBoxGroup;
                if (self.hurtBoxGroup)
                {
                    HurtBoxGroup hurtBoxGroup = self.hurtBoxGroup;
                    int i = hurtBoxGroup.hurtBoxesDeactivatorCounter + 1;
                    hurtBoxGroup.hurtBoxesDeactivatorCounter = i;
                }

                effectCounter = 0.15f;
            }
        }
        private void OnExit(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (buffDef == inTransmitterDef)
            {
                CharacterModel characterModel = self.modelLocator.modelTransform.GetComponent<CharacterModel>();
                CharacterMotor characterMotor = self.characterMotor;
                characterMotor.walkSpeedPenaltyCoefficient = 1.0f;
                characterModel.invisibilityCount--;

                if (self.hurtBoxGroup)
                {
                    HurtBoxGroup hurtBoxGroup = self.hurtBoxGroup;
                    int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter - 1;
                    hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
                }

                EffectManager.SpawnEffect(GlobalEventManager.CommonAssets.bleedOnHitAndExplodeImpactEffect, new EffectData
                {
                    origin = self.transform.position,
                    scale = 5f,
                    rotation = Quaternion.identity
                }, true);
            }
        }

        private void SpecialEffect(On.RoR2.CharacterBody.orig_DoItemUpdates orig, CharacterBody self, float deltaTime)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'System.Void RoR2.CharacterBody:SpecialEffect(System.Single)' called on client");
                return;
            }
            if (self.GetBuffCount(inTransmitterDef) > 0)
            {
                if (effectCounter < 0) {
                    effectCounter = 0.15f;
                    EffectManager.SpawnEffect(GlobalEventManager.CommonAssets.bleedOnHitAndExplodeImpactEffect, new EffectData
                    {
                        origin = self.transform.position,
                        scale = 1.25f,
                        rotation = Quaternion.identity
                    }, true);
                }
                effectCounter -= deltaTime;
                Debug.Log(effectCounter);
            }
        }

        public static BuffDef getTransmitterDef()
        {
            return inTransmitterDef;
        }

        private float effectCounter = -1;
    }
}
