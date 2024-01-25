using Fanex.Athena.Report.Core.Constants;

namespace Fanex.Athena.Report.Core.Helpers
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
    }
}