using static SeekingItemReworks.ColorCode;

namespace SeekingItemReworks.AllItems.Common
{
    internal class ChronicExpansion : RiskItem
    {
        static readonly string descOne = $"{MainConfig.CCE_Base.Value.ToString($"F{MainConfig.RoundNumber.Value}")}";
        static readonly string descTwo = $"{MainConfig.CCE_Stack.Value.ToString($"F{MainConfig.RoundNumber.Value}")}";
        public ChronicExpansion() : base(
            "INCREASEDAMAGEONMULTIKILL",
            "Gain stacking damage after killing 5 enemies while in combat.",
            "Killing 5 enemies ".Style(Color.cIsDamage) + "in combat buffs damage by " + (descOne + "% ").Style(Color.cIsDamage) + ("(+" + descTwo + "% per stack) ").Style(Color.cStack) + "temporarily each time, until leaving combat." 
        ) {} 
    }
}
