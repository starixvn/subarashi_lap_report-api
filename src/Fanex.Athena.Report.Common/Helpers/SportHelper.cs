namespace Fanex.Athena.Report.Common.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using Resources.Constants;

    public static class SportHelper
    {
        private static readonly IEnumerable<int> VSNewRacingSportTypes = new List<int>
        {
            SportType.VirtualHorseRacing,
            SportType.VirtualDogRacing,
            SportType.VirtualSteepleChase,
            SportType.VirtualHarnessRacing,
            SportType.VirtualMotorRacing
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
            SportType.VirtuallySports,
            SportType.VirtualSteepleChase,
            SportType.VirtualHarnessRacing,
            SportType.VirtualMotorRacing
        };

        private static readonly IEnumerable<int> League3rdSportsTypes = new List<int>
        {
            SportType.BitCoin,
            SportType.VirtualHorseRacing,
            SportType.VirtualDogRacing,
            SportType.BetbyBasketball,
            SportType.VirtuallySports,
            SportType.VirtualSteepleChase,
            SportType.VirtualHarnessRacing,
            SportType.VirtualMotorRacing
        };

        public static bool IsVSNewRacingSportTypes(int sportType) => VSNewRacingSportTypes.Contains(sportType);

        public static bool IsLeague3rdSports(int sportType) => League3rdSportsTypes.Contains(sportType);

        public static bool IsMatch3rdSports(int sportType) => Match3rdSportTypes.Contains(sportType);

        public static bool IsVSSportTypes(int sportType) => VSSportTypes.Contains(sportType);
    }
}