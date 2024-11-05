using Fanex.Athena.Report.Resources.Constants;
using System.Collections.Generic;
using System.Linq;

namespace Fanex.Athena.Report.Core.Helpers
{
    public static class SportHelper
    {
        private static readonly IEnumerable<int> VSBetRadarSportTypes = new List<int>
        {
            SportType.VirtualHorseRacing ,
            SportType.VirtualDogRacing
        };

        private static readonly IEnumerable<int> VSSportTypes = new List<int>
        {
            SportType.VirtualGreyhound,
            SportType.VirtualMotoRacing,
            SportType.VirtualSpeedway,
            SportType.VirtualCycling
        };

        public static bool IsVirtualBetRadarSports(int sportType) => VSBetRadarSportTypes.Contains(sportType);

        public static bool IsVSSportTypes(int sportType) => VSSportTypes.Contains(sportType);
    }
}
