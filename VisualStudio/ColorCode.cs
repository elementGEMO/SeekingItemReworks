namespace SeekingItemReworks
{
    public static class ColorCode
    {
        public enum FontColor
        {
            cStack,
            cIsDamage,
            cIsHealth,
            cIsUtility,
            cIsHealing
        };

        public static string Style(this string self, FontColor style) => $"<style={style}>{self}</style>";
    }
}
