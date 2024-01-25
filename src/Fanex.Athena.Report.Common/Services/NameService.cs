using System;
using Fanex.Athena.Report.Common.Constants;
using Fanex.Athena.Report.Common.Model.DataDomain;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Resources;
using Fanex.Athena.Report.Resources.Constants;

namespace Fanex.Athena.Report.Common.Services
{
    public class NameService : INameService
    {
        private readonly ICacheRefAdapter _cacheRefAdapter;

        public NameService(ICacheRefAdapter cacheRefAdapter)
            => _cacheRefAdapter = cacheRefAdapter;

        public string GetBetTypeName(int betTypeId, bool isHideSaba = false, string lang = null)
        {
            string suffix = GetBettypeNameSuffix(betTypeId);
            return _cacheRefAdapter.GetBettype(Convert.ToInt32(betTypeId), isHideSaba) + suffix;
        }

        public string GetSportName(int sportTypeId, bool isHideSaba = false, string lang = null)
        {
            if (sportTypeId > 0)
            {
                return _cacheRefAdapter.GetSportName(sportTypeId, lang);
            }
            if (sportTypeId == HardCodeSportType.Parlay)
            {
                return ViewResources.Parlay;
            }
            string sportName = GetSabaSportSuffixName(sportTypeId, lang);
            var prefixText = isHideSaba ? ViewResources.Virtual : ViewResources.Saba;
            if (String.IsNullOrWhiteSpace(sportName))
            {
                return sportTypeId.ToString();
            }
            return $"{prefixText}{sportName}";
        }

        private string GetSabaSportSuffixName(int sportTypeId, string lang)
        {
            var sportName = string.Empty;
            switch (sportTypeId)
            {
                case HardCodeSportType.SabaBasketball:
                    sportName = _cacheRefAdapter.GetSportName(SportType.BasketBall, lang);
                    break;

                case HardCodeSportType.SabaSoccer:
                    sportName = _cacheRefAdapter.GetSportName(SportType.Soccer, lang);
                    break;

                case HardCodeSportType.SabaCricket:
                    sportName = _cacheRefAdapter.GetSportName(SportType.Cricket, lang);
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
                    sportName = _cacheRefAdapter.GetSportName(SportType.Futsal, lang);
                    break;

                case HardCodeSportType.SabaTennis:
                    sportName = _cacheRefAdapter.GetSportName(SportType.Tennis, lang);
                    break;

                case HardCodeSportType.SabaIceHockey:
                    sportName = _cacheRefAdapter.GetSportName(SportType.IceHockey, lang);
                    break;

                default:
                    sportName = string.Empty;
                    break;
            }

            return sportName;
        }

        private string GetBettypeNameSuffix(int betType)
        {
            if (betType == BetType.NumberGameHappy5OddEven)
            {
                return " (" + _cacheRefAdapter.GetSportName(SportType.NumberGameHappy5) + ")";
            }

            if (betType >= BetType.VirtualBasketballOverUnder && betType <= BetType.VirtualBasketballFirstHalfHandicap)
            {
                return " (" + _cacheRefAdapter.GetSportName(SportType.VirtualBasketball) + ")";
            }

            if (betType == BetType.VirtualSportsTennisCorrectSoccer)
            {
                return $" ({_cacheRefAdapter.GetSportName(SportType.Tennis)})";
            }

            return string.Empty;
        }
    }
}