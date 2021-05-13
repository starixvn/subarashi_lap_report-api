namespace Fanex.Athena.Report.Resources.Helpers
{
    using Fanex.Athena.Report.Resources.Constants;

    public static class DataHelper
    {
#pragma warning disable S1541 // Methods and properties should not be too complex
        public static string GetPlatformName(int type)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            switch (type)
            {
                case PlatformType.DesktopNewAsia: return ViewResources.DesktopNewAsia;
                case PlatformType.DesktopChina: return ViewResources.DesktopChina;
                case PlatformType.DesktopOthers: return ViewResources.DesktopOthers;
                case PlatformType.MobileStandard: return ViewResources.MobileStandard;
                case PlatformType.MobileEuro: return ViewResources.MobileEuro;
                case PlatformType.MobileCompact: return ViewResources.MobileCompact;
                case PlatformType.MobileOthers: return ViewResources.MobileOthers;
                case PlatformType.ESportsDesktop: return ViewResources.ESportsDesktop;
                case PlatformType.ESportsMobile: return ViewResources.ESportsMobile;
                case PlatformType.ESportsApp: return ViewResources.ESportsApp;
                case PlatformType.MobileApp: return ViewResources.MobileApp;
                case PlatformType.NativeApp: return ViewResources.NativeApp;
                case PlatformType.Tablet: return ViewResources.Tablet;
                case PlatformType.DeepLink: return ViewResources.DeepLink;
                case PlatformType.WAP: return ViewResources.Wap;
                case PlatformType.BetfromOthers: return ViewResources.BetfromOthers;
                default: return ViewResources.All;
            }
        }

        public static string GetProductName(string productCode)
        {
            return ViewResources.ResourceManager.GetString(productCode);
        }
    }
}
