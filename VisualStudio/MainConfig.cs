using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeekingItemReworks
{
    public static class MainConfig
    {
        public static void SetUpConfigs(BaseUnityPlugin plugin)
        {
            CommonConfig(plugin);
        }

        // General Config
        public static ConfigEntry<int> RoundNumber;
        public static ConfigEntry<bool> CursedRewording;

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

        public static void CommonConfig(BaseUnityPlugin plugin)
        {
            RoundNumber = plugin.Config.Bind("! General !", "Item Description Round", 0, "Decides what decimal spot to round to.\n- 0 = Whole, 1 = Tenth, 2 = Hundredths, 3 = ...\n");
            CursedRewording = plugin.Config.Bind("! General !", "Cursed Wording", false, "Makes some item descriptions... a little cursed.\n");

            WarpedEchoReworkEnabled = plugin.Config.Bind("Warped Echo", "Enable Rework?", true, "Changes Warped Echo to only split damage one at a time, stacking delay duration. Forces bug fixes internally.");
            WarpedEchoFixEnabled = plugin.Config.Bind("Warped Echo", "Enable Bug Fixes?", true, "Prevents Warped Echo from splitting lethal damage.");

            CWE_Cap = plugin.Config.Bind("Warped Echo", "! Damage Cap !", 1.0f, "[ Applies to Rework & Bugfix ]\nThe percentage at which damage can't exceed the current health.\n - 1.0 = Damage can't be over 100% of health.\n");
            CWE_Delay = plugin.Config.Bind("Warped Echo", "! Base Delay !", 3.0f, "[ Applies to Rework Only ]\nThe base time for delayed damage triggering.\n- 3.0 = Damage delayed for 3 seconds.\n");
            CWE_Stack = plugin.Config.Bind("Warped Echo", "! Stack Delay !", 1.5f, "[ Applies to Rework Only ]\nThe additional time for delayed damage triggering when stacking.\n- 1.5 = Damage delayed for +1.5 seconds with each stack.\n");
            
            ChronicExpansionReworkEnabled = plugin.Config.Bind("Chronic Expansion", "Enable Rework?", true, "Changes Chronic Expansion by slightly nerfing the values and replacing current stacking with a proper formula.");

            CCE_Base = plugin.Config.Bind("Chronic Expansion", "! Base Stack !", 7.0f, "[ Applies to Rework Only ]\nThe base percentage damage for one buff.\n- 3.0 = Damage delayed for 3 seconds.\n");
            CCE_Stack = plugin.Config.Bind("Chronic Expansion", "! Stack  !", 5.0f, "[ Applies to Rework Only ]\nThe additional percentage damage for one buff when stacking.\n- 1.5 = Damage delayed for +1.5 seconds with each stack.\n");

            KnockbackFinReworkEnabled = plugin.Config.Bind("Knockback Fin", "Enable Rework?", true, "Changes Knockback Fin by reducing knock chances, and adds extra damage to airborne enemies.");
        }
    }
}
