using BepInEx;
using BepInEx.Configuration;

namespace SeekerItems
{
    public static class MainConfig
    {
        public static void SetUpConfigs(BaseUnityPlugin plugin)
        {
            GenericConfig(plugin);
            CommonConfig(plugin);
            UncommonConfig(plugin);
            LegendaryConfig(plugin);
            EquipmentConfig(plugin);
            MiscConfig(plugin);
        }

        // General Config
        public static ConfigEntry<int> RoundNumber;
        public static ConfigEntry<bool> CursedRewording;

        public static void GenericConfig(BaseUnityPlugin plugin)
        {
            RoundNumber = plugin.Config.Bind("! General !", "Item Description Round", 0, "Decides what decimal spot to round to.\n- 0 = Whole, 1 = Tenth, 2 = Hundredths, 3 = ...\n");
            CursedRewording = plugin.Config.Bind("! General !", "Cursed Wording", false, "Makes some item descriptions... a little cursed.\n");
        }

        // Warped Echo
        public static ConfigEntry<bool> WarpedEchoReworkEnabled;
        public static ConfigEntry<bool> WarpedEchoFixEnabled;

        public static ConfigEntry<float> CWE_Cap;
        public static ConfigEntry<float> CWE_Delay;
        public static ConfigEntry<float> CWE_Stack;

        // Chronic Expansion
        public static ConfigEntry<bool> ChronicExpansionReworkEnabled;

        public static ConfigEntry<float> CCE_Base;
        public static ConfigEntry<float> CCE_Stack;

        // Chronic Expansion
        public static ConfigEntry<bool> KnockbackFinReworkEnabled;

        public static ConfigEntry<bool> CKF_Hyperbolic;
        public static ConfigEntry<float> CKF_KBase;
        public static ConfigEntry<float> CKF_KStack;
        public static ConfigEntry<float> CKF_DBase;
        public static ConfigEntry<float> CKF_DStack;

        public static void CommonConfig(BaseUnityPlugin plugin)
        {
            WarpedEchoReworkEnabled = plugin.Config.Bind("Warped Echo", "Enable Rework?", true, "Changes Warped Echo to only split damage one at a time, stacking delay duration. Forces bug fixes internally.");
            WarpedEchoFixEnabled = plugin.Config.Bind("Warped Echo", "Enable Bug Fixes?", true, "Prevents Warped Echo from splitting lethal damage.");

            CWE_Cap = plugin.Config.Bind("Warped Echo", "! Damage Cap !", 1.0f, "[ Applies to Rework & Bugfix ]\nThe percentage at which damage can't exceed the current health.\n - 1.0 = Damage can't be over 100% of health.\n");
            CWE_Delay = plugin.Config.Bind("Warped Echo", "! Base Delay !", 3.0f, "[ Applies to Rework Only ]\nThe base time for delayed damage triggering.\n- 3.0 = Damage delayed for 3 seconds.\n");
            CWE_Stack = plugin.Config.Bind("Warped Echo", "! Stack Delay !", 1.5f, "[ Applies to Rework Only ]\nThe extra time for delayed damage triggering when stacking.\n- 1.5 = Damage delayed for +1.5 seconds with each stack.\n");
            
            ChronicExpansionReworkEnabled = plugin.Config.Bind("Chronic Expansion", "Enable Rework?", true, "Changes Chronic Expansion by slightly nerfing the values and replacing current stacking with a proper formula.");

            CCE_Base = plugin.Config.Bind("Chronic Expansion", "! Base Stack !", 7.0f, "[ Applies to Rework Only ]\nThe base percentage damage for one buff.\n- 7.0 = 7% damage increase per buff.\n");
            CCE_Stack = plugin.Config.Bind("Chronic Expansion", "! Stack !", 5.0f, "[ Applies to Rework Only ]\nThe extra percentage damage for one buff when stacking.\n- 5% = +5% damage increase per buff when stacking.\n");

            KnockbackFinReworkEnabled = plugin.Config.Bind("Knockback Fin", "Enable Rework?", true, "Changes Knockback Fin by reducing knock chances, and adds extra damage to airborne enemies.");

            CKF_Hyperbolic = plugin.Config.Bind("Knockback Fin", "! Hyperbolic Scaling !", true, "[ Applies to Rework Only ]\nKnockback Fin's proc chance is hyperbolic, though this is an option to toggle it.\n");
            CKF_KBase = plugin.Config.Bind("Knockback Fin", "! Knock Base Stack !", 5.0f, "[ Applies to Rework Only ]\nThe base percentage chance to trigger.\n- 5.0 = 5% chance to trigger.\n");
            CKF_KStack = plugin.Config.Bind("Knockback Fin", "! Knock Stack !", 2.0f, "[ Applies to Rework Only ]\nThe extra percentage chance to trigger when stacking.\n- 2.0 = +2% per stack to trigger.\n");
            CKF_DBase = plugin.Config.Bind("Knockback Fin", "! Damage Base Stack !", 10.0f, "[ Applies to Rework Only ]\nThe damage percentage to airborne enemies.\n- 10.0 = 10% extra damage to airborne enemies.\n");
            CKF_DStack = plugin.Config.Bind("Knockback Fin", "! Damage Stack !", 10.0f, "[ Applies to Rework Only ]\nThe extra damage percentage to airborne enemies when stacking.\n- 10.0 = +10% extra damage per stack to airborne enemies.\n");
        }

        // Unstable Transmitter
        public static ConfigEntry<bool> UnstableTransmitterReworkEnabled;

        public static ConfigEntry<float> UUT_BBase;
        public static ConfigEntry<float> UUT_BStack;
        public static ConfigEntry<float> UUT_DBase;
        public static ConfigEntry<float> UUT_DStack;

        public static void UncommonConfig(BaseUnityPlugin plugin)
        {
            UnstableTransmitterReworkEnabled = plugin.Config.Bind("Unstable Transmitter", "Enable Rework?", true, "Changes Unstable Transmitter to instead burst in bleed damage, and become intangible.");

            UUT_BBase = plugin.Config.Bind("Unstable Transmitter", "! Base Bleed !", 100.0f, "[ Applies to Rework Only ]\nHow much base damage bleed does.\n-\n 100.0 = 100%.\n");
            UUT_BStack = plugin.Config.Bind("Unstable Transmitter", "! Stack Bleed !", 50.0f, "[ Applies to Rework Only ]\nHow much additional base damage bleed does when stacking.\n-\n 50.0 = +50% per stack.\n");
            UUT_DBase = plugin.Config.Bind("Unstable Transmitter", "! Base Duration !", 2.0f, "[ Applies to Rework Only ]\nHow long intangibility lasts.\n-\n 2.0f = 2 seconds.\n");
            UUT_DStack = plugin.Config.Bind("Unstable Transmitter", "! Stack Duration !", 0.5f, "[ Applies to Rework Only ]\nHow long intangibility lasts when stacking.\n-\n 0.5f = +0.5 seconds.\n");
        }

        // War Bonds
        public static ConfigEntry<bool> WarBondsReworkEnabled;
        public static ConfigEntry<bool> WarBondsReplaceVFX;

        public static ConfigEntry<int> LWB_PBase;
        public static ConfigEntry<int> LWB_PStack;
        public static ConfigEntry<float> LWB_EBase;
        public static ConfigEntry<float> LWB_EStack;

        public static void LegendaryConfig(BaseUnityPlugin plugin)
        {
            WarBondsReworkEnabled = plugin.Config.Bind("War Bonds", "Enable Rework?", true, "Changes War Bonds to free purchases instead, and gold purchases give experience.");
            WarBondsReplaceVFX = plugin.Config.Bind("War Bonds", "Change Visual Effects?", true, "Replaces War Bond's gold effect at the start of a stage with Brittle Crown's.");

            LWB_PBase = plugin.Config.Bind("War Bonds", "! Purchase Base Stack !", 3, "[ Applies to Rework Only ]\nHow much free purchases are gained at base.\n-\n 3 = 3 free purchases.\n");
            LWB_PStack = plugin.Config.Bind("War Bonds", "! Purchase Stack !", 2, "[ Applies to Rework Only ]\nHow much more free purchases per stack.\n-\n 2 = +2 free purchases per stack.\n");
            LWB_EBase = plugin.Config.Bind("War Bonds", "! Experience Base Stack !", 20.0f, "[ Applies to Rework Only ]\nBase conversion at base.\n-\n 10.0 = 20% converted experience.\n");
            LWB_EStack = plugin.Config.Bind("War Bonds", "! Experience Stack !", 10.0f, "[ Applies to Rework Only ]\nMore conversion per stack.\n-\n 10.0 = +10% converted experience per stack.\n");
        }

        // Seed of Life
        public static ConfigEntry<bool> SeedOfLifeRewriteEnabled;

        public static void EquipmentConfig(BaseUnityPlugin plugin)
        {
            SeedOfLifeRewriteEnabled = plugin.Config.Bind("Seed of Life", "Enable Rewrite?", true, "Changes Seed of Life, simplifying the pickup, and expands the description.");
        }

        // Ben's Raincoat
        public static ConfigEntry<bool> SkillDisableCleansable;

        public static void MiscConfig(BaseUnityPlugin plugin)
        {
            SkillDisableCleansable = plugin.Config.Bind("! Misc !", "Bens Raincoat Cleansing Skill Disable", false, "Lets Ben's Raincoat able to cleanse False Son's skill disable debuff.\nReally, any cleanse item works now.");
        }
    }
}
