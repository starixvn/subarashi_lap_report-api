namespace Fanex.Athena.Report.Common.Helpers
{
    using System;
    using System.Collections.Generic;
    using Constants;
    using Resources;
    using Resources.Constants;

    public static class ProductHelper
    {
        private static readonly IDictionary<string, string> ProductNameMap =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "VS180", nameof(ViewResources.VirtualSoccer) },
                { "VS181", nameof(ViewResources.VirtualHorseRacing) },
                { "VS182", nameof(ViewResources.VirtualGreyhounds) },
                { "VS183", nameof(ViewResources.VirtualSpeedway) },
                { "VS184", nameof(ViewResources.VirtualMotoRacing) },
                { "VS185", nameof(ViewResources.VirtualCycling) },
                { "VS186", nameof(ViewResources.VirtualTennis) },
                { "VS188", nameof(ViewResources.VirtualCockFighting) },
                { "VS2190", nameof(ViewResources.VirtualSoccerLeague) },
                { "VS2191", nameof(ViewResources.VirtualSoccerNation) },
                { "VS2192", nameof(ViewResources.VirtualSoccerWorldCup) },
                { "VS2193", nameof(ViewResources.VirtualBasketball) },
                { "VS2194", nameof(ViewResources.VirtualSoccerAsianCup) },
                { "VS2195", nameof(ViewResources.VirtualSoccerEnglishPremier) },
                { "VS2196", nameof(ViewResources.VirtualSoccerChampionsCup) },
                { "VS2197", nameof(ViewResources.VirtualSoccerEuroCup) }
            };

        public static int GetProductId(int bettype, string productCode)
        {
            if (BetTypeHelper.IsNumberGame(bettype) ||
                BetTypeHelper.IsNumberGameHappy5(bettype))
            {
                return ProductTypes.NumberGame;
            }

            if (BetTypeHelper.IsVirtualSports(bettype) || BetTypeHelper.IsVirtualSports2(bettype) || productCode == Product.VS)
            {
                return ProductTypes.VirtualSports;
            }

            return ProductTypes.Sportsbook;
        }

        public static string GetProductName(string productCode)
        {
            if (string.IsNullOrEmpty(productCode))
            {
                return productCode;
            }

            if (!ProductNameMap.TryGetValue(productCode, out var resourceKey))
            {
                resourceKey = "Product" + productCode;
            }

            return ViewResources.ResourceManager.GetString(resourceKey) ?? productCode;
        }
    }
}