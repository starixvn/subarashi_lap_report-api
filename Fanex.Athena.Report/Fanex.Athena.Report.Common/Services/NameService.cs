using Fanex.Athena.Report.Common.Constants;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Resources.Constants;
using Fanex.Athena.Report.Resources;
using System;
using Fanex.Athena.Report.Common.Model.DataDomain;

namespace Fanex.Athena.Report.Common.Services
{
    public class NameService : INameService
    {
        private readonly ICacheRefAdapter _cacheRefAdapter;

        public NameService(ICacheRefAdapter cacheRefAdapter)
            => _cacheRefAdapter = cacheRefAdapter;

        public string GetBetTypeName(int betTypeId, string lang = null)
        {
            string suffix = GetBettypeNameSuffix(betTypeId);
            return _cacheRefAdapter.GetBettype(Convert.ToInt32(betTypeId)) + suffix;
        }

        public string GetSportName(int sportTypeId, string lang = null)
        {
            if (sportTypeId > 0)
            {
                return _cacheRefAdapter.GetSportName(sportTypeId, lang);
            }
            if (sportTypeId == HardCodeSportType.Parlay)
            {
                return ViewResources.Parlay;
            }
            var sportName = string.Empty;
            switch (sportTypeId)
            {
                case HardCodeSportType.SabaBasketball:
                    sportName = _cacheRefAdapter.GetSportName((int)SportType.BasketBall, lang);
                    break;
                case HardCodeSportType.SabaSoccer:
                    sportName = _cacheRefAdapter.GetSportName((int)SportType.Soccer, lang);
                    break;
                case HardCodeSportType.SabaCricket:
                    sportName = _cacheRefAdapter.GetSportName((int)SportType.Cricket, lang);
                    break;
                case HardCodeSportType.SabaSoccerPinGoal:
                    sportName = ViewResources.SoccerPinGoal;
                    break;

                case HardCodeSportType.SabaBasketballPinGoal:
                    sportName = ViewResources.BasketballPinGoal;
                    break;

                case HardCodeSportType.SabaESportPinGoal:
                    sportName = ViewResources.ESportsPinGoal;
                    break;

                case HardCodeSportType.SabaFutsal:
                    sportName = _cacheRefAdapter.GetSportName((int)SportType.Futsal, lang);
                    break;

                default:
                    break;
            }
            if (String.IsNullOrWhiteSpace(sportName))
            {
                return sportTypeId.ToString();
            }
            return $"{ViewResources.Saba}{sportName}";
        }

        private string GetBettypeNameSuffix(int betType)
        {
            if (betType == (int)BetType.NumberGameHappy5OddEven)
            {
                return " (" + _cacheRefAdapter.GetSportName((int)SportType.NumberGameHappy5) + ")";
            }

            if (betType >= (int)BetType.VirtualBasketballOverUnder && betType <= (int)BetType.VirtualBasketballFirstHalfHandicap)
            {
                return " (" + _cacheRefAdapter.GetSportName((int)SportType.VirtualBasketball) + ")";
            }

            return string.Empty;
        }
    }
}
