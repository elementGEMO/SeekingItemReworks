using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SeekerItems
{
    public class LanternCountBuff
    {
        public static BuffDef LanternCounter;
        private static readonly Sprite spriteIcon = Addressables.LoadAssetAsync<Sprite>("RoR2/DLC2/Items/LowerHealthHigherDamage/texBuffLowerHealthHigherDamageIcon.png").WaitForCompletion();

        public LanternCountBuff()
        {
            LanternCounter = ScriptableObject.CreateInstance<BuffDef>();
            LanternCounter.name = "AllyLanternCount";
            LanternCounter.canStack = true;
            LanternCounter.isCooldown = false;
            LanternCounter.isDebuff = false;
            LanternCounter.ignoreGrowthNectar = false;
            LanternCounter.iconSprite = spriteIcon;
            (LanternCounter as UnityEngine.Object).name = LanternCounter.name;
            ContentAddition.AddBuffDef(LanternCounter);
        }
    }
}