namespace Fanex.Athena.Report.Common.Helpers
{
    using System.Text.RegularExpressions;
    using Constants;
    using Resources;

    public static class SabaHelper
    {
        public static bool IsHideSaba(int specialPlatform)
        {
            if (specialPlatform == SpecialPlatform.Jollysports || specialPlatform == SpecialPlatform.JollysportCV)
            {
                return true;
            }

            return false;
        }

        public static string RemoveSabaText(string text)
        {
            var sabaText = ViewResources.Saba.Trim();
            var result = Regex.Replace(text, sabaText, string.Empty, RegexOptions.IgnoreCase);
            // remove duplicate space
            return Regex.Replace(result, "  ", " ", RegexOptions.IgnoreCase);
        }
    }
}
