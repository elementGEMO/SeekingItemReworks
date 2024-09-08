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
            if (MainConfig.WarpedEchoReworkEnabled.Value || MainConfig.WarpedEchoFixEnabled.Value) { Items.SetNewDesc(new WarpedEcho(), !MainConfig.WarpedEchoReworkEnabled.Value); };
            if (MainConfig.ChronicExpansionReworkEnabled.Value) { Items.SetNewDesc(new ChronicExpansion()); }
            if (MainConfig.KnockbackFinReworkEnabled.Value)
            { 
                Items.SetNewName(new KnockbackFin(), "Flying Fin", MainConfig.CursedRewording.Value, "Knockup Fin");
                Items.SetNewName(new KnockbackFin(), "Flying Fin", MainConfig.CursedRewording.Value, "Knockup Fin");
            }

            // Uncommon
            if (MainConfig.UnstableTransmitterReworkEnabled.Value) { Items.SetNewDesc(new UnstableTransmitter()); }

            // Legendary
            if (MainConfig.WarBondsReworkEnabled.Value) { Items.SetNewDesc(new WarBonds()); }

            // Equipment
            if (MainConfig.SeedOfLifeRewriteEnabled.Value) { Items.SetNewDesc(new SeedOfLife(), false, "EQUIPMENT_"); }
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