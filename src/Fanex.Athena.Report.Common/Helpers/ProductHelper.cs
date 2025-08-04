namespace Fanex.Athena.Report.Common.Helpers
{
    using Constants;
    using Resources.Constants;

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
    }
}