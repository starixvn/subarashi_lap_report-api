using Fanex.Athena.Report.Core.Constants;

namespace Fanex.Athena.Report.Core.Helpers
{
    public static class ProductHelper
    {
        public static int GetProductId(int bettype)
        {
            if (BetTypeHelper.IsNumberGame(bettype) ||
                BetTypeHelper.IsNumberGameHappy5(bettype))
            {
                return ProductTypes.NumberGame;
            }

            if (BetTypeHelper.IsVirtualSports(bettype))
            {
                return ProductTypes.VirtualSports;
            }

            if (BetTypeHelper.IsVirtualSports2(bettype))
            {
                return ProductTypes.VirtualSports2;
            }

            return ProductTypes.Sportsbook;
        }
    }
}
