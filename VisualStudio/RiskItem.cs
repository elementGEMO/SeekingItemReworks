using System;
using System.Collections.Generic;
using System.Text;

namespace SeekingItemReworks
{
    internal class RiskItem
    {
        public string ItemInternal;
        public string ItemInfo;
        public string ItemDesc;
        public string ItemInfoAlt;
        public string ItemDescAlt;

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
