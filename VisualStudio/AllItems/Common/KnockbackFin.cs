using System;
using static SeekingItemReworks.ColorCode;

namespace SeekerItems.Common
{
    internal class KnockbackFin : RiskItem
    {
        public KnockbackFin() : base(
            "KNOCKBACKHITENEMIES",
            "Chance on hit to knock enemies into the air.\nDeal bonus damage to airborne enemies.",
            string.Format("{0}% ".Style(Color.cIsUtility) + "(+{1}% per stack) ".Style(Color.cStack) + "chance on hit to " + "knock enemies into the air".Style(Color.cIsUtility) + ". Deal " + "+{2}% ".Style(Color.cIsDamage) + "(+{3}% per stack) ".Style(Color.cStack) + "damage to " + "airborne ".Style(Color.cIsUtility) + "enemies.",
                Math.Round(MainConfig.CKF_KBase.Value, roundVal), Math.Round(MainConfig.CKF_KStack.Value, roundVal), Math.Round(MainConfig.CKF_DBase.Value, roundVal), Math.Round(MainConfig.CKF_DStack.Value, roundVal)),

            "Chance on hit to knock up enemies.\nDeal bonus damage to airborne enemies.",
            string.Format("{0}% ".Style(Color.cIsUtility) + "(+{1}% per stack) ".Style(Color.cStack) + "chance on hit to " + "knock up enemies".Style(Color.cIsUtility) + ". Deal " + "+{2}% ".Style(Color.cIsDamage) + "(+{3}% per stack) ".Style(Color.cStack) + "damage to " + "airborne ".Style(Color.cIsUtility) + "enemies.",
                Math.Round(MainConfig.CKF_KBase.Value, roundVal), Math.Round(MainConfig.CKF_KStack.Value, roundVal), Math.Round(MainConfig.CKF_DBase.Value, roundVal), Math.Round(MainConfig.CKF_DStack.Value, roundVal))
        ) { }
    }
}
