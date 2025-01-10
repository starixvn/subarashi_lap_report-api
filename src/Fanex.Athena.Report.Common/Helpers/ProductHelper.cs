using Fanex.Athena.Report.Common.Constants;
using Fanex.Athena.Report.Resources;
using Fanex.Athena.Report.Resources.Constants;
using System;
using System.Collections.Generic;

namespace Fanex.Athena.Report.Common.Helpers
{
    public static class ProductHelper
    {
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

        public static string GetProductName(string productCode, string defaultValue = null)
        {
            if (!ProductNameMap.ContainsKey(productCode))
            {
                return defaultValue ?? productCode;
            }

            var resourceKey = ProductNameMap[productCode];
            return ViewResources.ResourceManager.GetString(resourceKey);
        }

        private static readonly IDictionary<string, string> ProductNameMap =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { Product.BA, nameof(ViewResources.ProductBA) },
                { Product.NG, nameof(ViewResources.ProductNG) },
                { Product.SB, nameof(ViewResources.Sportsbook) },
                { Product.VS, nameof(ViewResources.ProductVS) },
                { "VS180", nameof(ViewResources.VirtualSoccer) },
                { "VS181", nameof(ViewResources.VirtualHorseRacing) },
                { "VS182", nameof(ViewResources.VirtualGreyhounds) },
                { "VS183", nameof(ViewResources.VirtualSpeedway) },
                { "VS184", nameof(ViewResources.VirtualMotoRacing) },
                { "VS185", nameof(ViewResources.VirtualCycling) },
                { "VS186", nameof(ViewResources.VirtualTennis) },
                { "VS188", nameof(ViewResources.VirtualCockFighting) },
                { Product.CS, nameof(ViewResources.ProductCS) },
                { Product.KN, nameof(ViewResources.ProductKN) },
                { Product.VB, nameof(ViewResources.ProductVB) },
                { Product.VS2, nameof(ViewResources.ProductVS2) },
                { "VS2190", nameof(ViewResources.VirtualSoccerLeague) },
                { "VS2191", nameof(ViewResources.VirtualSoccerNation) },
                { "VS2192", nameof(ViewResources.VirtualSoccerWorldCup) },
                { "VS2193", nameof(ViewResources.VirtualBasketball) },
                { "VS2194", nameof(ViewResources.VirtualSoccerAsianCup) },
                { "VS2195", nameof(ViewResources.VirtualSoccerEnglishPremier) },
                { "VS2196", nameof(ViewResources.VirtualSoccerChampionsCup) },
                { "VS2197", nameof(ViewResources.VirtualSoccerEuroCup) },
                { Product.LT, nameof(ViewResources.ProductLT) },
                { Product.AB, nameof(ViewResources.ProductAB) },
                { Product.PP, nameof(ViewResources.ProductPP) },
                { Product.CC, nameof(ViewResources.ProductCC) },
                { Product.AGC, nameof(ViewResources.ProductAGC) },
                { Product.SG, nameof(ViewResources.ProductSG) },
                { Product.VRG, nameof(ViewResources.ProductVRG) },
                { Product.TG, nameof(ViewResources.ProductTG) },
                { Product.I4D, nameof(ViewResources.ProductI4D) },
                { Product.SXG, nameof(ViewResources.ProductSXG) },
                { Product.SBC, nameof(ViewResources.ProductSBC) },
                { Product.SAG, nameof(ViewResources.ProductSAG) },
                { Product.PGS, nameof(ViewResources.ProductPGS) },
                { Product.JK, nameof(ViewResources.ProductJK) },
                { Product.SLT, nameof(ViewResources.ProductSLT) },
                { Product.HB, nameof(ViewResources.ProductHB) },
                { Product.WM, nameof(ViewResources.ProductWM) },
                { Product.BGG, nameof(ViewResources.ProductBGG) },
                { Product.VGA, nameof(ViewResources.ProductVGA) },
                { Product.CG, nameof(ViewResources.ProductCG) },
                { Product.CO, nameof(ViewResources.ProductCO) },
                { Product.BX, nameof(ViewResources.ProductBX) },
                { Product.AP, nameof(ViewResources.ProductAP) },
                { Product.APM, nameof(ViewResources.ProductAPM) },
                { Product.VS3, nameof(ViewResources.ProductVS3) },
                { Product.FGGL, nameof(ViewResources.ProductFGGL) },
                { Product.MXG, nameof(ViewResources.ProductMXG) },
                { Product.JIG, nameof(ViewResources.ProductJIG) },
                { Product.YB, nameof(ViewResources.ProductYB) },
                { Product.MGRNG, nameof(ViewResources.ProductMGRNG) },
                { Product.LG, nameof(ViewResources.ProductLG) },
                { Product.SEA, nameof(ViewResources.ProductSEA) },
                { Product.WE, nameof(ViewResources.ProductWE) },
                { Product.L22, nameof(ViewResources.ProductL22) },
                { Product.UUSL, nameof(ViewResources.ProductUUSL) },
                { Product.FS, nameof(ViewResources.ProductFS) },
                { Product.FAC, nameof(ViewResources.ProductFAC) },
                { Product.NS, nameof(ViewResources.ProductNS) },
                { Product.ON, nameof(ViewResources.ProductON) },
                { Product.PTRNG, nameof(ViewResources.ProductPTRNG) },
                { Product.GPIRNG, nameof(ViewResources.ProductGPIRNG) },
                { Product.GPILC, nameof(ViewResources.ProductGPILC) },
                { Product.PS, nameof(ViewResources.ProductPS) }
            };
    }
}
