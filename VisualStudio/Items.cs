using R2API;
using SeekerItems.Common;
using SeekerItems.Legendary;

namespace SeekerItems
{
    internal class Items
    {
        public static void Init()
        {
            // Common
            Items.SetNewDesc(new WarpedEcho(), !MainConfig.WarpedEchoReworkEnabled.Value);
            Items.SetNewDesc(new ChronicExpansion());
            Items.SetNewDesc(new KnockbackFin(), MainConfig.CursedRewording.Value);
            Items.SetNewName(new KnockbackFin(), "Flying Fin", MainConfig.CursedRewording.Value, "Knockup Fin");

            // Legendary
            Items.SetNewDesc(new WarBonds());
        }

        public static void SetNewDesc(RiskItem ItemInfo, bool doAlt = false)
        {
            LanguageAPI.Add("ITEM_" + ItemInfo.GetName() + "_PICKUP", ItemInfo.GetInfo(doAlt));
            LanguageAPI.Add("ITEM_" + ItemInfo.GetName() + "_DESC", ItemInfo.GetDesc(doAlt));
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