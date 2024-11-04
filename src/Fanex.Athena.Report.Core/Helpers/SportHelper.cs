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

        public static bool IsVirtualBetRadarSports(int sportType) => VSBetRadarSportTypes.Contains(sportType);
    }
}
