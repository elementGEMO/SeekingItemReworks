using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UIElements;

namespace SeekingItemReworks
{
    public static class ColorCode
    {
        public enum Color
        {
            cStack,
            cIsDamage,
            cIsHealth,
            cIsUtility
        };

        public static string Style(this string self, Color style)
        {
            return "<style=" + style + ">" + self + "</style>";
        }
    }
}
