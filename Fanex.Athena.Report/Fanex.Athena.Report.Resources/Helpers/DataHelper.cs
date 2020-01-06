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
                case PlatformType.Asia: return ViewResources.Asia;
                case PlatformType.China: return ViewResources.China;
                case PlatformType.Euro: return ViewResources.Euro;
                case PlatformType.Wap: return ViewResources.Wap;
                case PlatformType.Mobile: return ViewResources.Mobile;
                case PlatformType.Tablet: return ViewResources.Tablet;
                case PlatformType.NewAsia: return ViewResources.NewAsia;
                case PlatformType.MobileApp: return ViewResources.MobileApp;
                case PlatformType.MobileLiteVersion: return ViewResources.MobileLiteVersion;
                case PlatformType.DeepLink: return ViewResources.DeepLink;
                case PlatformType.NativeApp: return ViewResources.NativeApp;
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
