using R2API;
using System;

namespace SeekerItems
{
    public static class ItemInfo
    {
        public static void SetUp()
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
            if (ChanceDoll.Rework.Value == 1) { SetDesc(new ChanceDoll(ChanceDoll.Rework.Value)); }
            if (ChanceDoll.Rework.Value == 2)
            {
                SetDesc(new ChanceDoll(ChanceDoll.Rework.Value));
                SetName(new ChanceDoll(ChanceDoll.Rework.Value), "Karma Doll");
            }
            if (SaleStar.Rework.Value == 1) { SetDesc(new SaleStar(SaleStar.Rework.Value)); }
            if (UnstableTransmitter.Rework.Value == 1) { SetDesc(new UnstableTransmitter(UnstableTransmitter.Rework.Value)); }
            if (NoxiousThorn.Rework.Value == 1) { SetDesc(new NoxiousThorn(NoxiousThorn.Rework.Value)); }

            // -- Risk of Rain 2 Content -- \\

            // Uncommon Tier Items
            if (OldWarStealthKit.Rework.Value == 1) { SetDesc(new OldWarStealthKit(OldWarStealthKit.Rework.Value)); }

            /*
            // Legendary Tier Items
            if (WarBonds.Rework.Value == 1) { SetDesc(new WarBonds(WarBonds.Rework.Value)); }
            if (GrowthNectar.Rework.Value == 1) { SetDesc(new GrowthNectar(GrowthNectar.Rework.Value)); }

            // Equipment
            if (SeedOfLife.Rework.Value == 1) { SetDesc(new SeedOfLife(SeedOfLife.Rework.Value), "EQUIPMENT_"); }
            */
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
    }
    internal class ItemTemplate
    {
        public string ItemInternal;
        public string ItemInfo;
        public string ItemDesc;

        public static float RoundVal(float value) => MathF.Round(value, MainConfig.RoundNumber.Value);
    }
}