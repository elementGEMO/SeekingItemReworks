namespace SeekerItems
{
    internal class RiskItem
    {
        public static int roundVal = MainConfig.RoundNumber.Value;

        private readonly string ItemInternal;
        private string ItemInfo;
        private string ItemDesc;
        private string ItemInfoAlt;
        private string ItemDescAlt;

        public RiskItem(string itemInternal, string itemInfo, string itemDesc, string itemInfoAlt = "", string itemDescAlt = "")
        {
            ItemInternal = itemInternal;
            ItemInfo = itemInfo;
            ItemDesc = itemDesc;
            ItemInfoAlt = itemInfoAlt;
            ItemDescAlt = itemDescAlt;
        }

        public string GetName()
        {
            return ItemInternal;
        }
        public string GetInfo(bool isAlt)
        {
            if (!isAlt)
            {
                return ItemInfo;
            }
            else
            {
                return ItemInfoAlt;
            }
        }
        public string GetDesc(bool isAlt)
        {
            if (!isAlt)
            {
                return ItemDesc;
            }
            else
            {
                return ItemDescAlt;
            }
        }
    }
}
