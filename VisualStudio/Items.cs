using R2API;
using SeekerItems.Common;
using SeekerItems.Legendary;
using SeekerItems.Equipment;
using SeekerItems.Uncommon;

namespace SeekerItems
{
    internal class Items
    {
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
            if (MainConfig.AntlerShieldReworkEnabled.Value) { SetNewDesc(new AntlerShield()); }

            // Uncommon
            if (MainConfig.UnstableTransmitterReworkEnabled.Value) { SetNewDesc(new UnstableTransmitter()); }

            // Legendary
            if (MainConfig.WarBondsReworkEnabled.Value) { SetNewDesc(new WarBonds()); }
            if (MainConfig.GrowthNectarReworkEnabled.Value) { SetNewDesc(new GrowthNectar()); }

            // Equipment
            if (MainConfig.SeedOfLifeRewriteEnabled.Value) { SetNewDesc(new SeedOfLife(), false, "EQUIPMENT_"); }
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
    }
}