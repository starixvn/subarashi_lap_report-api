using Fanex.Athena.Report.Common.Constants;
using Fanex.Athena.Report.Resources;
using System.Text.RegularExpressions;

namespace Fanex.Athena.Report.Common.Helpers
{
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
