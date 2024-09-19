using R2API;

namespace SeekerItems
{
    public class ItemInfo
    {
        public ItemInfo()
        {
            // -- Seekers of the Storm Content -- \\

            // Common Tier Items
            if (WarpedEcho.Rework.Value == 1) { SetDesc(new WarpedEcho(WarpedEcho.Rework.Value)); }
            if (ChronicExpansion.Rework.Value == 1) { SetDesc(new ChronicExpansion(ChronicExpansion.Rework.Value)); }
            if (KnockbackFin.Rework.Value == 1 && MainConfig.CursedRewrite.Value == false)
            { 
                SetDesc(new KnockbackFin(KnockbackFin.Rework.Value));
                SetName(new KnockbackFin(KnockbackFin.Rework.Value), "Flying Fin");
            }
            if (KnockbackFin.Rework.Value == 1 && MainConfig.CursedRewrite.Value == true)
            {
                SetDesc(new KnockbackFin(KnockbackFin.Rework.Value * -1));
                SetName(new KnockbackFin(KnockbackFin.Rework.Value * -1), "Knockup Fin");
            }
            if (BolsteringLantern.Rework.Value == 1) { SetDesc(new BolsteringLantern(BolsteringLantern.Rework.Value)); }
            if (BolsteringLantern.Rework.Value == 2) { SetDesc(new BolsteringLantern(BolsteringLantern.Rework.Value)); }
            if (AntlerShield.Rework.Value == 1) { SetDesc(new AntlerShield(AntlerShield.Rework.Value)); }

            // Uncommon Tier Items
            if (SaleStar.Rework.Value == 1) { SetDesc(new SaleStar(SaleStar.Rework.Value)); }
            if (UnstableTransmitter.Rework.Value == 1) { SetDesc(new UnstableTransmitter(UnstableTransmitter.Rework.Value)); }
            if (NoxiousThorn.Rework.Value == 1) { SetDesc(new NoxiousThorn(NoxiousThorn.Rework.Value)); }

            // Legendary Tier Items
            if (WarBonds.Rework.Value == 1) { SetDesc(new WarBonds(WarBonds.Rework.Value)); }
            if (GrowthNectar.Rework.Value == 1) { SetDesc(new GrowthNectar(GrowthNectar.Rework.Value)); }

            // Equipment
            if (SeedOfLife.Rework.Value == 1) { SetDesc(new SeedOfLife(SeedOfLife.Rework.Value), "EQUIPMENT_"); }

            // -- Seekers of the Storm Content -- \\

            // Uncommon Tier Items
            if (OldWarStealthKit.Rework.Value == 1) { SetDesc(new OldWarStealthKit(OldWarStealthKit.Rework.Value)); }
        }
        private static void SetDesc(ItemTemplate ItemInfo, string forcedPrefix = "ITEM_")
        {
            LanguageAPI.Add(forcedPrefix + ItemInfo.ItemInternal + "_PICKUP", ItemInfo.ItemInfo);
            LanguageAPI.Add(forcedPrefix + ItemInfo.ItemInternal + "_DESC", ItemInfo.ItemDesc);
        }

        private static void SetName(ItemTemplate ItemInfo, string name, string forcedPrefix = "ITEM_")
        {
            LanguageAPI.Add(forcedPrefix + ItemInfo.ItemInternal + "_NAME", name);
        }
        /*
        public static void Init()
        {
            // Common
            if (MainConfig.WarpedEchoReworkEnabled.Value) { SetNewDesc(new WarpedEcho()); };
            if (MainConfig.ChronicExpansionReworkEnabled.Value) { SetNewDesc(new ChronicExpansion()); }
            if (MainConfig.KnockbackFinReworkEnabled.Value)
            {
                SetNewDesc(new KnockbackFin(), MainConfig.CursedRewording.Value);
                SetNewName(new KnockbackFin(), "Flying Fin", MainConfig.CursedRewording.Value, "Knockup Fin");
            }
            if (MainConfig.BolsteringLanternReworkEnabled.Value || MainConfig.BolsteringLanternAltReworkEnabled.Value)
            {
                SetNewDesc(new BolsteringLantern(), MainConfig.BolsteringLanternAltReworkEnabled.Value);
                SetNewName(new BolsteringLantern(), "Smoldering Lantern");
            }
            if (MainConfig.AntlerShieldReworkEnabled.Value) { SetNewDesc(new AntlerShield()); }

            // Uncommon
            if (MainConfig.UnstableTransmitterReworkEnabled.Value) { SetNewDesc(new UnstableTransmitter()); }
            if (MainConfig.SaleStarReworkEnabled.Value) { SetNewDesc(new SaleStar()); }

            // Legendary
            if (MainConfig.WarBondsReworkEnabled.Value) { SetNewDesc(new WarBonds()); }
            if (MainConfig.GrowthNectarReworkEnabled.Value) { SetNewDesc(new GrowthNectar()); }

            // Equipment
            if (MainConfig.SeedOfLifeRewriteEnabled.Value) { SetNewDesc(new SeedOfLife(), false, "EQUIPMENT_"); }

            MiscInit();
        }
        */
        /*

        private static void MiscInit()
        {
            if (MainConfig.StealthKitCleanse.Value) { SetNewDesc(new OldWarStealthKit()); }
        }

        public static void SetNewDesc(RiskItem ItemInfo, bool doAlt = false, string forcedPrefix = "ITEM_")
        {
            LanguageAPI.Add(forcedPrefix + ItemInfo.GetName() + "_PICKUP", ItemInfo.GetInfo(doAlt));
            LanguageAPI.Add(forcedPrefix + ItemInfo.GetName() + "_DESC", ItemInfo.GetDesc(doAlt));
        }

        public static void SetNewName(RiskItem ItemInfo, string newName, bool doAlt = false, string altName = "")
        {
            if (!doAlt)
            {
                LanguageAPI.Add("ITEM_" + ItemInfo.GetName() + "_NAME", newName);
            }
            else
            {
                LanguageAPI.Add("ITEM_" + ItemInfo.GetName() + "_NAME", altName);
            }
        }
        */
    }
    internal class ItemTemplate
    {
        public static int roundVal = MainConfig.RoundNumber.Value;

        public string ItemInternal;
        public string ItemInfo;
        public string ItemDesc;
    }
}