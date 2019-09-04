namespace Fanex.Athena.Report.Resources.Helpers
{
    using Fanex.Athena.Report.Resources.Constants;

    public static class DataHelper
    {
        public static string GetPlatformName(int type)
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
                default: return ViewResources.All;
            }
        }

        public static string GetProductName(string productCode)
        {
            return ViewResources.ResourceManager.GetString(productCode);
        }
    }
}
