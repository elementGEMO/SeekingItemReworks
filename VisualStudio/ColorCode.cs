namespace SeekingItemReworks
{
    public static class ColorCode
    {
        public enum Color
        {
            cStack,
            cIsDamage,
            cIsHealth,
            cIsUtility,
            cIsHealing
        };

        public static string Style(this string self, Color style)
        {
            return "<style=" + style + ">" + self + "</style>";
        }
    }
}
