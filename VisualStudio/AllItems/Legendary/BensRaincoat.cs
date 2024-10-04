using BepInEx.Configuration;
using UnityEngine.AddressableAssets;
using RoR2;

namespace SeekerItems
{
    internal class BensRaincoat : ItemTemplate
    {
        public static string StaticName = "Bens Raincoat";

        public static ConfigEntry<int> Rework;
    }
    public static class BensRaincoatBehavior
    {
        public static void Init()
        {
            if (BensRaincoat.Rework.Value == 1)
            {
                BuffDef disableSkill = Addressables.LoadAsset<BuffDef>("RoR2/DLC2/bdDisableAllSkills.asset").WaitForCompletion();
                disableSkill.isDebuff = true;
            }
        }
    }
}
