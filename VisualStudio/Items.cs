using R2API;
using SeekingItemReworks;
using SeekingItemReworks.AllItems.Common;

namespace SeekerItems
{
    internal class Items
    {
        public static void Init()
        {
            Items.SetNewDesc(new WarpedEcho(), !MainConfig.WarpedEchoReworkEnabled.Value);
            Items.SetNewDesc(new ChronicExpansion());
            Items.SetNewDesc(new KnockbackFin(), MainConfig.CursedRewording.Value);
            Items.SetNewName(new KnockbackFin(), "Flying Fin", MainConfig.CursedRewording.Value, "Knockup Fin");
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