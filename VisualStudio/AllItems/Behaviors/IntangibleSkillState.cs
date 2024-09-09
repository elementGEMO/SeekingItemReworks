using RoR2;
using SeekerItems;
using UnityEngine;

namespace EntityStates
{
    public class IntangibleSkillState : GenericCharacterMain
    {
        public override void OnEnter()
        {
            base.OnEnter();
            int stackCount = characterBody.inventory.GetItemCount(DLC2Content.Items.TeleportOnLowHealth);
            characterGravityParameterProvider = gameObject.GetComponent<ICharacterGravityParameterProvider>();
            characterFlightParameterProvider = gameObject.GetComponent<ICharacterFlightParameterProvider>();
            duration = (float)MainConfig.UUT_DBase.Value + MainConfig.UUT_DStack.Value * (stackCount - 1);

            Transform modelTransform = GetModelTransform();
            characterModel = modelTransform.GetComponent<CharacterModel>();

            if (characterBody && characterModel)
            {
                characterMotor.walkSpeedPenaltyCoefficient = 2.0f;
                characterModel.invisibilityCount++;

                characterBody.hurtBoxGroup = characterBody.hurtBoxGroup;
                if (characterBody.hurtBoxGroup)
                {
                    HurtBoxGroup hurtBoxGroup = characterBody.hurtBoxGroup;
                    int i = hurtBoxGroup.hurtBoxesDeactivatorCounter + 1;
                    hurtBoxGroup.hurtBoxesDeactivatorCounter = i;
                };

                Util.PlaySound("Play_item_proc_teleportOnLowHealth", gameObject);
                EffectManager.SpawnEffect(CharacterBody.CommonAssets.teleportOnLowHealthExplosion, new EffectData
                {
                    origin = transform.position,
                    scale = 30,
                    rotation = Quaternion.identity
                }, true);

                repeatVFX = 0.15f;
            }

            if (MainConfig.UnstableTransmitterFloatEnabled.Value)
            {
                if (characterGravityParameterProvider != null)
                {
                    CharacterGravityParameters gravityParameters = characterGravityParameterProvider.gravityParameters;
                    gravityParameters.channeledAntiGravityGranterCount++;
                    characterGravityParameterProvider.gravityParameters = gravityParameters;
                }
                if (characterFlightParameterProvider != null)
                {
                    CharacterFlightParameters flightParameters = characterFlightParameterProvider.flightParameters;
                    flightParameters.channeledFlightGranterCount++;
                    characterFlightParameterProvider.flightParameters = flightParameters;
                }
            }
        }

        public override void OnExit()
        {
            Transform modelTransform = GetModelTransform();
            characterModel = modelTransform.GetComponent<CharacterModel>();

            if (characterBody && characterModel)
            {
                characterMotor.walkSpeedPenaltyCoefficient = 1.0f;
                characterModel.invisibilityCount--;

                characterBody.hurtBoxGroup = characterBody.hurtBoxGroup;
                if (characterBody.hurtBoxGroup)
                {
                    HurtBoxGroup hurtBoxGroup = characterBody.hurtBoxGroup;
                    int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter - 1;
                    hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
                };

                Util.PlaySound("Play_item_proc_teleportOnLowHealth", gameObject);
                EffectManager.SpawnEffect(CharacterBody.CommonAssets.teleportOnLowHealthExplosion, new EffectData
                {
                    origin = transform.position,
                    scale = 10,
                    rotation = Quaternion.identity
                }, true);
            }

            if (MainConfig.UnstableTransmitterFloatEnabled.Value)
            {
                if (characterFlightParameterProvider != null)
                {
                    CharacterFlightParameters flightParameters = characterFlightParameterProvider.flightParameters;
                    flightParameters.channeledFlightGranterCount--;
                    characterFlightParameterProvider.flightParameters = flightParameters;
                }
                if (characterGravityParameterProvider != null)
                {
                    CharacterGravityParameters gravityParameters = characterGravityParameterProvider.gravityParameters;
                    gravityParameters.channeledAntiGravityGranterCount--;
                    characterGravityParameterProvider.gravityParameters = gravityParameters;
                }
            }

            base.OnExit();
        }
        public override bool CanExecuteSkill(GenericSkill skillSlot)
        {
            return false;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            repeatVFX -= GetDeltaTime();
            if (repeatVFX <= 0f)
            {
                Ray aimRay = GetAimRay();
                Util.PlaySound("Play_bleedOnCritAndExplode_explode", gameObject);
                EffectManager.SpawnEffect(GlobalEventManager.CommonAssets.bleedOnHitAndExplodeImpactEffect, new EffectData
                {
                    origin = transform.position,
                    scale = 1.25f,
                    rotation = Quaternion.LookRotation(aimRay.direction)
                }, true);
                repeatVFX = 0.15f;
            }
            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        private float duration;
        private float repeatVFX;
        private CharacterModel characterModel;

        private ICharacterGravityParameterProvider characterGravityParameterProvider;
        private ICharacterFlightParameterProvider characterFlightParameterProvider;
    }
}
