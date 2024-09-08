using System;
using static SeekingItemReworks.ColorCode;

namespace SeekerItems.Equipment
{
    internal class SeedOfLife : RiskItem
    {
        public SeedOfLife() : base(
            "HEALANDREVIVE",
            "Revives user or allies. Consume on use.",
            "Returns ".Style(Color.cIsHealing) + "the user " + "to life ".Style(Color.cIsHealing) + "upon death ".Style(Color.cIsUtility) + "or dead allies on use. Equipment is " + "consumed ".Style(Color.cIsUtility) + "on use."
        ) {} 
    }
}
