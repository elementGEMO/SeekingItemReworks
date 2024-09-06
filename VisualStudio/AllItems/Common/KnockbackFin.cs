using System;
using static SeekingItemReworks.ColorCode;

namespace SeekingItemReworks.AllItems.Common
{
    internal class KnockbackFin : RiskItem
    {
        //static readonly string descOne = $"{MainConfig.CWE_Delay.Value.ToString($"F{MainConfig.RoundNumber.Value}")}";
        //static readonly string descTwo = $"{MainConfig.CWE_Stack.Value.ToString($"F{MainConfig.RoundNumber.Value}")}";
        public KnockbackFin() : base(
            "KNOCKBACKHITENEMIES",
            "Chance on hit to knock enemies into the air.\nDeal bonus damage to airborne enemies.",
            "5% (+2% per stack) chance on hit to knock enemies into the air. Deal +10% (+10% per stack) damage to airborne enemies.",
            "Chance on hit to knock up enemies.\nDeal bonus damage to airborne enemies.",
            "5% (+2% per stack) chance on hit to knock up enemies. Deal +10% (+10% per stack) damage to airborne enemies."
        ) { }
    }
}
