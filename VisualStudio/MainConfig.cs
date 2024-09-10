﻿using BepInEx;
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

        // Antler Shield
        public static ConfigEntry<bool> AntlerShieldReworkEnabled;

        public static ConfigEntry<float> CAS_ABase;
        public static ConfigEntry<float> CAS_AStack;
        public static ConfigEntry<float> CAS_MBase;
        public static ConfigEntry<float> CAS_MStack;

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

            CKF_Hyperbolic = plugin.Config.Bind("Knockback Fin", "! Hyperbolic Scaling !", false, "[ Applies to Rework Only ]\nKnockback Fin's proc chance is hyperbolic, though this is an option to toggle it.\n");
            CKF_KBase = plugin.Config.Bind("Knockback Fin", "! Knock Base Stack !", 5.0f, "[ Applies to Rework Only ]\nThe base percentage chance to trigger.\n- 5.0 = 5% chance to trigger.\n");
            CKF_KStack = plugin.Config.Bind("Knockback Fin", "! Knock Stack !", 2.0f, "[ Applies to Rework Only ]\nThe extra percentage chance to trigger when stacking.\n- 2.0 = +2% per stack to trigger.\n");
            CKF_DBase = plugin.Config.Bind("Knockback Fin", "! Damage Base Stack !", 10.0f, "[ Applies to Rework Only ]\nThe damage percentage to airborne enemies.\n- 10.0 = 10% extra damage to airborne enemies.\n");
            CKF_DStack = plugin.Config.Bind("Knockback Fin", "! Damage Stack !", 10.0f, "[ Applies to Rework Only ]\nThe extra damage percentage to airborne enemies when stacking.\n- 10.0 = +10% extra damage per stack to airborne enemies.\n");

            AntlerShieldReworkEnabled = plugin.Config.Bind("Antler Shield", "Enable Rework?", true, "Changes Antler Shield to scale armor off of speed, and gives slightly extra speed.");

            CAS_ABase = plugin.Config.Bind("Antler Shield", "! Armor Base Stack !", 5.0f, "[ Applies to Rework Only ]\nThe percent conversion of movement speed to armor.\n- 5.0 = 5% movement to armor.\n");
            CAS_AStack = plugin.Config.Bind("Antler Shield", "! Armor Stack !", 5.0f, "[ Applies to Rework Only ]\nThe extra percent conversion of movement speed to armor when stacking.\n- 5.0 = +5% movement to armor per stack.\n");
            CAS_MBase = plugin.Config.Bind("Antler Shield", "! Speed Base Stack !", 7.0f, "[ Applies to Rework Only ]\nThe base movement speed increase.\n- 7.0 = 7% movement speed.\n");
            CAS_MStack = plugin.Config.Bind("Antler Shield", "! Speed Stack !", 3.5f, "[ Applies to Rework Only ]\nThe extra movement speed increase per stack.\n- 3.5 = +3.5% movement speed per stack.\n");
        }

        // Unstable Transmitter
        public static ConfigEntry<bool> UnstableTransmitterReworkEnabled;
        public static ConfigEntry<bool> UnstableTransmitterFloatEnabled;

        public static ConfigEntry<float> UUT_BBase;
        public static ConfigEntry<float> UUT_BStack;
        public static ConfigEntry<float> UUT_DBase;
        public static ConfigEntry<float> UUT_DStack;
        public static ConfigEntry<float> UUT_Refresh;

        public static void UncommonConfig(BaseUnityPlugin plugin)
        {
            UnstableTransmitterReworkEnabled = plugin.Config.Bind("Unstable Transmitter", "Enable Rework?", true, "Changes Unstable Transmitter to instead burst in bleed damage, and become intangible.");
            UnstableTransmitterFloatEnabled = plugin.Config.Bind("Unstable Transmitter", "Enable Floating with Rework?", true, "During intangible state, float exactly like Strides of Heresy.");

            UUT_BBase = plugin.Config.Bind("Unstable Transmitter", "! Base Bleed !", 100.0f, "[ Applies to Rework Only ]\nHow much base damage bleed does.\n-\n 100.0 = 100%.\n");
            UUT_BStack = plugin.Config.Bind("Unstable Transmitter", "! Stack Bleed !", 50.0f, "[ Applies to Rework Only ]\nHow much additional base damage bleed does when stacking.\n-\n 50.0 = +50% per stack.\n");
            UUT_DBase = plugin.Config.Bind("Unstable Transmitter", "! Base Duration !", 2.0f, "[ Applies to Rework Only ]\nHow long intangibility lasts.\n-\n 2.0f = 2 seconds.\n");
            UUT_DStack = plugin.Config.Bind("Unstable Transmitter", "! Stack Duration !", 1.0f, "[ Applies to Rework Only ]\nHow long intangibility lasts when stacking.\n-\n 1.0f = +1.0 seconds.\n");
            UUT_Refresh = plugin.Config.Bind("Unstable Transmitter", "! Refresh Duration !", 60.0f, "[ Applies to Rework Only ]\nThe time needed for Unstable Transmitter to refresh.\n-\n 60.0f = 60 seconds.\n");
        }

        // War Bonds
        public static ConfigEntry<bool> GrowthNectarReworkEnabled;

        public static ConfigEntry<float> LGN_SBase;
        public static ConfigEntry<float> LGN_SStack;
        public static ConfigEntry<float> LGN_Charge;
        public static ConfigEntry<float> LGN_CBase;
        public static ConfigEntry<float> LGN_CStack;

        // War Bonds
        public static ConfigEntry<bool> WarBondsReworkEnabled;
        public static ConfigEntry<bool> WarBondsReplaceVFX;

        public static ConfigEntry<int> LWB_PBase;
        public static ConfigEntry<int> LWB_PStack;
        public static ConfigEntry<float> LWB_EBase;
        public static ConfigEntry<float> LWB_EStack;

        public static void LegendaryConfig(BaseUnityPlugin plugin)
        {
            GrowthNectarReworkEnabled = plugin.Config.Bind("Growth Nectar", "Enable Rework?", true, "Changes Growth Nectar to give stat boosts when equip is not on cooldown.");

            LGN_SBase = plugin.Config.Bind("Growth Nectar", "! Stats Base Stack !", 20f, "[ Applies to Rework Only ]\nHow much of ALL stats to increase.\n-\n 20.0 = 20% increase.\n");
            LGN_SStack = plugin.Config.Bind("Growth Nectar", "! Stats Stack !", 20f, "[ Applies to Rework Only ]\nHow much of ALL stats to increase per stack.\n-\n 20.0 = +20% increase per stack.\n");
            LGN_Charge = plugin.Config.Bind("Growth Nectar", "! Stats Per Charge !", 2f, "[ Applies to Rework Only ]\nHow much ALL stats increase per equip charge.\n-\n 2.0 = 2% per equip charge.\n");
            LGN_CBase = plugin.Config.Bind("Growth Nectar", "! Stats Base Stack Charge Max !", 10f, "[ Applies to Rework Only ]\nMax value of ALL stat increases from equip charges.\n-\n 10.0 = 10% maximum.\n");
            LGN_CStack = plugin.Config.Bind("Growth Nectar", "! Stats Stack Charge Max !", 10f, "[ Applies to Rework Only ]\nMax value of ALL stat increases from equip charges per stack.\n-\n 10.0 = +10% maximum per stack.\n");

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
        public static ConfigEntry<bool> StealthKitCleanse;

        public static void MiscConfig(BaseUnityPlugin plugin)
        {
            SkillDisableCleansable = plugin.Config.Bind("! Misc !", "Bens Raincoat Cleansing Skill Disable", true, "Lets Ben's Raincoat able to cleanse False Son's skill disable debuff.\nReally, any cleanse item works now.");
            StealthKitCleanse = plugin.Config.Bind("! Misc !", "Old War Stealth Kit Cleanse Effect", true, "Lets Old War Stealthkit cleanse debuffs when activating.\n2 (+1 per stack) debuffs cleansed.");
        }
    }
}
