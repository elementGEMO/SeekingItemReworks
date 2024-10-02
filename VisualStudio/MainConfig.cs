using BepInEx;
using BepInEx.Configuration;

namespace SeekerItems
{
    public static class MainConfig
    {
        private static readonly string RoR2 = "RoR2 ";
        private static readonly string DLC1 = "SotV ";
        private static readonly string DLC2 = "SotS ";
        public static void SetUp(BaseUnityPlugin plugin)
        {
            GeneralConfig(plugin);
            CommonConfig(plugin);
            UncommonConfig(plugin);
            LegendaryConfig(plugin);
            EquipmentConfig(plugin);
        }

        public static void GeneralConfig(BaseUnityPlugin plugin)
        {
            string StaticName = "! General !";

            RoundNumber = plugin.Config.Bind(
                StaticName,
                "Item Stats Round", 0,
                "[ 0 = Whole | 1 = Tenths | 2 = Hundrenths | 3 = ... ]\n Rounds item values to respective decimal spot"
            );

            CursedRewrite = plugin.Config.Bind(
                StaticName,
                "Cursed Rewrite", false,
                "Enable for cursed item description rewrites"
            );
        }

        public static void CommonConfig(BaseUnityPlugin plugin)
        {
            // -- Seekers of the Storm Content -- \\

            // Warped Echo
            WarpedEcho.Rework = plugin.Config.Bind(
                DLC2 + WarpedEcho.StaticName,
                "Toggle Rework", 1,
                new ConfigDescription(
                    "[ 0 = Vanilla | 1 = Rework #1 ]",
                    new AcceptableValueRange<int>(0, 1)
                )
            );
            WarpedEcho.Delay_Base = plugin.Config.Bind(
                DLC2 + WarpedEcho.StaticName,
                "! Rework #1 - Base Delay !", 4f,
                "[ 4 = 4 Seconds ]\nBase delay duration for damage"
            );
            WarpedEcho.Instance_Stack = plugin.Config.Bind(
                DLC2 + WarpedEcho.StaticName,
                "! Rework #1 - Instance Stack !", 1,
                "[ 1 = +1 Instance ]\nInstances gained per stack"
            );

            // Chronic Expansion
            ChronicExpansion.Rework = plugin.Config.Bind(
                DLC2 + ChronicExpansion.StaticName,
                "Toggle Rework", 1,
                new ConfigDescription(
                    "[ 0 = Vanilla | 1 = Rework #1 ]",
                    new AcceptableValueRange<int>(0, 1)
                )
            );
            ChronicExpansion.TallyKills = plugin.Config.Bind(
                DLC2 + ChronicExpansion.StaticName,
                "Toggle Tally Display", true,
                "[ True = Display Counts on Kill Streak, not Buffs | False = Vanilla ]"
            );
            ChronicExpansion.Damage_Base = plugin.Config.Bind(
                DLC2 + ChronicExpansion.StaticName,
                "! Rework #1 - Base Damage !", 7.5f,
                "[ 7.5 = 7.5% Damage ]\nBase damage"
            );
            ChronicExpansion.Damage_Stack = plugin.Config.Bind(
                DLC2 + ChronicExpansion.StaticName,
                "! Rework #1 - Stack Damage !", 7.5f,
                "[ 7.5 = +7.5% Damage ]\nBase damage per stack"
            );

            // Knockback Fin
            KnockbackFin.Rework = plugin.Config.Bind(
                DLC2 + KnockbackFin.StaticName,
                "Toggle Rework", 1,
                new ConfigDescription(
                    "[ 0 = Vanilla | 1 = Rework #1 ]",
                    new AcceptableValueRange<int>(0, 1)
                )
            );
            KnockbackFin.DamageColor = plugin.Config.Bind(
                DLC2 + KnockbackFin.StaticName,
                "Toggle Damage Color", true,
                "[ True = Damage Color to Airborne | False = Vanilla ]"
            );
            KnockbackFin.IsHyperbolic = plugin.Config.Bind(
                DLC2 + KnockbackFin.StaticName,
                "! Rework #1 - Hyperbolic Scaling !", false,
                "[ True = Vanilla | False = Linear ]\nFormula for chance"
            );
            KnockbackFin.Chance_Base = plugin.Config.Bind(
                DLC2 + KnockbackFin.StaticName,
                "! Rework #1 - Base Chance !", 5f,
                "[ 5.0 = 5.0% Chance ]\nBase chance"
            );
            KnockbackFin.Chance_Stack = plugin.Config.Bind(
                DLC2 + KnockbackFin.StaticName,
                "! Rework #1 - Stack Chance !", 2.5f,
                "[ 2.5 = +2.5% Damage ]\nBase chance per stack"
            );
            KnockbackFin.Damage_Base = plugin.Config.Bind(
                DLC2 + KnockbackFin.StaticName,
                "! Rework #1 - Base Damage !", 10f,
                "[ 10.0 = 10.0% Damage ]\nBase damage"
            );
            KnockbackFin.Damage_Stack = plugin.Config.Bind(
                DLC2 + KnockbackFin.StaticName,
                "! Rework #1 - Stack Damage !", 10f,
                "[ 10.0 = +10.0% Damage ]\nBase damage per stack"
            );

            // Bolstering Lantern
            BolsteringLantern.Rework = plugin.Config.Bind(
                DLC2 + BolsteringLantern.StaticName,
                "Toggle Rework", 1,
                new ConfigDescription(
                    "[ 0 = Vanilla | 1 = Rework #1 | 2 = Rework #2 ]",
                    new AcceptableValueRange<int>(0, 2)
                )
            );
            BolsteringLantern.LowHealth = plugin.Config.Bind(
                DLC2 + BolsteringLantern.StaticName,
                "! Rework #1 - Low Health !", 50,
                new ConfigDescription(
                    "[ 50.0 = 50% Health ]\nLow health activate",
                    new AcceptableValueRange<int>(0, 100)
                )
            );
            BolsteringLantern.HighHealth = plugin.Config.Bind(
                DLC2 + BolsteringLantern.StaticName,
                "! Rework #1 - High Health !", 90,
                new ConfigDescription(
                    "[ 90.0 = 90% Health ]\nHigh health deactivate",
                    new AcceptableValueRange<int>(0, 100)
                )
            );
            BolsteringLantern.Chance_Base = plugin.Config.Bind(
                DLC2 + BolsteringLantern.StaticName,
                "! Rework #2 - Base Chance !", 3f,
                "[ 3.0 = 3.0% Chance ]\nBase chance"
            );
            BolsteringLantern.Chance_Stack = plugin.Config.Bind(
                DLC2 + BolsteringLantern.StaticName,
                "! Rework #2 - Stack Chance !", 3.0f,
                "[ 3.0 = +3.0% Damage ]\nBase chance per stack"
            );
            BolsteringLantern.Damage_Base = plugin.Config.Bind(
                DLC2 + BolsteringLantern.StaticName,
                "! Rework #1, #2 - Base Damage !", 20f,
                "[ 20.0 = 20.0% Damage ]\nBase damage"
            );
            BolsteringLantern.Damage_Stack = plugin.Config.Bind(
                DLC2 + BolsteringLantern.StaticName,
                "! Rework #1, #2 - Stack Damage !", 20f,
                "[ 20.0 = +20.0% Damage ]\nBase damage per stack"
            );

            // Antler Shield
            AntlerShield.Rework = plugin.Config.Bind(
                DLC2 + AntlerShield.StaticName,
                "Toggle Rework", 1,
                new ConfigDescription(
                    "[ 0 = Vanilla | 1 = Rework #1 ]",
                    new AcceptableValueRange<int>(0, 1)
                )
            );
            AntlerShield.Armor_Percent_Base = plugin.Config.Bind(
                DLC2 + AntlerShield.StaticName,
                "! Rework #1 - Armor Percent Base !", 20f,
                "[ 20.0 = 20.0% Armor from Speed ]\nArmor percent"
            );
            AntlerShield.Armor_Percent_Stack = plugin.Config.Bind(
                DLC2 + AntlerShield.StaticName,
                "! Rework #1 - Armor Percent Stack !", 20f,
                "[ 20.0 = +20.0% Armor from Speed ]\nArmor percent per stack"
            );
            AntlerShield.Movement_Base = plugin.Config.Bind(
                DLC2 + AntlerShield.StaticName,
                "! Rework #1 - Base Movement !", 7.5f,
                "[ 7.5 = 7.5% Movement ]\nBase damage"
            );
            AntlerShield.Movement_Stack = plugin.Config.Bind(
                DLC2 + AntlerShield.StaticName,
                "! Rework #1 - Stack Movement !", 7.5f,
                "[ 7.5 = +7.5% Movement ]\nBase damage per stack"
            );
        }

        public static void UncommonConfig(BaseUnityPlugin plugin)
        {
            // -- Seekers of the Storm Content -- \\

            // Chance Doll
            ChanceDoll.Rework = plugin.Config.Bind(
                DLC2 + ChanceDoll.StaticName,
                "Toggle Rework", 1,
                new ConfigDescription(
                    "[ 0 = Vanilla | 1 = Rework #1 | 2 = Rework #2 ]",
                    new AcceptableValueRange<int>(0, 2)
                )
            );
            ChanceDoll.Hidden_Chance = plugin.Config.Bind(
                DLC2 + ChanceDoll.StaticName,
                "! Rework #1 - Hidden Chance !", 10f,
                "[ 10.0 = 10% Chance ]\nChance for upgrade without failures"
            );
            ChanceDoll.Chance_Base = plugin.Config.Bind(
                DLC2 + ChanceDoll.StaticName,
                "! Rework #1 - Base Chance !", 20f,
                "[ 20.0 = 20% Chance ]\nChance for upgrade per shrine failure"
            );
            ChanceDoll.Chance_Stack = plugin.Config.Bind(
                DLC2 + ChanceDoll.StaticName,
                "! Rework #1 - Stack Chance !", 5f,
                "[ 5.0 = +5% Chance ]\nMore chance for upgrade per shrine failure, per stack"
            );
            ChanceDoll.Karma_Required = plugin.Config.Bind(
                DLC2 + ChanceDoll.StaticName,
                "! Rework #2 - Interacts Required !", 15,
                "[ 5.0 = 5 Shrines ]\nHow much Shrines to count as X luck"
            );

            // Sale Star
            SaleStar.Rework = plugin.Config.Bind(
                DLC2 + SaleStar.StaticName,
                "Toggle Rework", 1,
                new ConfigDescription(
                    "[ 0 = Vanilla | 1 = Rework #1 ]",
                    new AcceptableValueRange<int>(0, 1)
                )
            );
            SaleStar.IsHyperbolic = plugin.Config.Bind(
                DLC2 + SaleStar.StaticName,
                "! Rework #1 - Hyperbolic Scaling !", true,
                "[ True = Hyperbolic | False = Linear ]\nFormula for chance"
            );
            SaleStar.Consume_Base = plugin.Config.Bind(
                DLC2 + SaleStar.StaticName,
                "! Rework #1 - Base Chance !", 100f,
                "[ 100.0 = 100% Chance ]\nChance for consume"
            );
            SaleStar.Consume_Stack = plugin.Config.Bind(
                DLC2 + SaleStar.StaticName,
                "! Rework #1 - Stack Chance !", 7.5f,
                "[ 7.5 = -7.5% Chance ]\nLess chance for consume per stack"
            );

            // Unstable Transmitter
            UnstableTransmitter.Rework = plugin.Config.Bind(
                DLC2 + UnstableTransmitter.StaticName,
                "Toggle Rework", 1,
                new ConfigDescription(
                    "[ 0 = Vanilla | 1 = Rework #1 ]",
                    new AcceptableValueRange<int>(0, 1)
                )
            );
            UnstableTransmitter.IsFloat = plugin.Config.Bind(
                DLC2 + UnstableTransmitter.StaticName,
                "! Rework #1 - Flight !", true,
                "[ True = Fly | False = Movement ]\nHeretic flight during intangibility"
            );
            UnstableTransmitter.LowHealth = plugin.Config.Bind(
                DLC2 + UnstableTransmitter.StaticName,
                "! Rework #1 - Low Health !", 25,
                new ConfigDescription(
                    "[ 25.0 = 25% Health ]\nLow health activate",
                    new AcceptableValueRange<int>(0, 100)
                )
            );
            UnstableTransmitter.Refresh = plugin.Config.Bind(
                DLC2 + UnstableTransmitter.StaticName,
                "! Rework #1 - Refresh !", 60f,
                "[ 60.0 = 60 Seconds ]\nRefresh timer"
            );
            UnstableTransmitter.Range = plugin.Config.Bind(
                DLC2 + UnstableTransmitter.StaticName,
                "! Rework #1 - Range !", 30f,
                "[ 30.0 = 30 Meters ]\nArea effect"
            );
            UnstableTransmitter.Damage_Base = plugin.Config.Bind(
                DLC2 + UnstableTransmitter.StaticName,
                "! Rework #1 - Base Damage !", 150f,
                "[ 150.0 = 150% Base Damage ]\nBleed base damage"
            );
            UnstableTransmitter.Damage_Stack = plugin.Config.Bind(
                DLC2 + UnstableTransmitter.StaticName,
                "! Rework #1 - Stack Damage !", 150f,
                "[ 150.0 = +150% Base Damage ]\nBleed base damage per stack"
            );
            UnstableTransmitter.Duration_Base = plugin.Config.Bind(
                DLC2 + UnstableTransmitter.StaticName,
                "! Rework #1 - Base Duration !", 2f,
                "[ 2.0 = 2 Seconds ]\nEffect duration"
            );
            UnstableTransmitter.Duration_Stack = plugin.Config.Bind(
                DLC2 + UnstableTransmitter.StaticName,
                "! Rework #1 - Stack Duration !", 1f,
                "[ 1.0 = +1 Second ]\nEffect duration per stack"
            );

            // Noxious Thorn
            NoxiousThorn.Rework = plugin.Config.Bind(
                DLC2 + NoxiousThorn.StaticName,
                "Toggle Rework", 1,
                new ConfigDescription(
                    "[ 0 = Vanilla | 1 = Rework #1 ]",
                    new AcceptableValueRange<int>(0, 1)
                )
            );
            NoxiousThorn.Inflict_Type = plugin.Config.Bind(
                DLC2 + NoxiousThorn.StaticName,
                "! Rework #1 - Type of Damage !", 2,
                new ConfigDescription(
                    "[ 0 = Bleed | 1 = Poison | 2 = Blight ]",
                    new AcceptableValueRange<int>(0, 2)
                )
            );
            NoxiousThorn.Refresh = plugin.Config.Bind(
                DLC2 + NoxiousThorn.StaticName,
                "! Rework #1 - Refresh !", 6f,
                "[ 6.0 = 6 Seconds ]\nTime until refresh"
            );
            NoxiousThorn.Damage_Frequency = plugin.Config.Bind(
                DLC2 + NoxiousThorn.StaticName,
                "! Rework #1 - Frequency !", 0.6f,
                "[ 1.0 = 1 Second ]\nDoT frequency"
            );
            NoxiousThorn.Damage_Base = plugin.Config.Bind(
                DLC2 + NoxiousThorn.StaticName,
                "! Rework #1 - Base Damage !", 150f,
                "[ 120.0 = 120% Damage ]\nDoT damage"
            );
            NoxiousThorn.Damage_Stack = plugin.Config.Bind(
                DLC2 + NoxiousThorn.StaticName,
                "! Rework #1 - Stack Damage !", 150f,
                "[ 120.0 = +120% Damage ]\nDoT damage per stack"
            );
            NoxiousThorn.Initial_Range = plugin.Config.Bind(
                DLC2 + NoxiousThorn.StaticName,
                "! Rework #1 - Initial Radius !", 3.5f,
                "[ 3.5 = 3.5m ]\nRing radius on first hit"
            );
            NoxiousThorn.Range_Increment = plugin.Config.Bind(
                DLC2 + NoxiousThorn.StaticName,
                "! Rework #1 - Radius Increment !", 1.5f,
                "[ 1.5 = 1.5m ]\nRing radius per hit"
            );
            NoxiousThorn.Base_Cap = plugin.Config.Bind(
                DLC2 + NoxiousThorn.StaticName,
                "! Rework #1 - Base radius Cap !", 10f,
                "[ 10.0 = 10m ]\nMax ring radius"
            );
            NoxiousThorn.Stack_Cap = plugin.Config.Bind(
                DLC2 + NoxiousThorn.StaticName,
                "! Rework #1 - Stack radius Cap !", 4f,
                "[ 4.0 = +4m ]\nMore max ring radius per stack"
            );

            // -- Risk of Rain 2 Content -- \\

            // Old War Stealthkit
            OldWarStealthKit.Rework = plugin.Config.Bind(
                RoR2 + OldWarStealthKit.StaticName,
                "Toggle Rework", 1,
                new ConfigDescription(
                    "[ 0 = Vanilla | 1 = Rework #1 ]",
                    new AcceptableValueRange<int>(0, 1)
                )
            );
        }
        public static void LegendaryConfig(BaseUnityPlugin plugin)
        {
            // -- Seekers of the Storm Content -- \\

            // War Bonds
            WarBonds.Rework = plugin.Config.Bind(
                DLC2 + WarBonds.StaticName,
                "Toggle Rework", 1,
                new ConfigDescription(
                    "[ 0 = Vanilla | 1 = Rework #1 ]",
                    new AcceptableValueRange<int>(0, 1)
                )
            );
            WarBonds.Purchase_Base = plugin.Config.Bind(
                DLC2 + WarBonds.StaticName,
                "! Rework #1 - Purchase Base !", 3,
                "[ 3 = 3 Purchases ]\nBase purchases"
            );
            WarBonds.Purchase_Stack = plugin.Config.Bind(
                DLC2 + WarBonds.StaticName,
                "! Rework #1 - Purchase Stack !", 2,
                "[ 2 = +2 Purchases ]\nPurchases gained per stack"
            );
            WarBonds.Experience_Percent_Base = plugin.Config.Bind(
                DLC2 + WarBonds.StaticName,
                "! Rework #1 - Base Experience Percent !", 50f,
                "[ 50.0 = 50% Experience ]\nExperience percent gain"
            );
            WarBonds.Experience_Percent_Stack = plugin.Config.Bind(
                DLC2 + WarBonds.StaticName,
                "! Rework #1 - Stack Experience Percent !", 25f,
                "[ 25.0 = +25% Experience ]\nMore experience percent gain per stack"
            );

            // Growth Nectar
            GrowthNectar.Rework = plugin.Config.Bind(
                DLC2 + GrowthNectar.StaticName,
                "Toggle Rework", 1,
                new ConfigDescription(
                    "[ 0 = Vanilla | 1 = Rework #1 ]",
                    new AcceptableValueRange<int>(0, 1)
                )
            );
            GrowthNectar.Stat_Base = plugin.Config.Bind(
                DLC2 + GrowthNectar.StaticName,
                "! Rework #1 - Base Stats !", 15f,
                "[ 15.0 = 15% Stats ]\nALL stats base"
            );
            GrowthNectar.Stat_Stack = plugin.Config.Bind(
                DLC2 + GrowthNectar.StaticName,
                "! Rework #1 - Stack Stats !", 10f,
                "[ 10.0 = +10% Stats ]\nIncrease ALL stats per stack"
            );
            GrowthNectar.Charge_Stat_Increase = plugin.Config.Bind(
                DLC2 + GrowthNectar.StaticName,
                "! Rework #1 - Charge Stat Increase !", 2f,
                "[ 2.0 = 2% Stats per Charge ]\nIncrease ALL stats per equipment charge"
            );
            GrowthNectar.Charge_Cap_Base = plugin.Config.Bind(
                DLC2 + GrowthNectar.StaticName,
                "! Rework #1 - Base Charge Cap !", 10f,
                "[ 10.0 = 10% Experience ]\nMax stats per equipment charge"
            );
            GrowthNectar.Charge_Cap_Stack = plugin.Config.Bind(
                DLC2 + GrowthNectar.StaticName,
                "! Rework #1 - Stack Charge Cap !", 10f,
                "[ 10.0 = +10% Experience ]\nMore max stats per equipment charge per stack"
            );

            // -- Survivors of the Void Content -- \\

            // Ben's Raincoat
            BensRaincoat.Rework = plugin.Config.Bind(
                DLC1 + BensRaincoat.StaticName,
                "Toggle Rework", 1,
                new ConfigDescription(
                    "[ 0 = Vanilla | 1 = Rework #1 ]\nNot really a rework. Lets False Son's skill disable allowed to be cleansed.",
                    new AcceptableValueRange<int>(0, 1)
                )
            );
        }

        public static void EquipmentConfig(BaseUnityPlugin plugin)
        {
            // -- Seekers of the Storm Content -- \\

            // Seed of Life
            SeedOfLife.Rework = plugin.Config.Bind(
                DLC2 + SeedOfLife.StaticName,
                "Toggle Rewrite", 1,
                new ConfigDescription(
                    "[ 0 = Vanilla | 1 = Rewrite ]",
                    new AcceptableValueRange<int>(0, 1)
                )
            );
        }

        public static ConfigEntry<int> RoundNumber;
        public static ConfigEntry<bool> CursedRewrite;
    }
}
