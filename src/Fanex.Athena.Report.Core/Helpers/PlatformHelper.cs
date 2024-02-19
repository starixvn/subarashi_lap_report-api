using System.Collections.Generic;
using Fanex.Athena.Report.Core.Constants;
using Fanex.Athena.Report.Resources;
using Fanex.Athena.Report.Resources.Constants;

namespace Fanex.Athena.Report.Core.Helpers
{
    public static class PlatformHelper
    {
        private static IDictionary<int, string> NormalPlatform => new Dictionary<int, string>
        {
            { PlatformType.DesktopNewAsia, ViewResources.DesktopNewAsia },
            { PlatformType.DesktopChina, ViewResources.DesktopChina },
            { PlatformType.DesktopOthers, ViewResources.DesktopOthers },
            { PlatformType.DesktopGalaxy, ViewResources.DesktopGalaxy },
            { PlatformType.MobileStandard, ViewResources.MobileStandard },
            { PlatformType.MobileEuro, ViewResources.MobileEuro },
            { PlatformType.MobileGalaxy, ViewResources.MobileGalaxy },
            { PlatformType.MobileCompact, ViewResources.MobileCompact },
            { PlatformType.MobileOthers, ViewResources.MobileOthers },
            { PlatformType.ESportsDesktop, ViewResources.ESportsDesktop },
            { PlatformType.ESportsMobile, ViewResources.ESportsMobile },
            { PlatformType.ESportsApp, ViewResources.ESportsApp },
            { PlatformType.MobileApp, ViewResources.MobileApp },
            { PlatformType.NativeApp, ViewResources.NativeApp },
            { PlatformType.Tablet, ViewResources.Tablet },
            { PlatformType.DeepLink, ViewResources.DeepLink },
            { PlatformType.WAP, ViewResources.Wap },
            { PlatformType.ChatRoom, ViewResources.ChatRoom },
            { PlatformType.BetfromOthers, ViewResources.BetfromOthers }
        };

        private static IDictionary<int, string> JollysportsPlatform => new Dictionary<int, string>
        {
            { PlatformType.Desktop, ViewResources.Desktop },
            { PlatformType.Mobile, ViewResources.Mobile },
            { PlatformType.Others, ViewResources.BetfromOthers }
        };

        public static IDictionary<int, string> GetPlatformList(int specialPlatform)
        {
            if (specialPlatform == SpecialPlatform.Jollysports || specialPlatform == SpecialPlatform.JollysportCV)
            {
                return JollysportsPlatform;
            }

            return NormalPlatform;
        }
    }
}
