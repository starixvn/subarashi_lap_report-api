using Fanex.Athena.Report.Resources.Constants;
using System.Collections.Generic;
using System.Linq;

namespace Fanex.Athena.Report.Common.Helpers
{
    public static class SportHelper
    {
        private static readonly IEnumerable<int> VSBetRadarSportTypes = new List<int>
        {
            SportType.VirtualHorseRacing,
            SportType.VirtualDogRacing
        };

        private static readonly IEnumerable<int> VSSportTypes = new List<int>
        {
            SportType.VirtualGreyhound,
            SportType.VirtualMotoRacing,
            SportType.VirtualSpeedway,
            SportType.VirtualCycling
        };

        private static readonly IEnumerable<int> Match3rdSportTypes = new List<int>
        {
            SportType.BetbyBasketball,
            SportType.VirtuallySports
        };

        private static readonly IEnumerable<int> League3rdSportsTypes = new List<int>
        {
            SportType.BitCoin,
            SportType.VirtualHorseRacing,
            SportType.VirtualDogRacing,
            SportType.BetbyBasketball,
            SportType.VirtuallySports
        };

        public static bool IsVirtualBetRadarSports(int sportType) => VSBetRadarSportTypes.Contains(sportType);

        public static bool IsLeague3rdSports(int sportType) => League3rdSportsTypes.Contains(sportType);

        public static bool IsMatch3rdSports(int sportType) => Match3rdSportTypes.Contains(sportType);

        public static bool IsVSSportTypes(int sportType) => VSSportTypes.Contains(sportType);
    }
}