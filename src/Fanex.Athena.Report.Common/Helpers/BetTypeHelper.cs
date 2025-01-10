namespace Fanex.Athena.Report.Common.Helpers
{
    public static class BetTypeHelper
    {
        private const int NGMinBetType = 81;
        private const int NGMaxBetType = 119;
        private const int NGHappy5MinBetType = 8101;
        private const int NGHappy5MaxBetType = 8120;
        private const int Happy5LinkedGamesMinBetType = 3601;
        private const int Happy5LinkedGamesMaxBetType = 3610;

        private const int VSMinBetType = 1201;
        private const int VSMaxBetType = 1299;
        private const int VS2MinBetType = 2700;
        private const int VS2MaxBetType = 2899;

        public static bool IsNumberGame(int bettype)
        {
            return IsInRange(bettype, NGMinBetType, NGMaxBetType);
        }

        public static bool IsNumberGameHappy5(int bettype)
        {
            return IsInRange(bettype, NGHappy5MinBetType, NGHappy5MaxBetType)
                || IsInRange(bettype, Happy5LinkedGamesMinBetType, Happy5LinkedGamesMaxBetType);
        }

        public static bool IsVirtualSports(int bettype)
        {
            return IsInRange(bettype, VSMinBetType, VSMaxBetType);
        }

        public static bool IsVirtualSports2(int bettype)
        {
            return IsInRange(bettype, VS2MinBetType, VS2MaxBetType);
        }

        private static bool IsInRange(int value, int min, int max)
        {
            return value >= min && value <= max;
        }
    }
}
