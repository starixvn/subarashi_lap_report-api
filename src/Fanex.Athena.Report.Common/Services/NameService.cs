namespace Fanex.Athena.Report.Common.Services
{
    using System;
    using System.Collections.Generic;
    using Constants;
    using Interfaces;
    using Model.DataDomain;
    using Resources;
    using Resources.Constants;

    public class NameService : INameService
    {
        private readonly ICacheRefAdapter _cacheRefAdapter;

        private readonly Dictionary<int, Func<int, string, string>> _sabaSportNameMapper;

        public NameService(ICacheRefAdapter cacheRefAdapter)
        {
            _cacheRefAdapter = cacheRefAdapter;

            // init saba sport name mapper
            _sabaSportNameMapper = new Dictionary<int, Func<int, string, string>>
            {
                { HardCodeSportType.SabaBasketball, (_, lang) => _cacheRefAdapter.GetSportName(SportType.BasketBall, lang) },
                { HardCodeSportType.SabaSoccer, (_, lang) => _cacheRefAdapter.GetSportName(SportType.Soccer, lang) },
                { HardCodeSportType.SabaCricket, (_, lang) => _cacheRefAdapter.GetSportName(SportType.Cricket, lang) },
                { HardCodeSportType.SabaSoccerPinGoal, (_, _) => ViewResources.SoccerPinGoal },
                { HardCodeSportType.SabaBasketballPinGoal, (_, _) => ViewResources.BasketballPinGoal },
                { HardCodeSportType.SabaESportPinGoal, (_, _) => ViewResources.ESportsPinGoal },
                { HardCodeSportType.SabaFutsal, (_, lang) => _cacheRefAdapter.GetSportName(SportType.Futsal, lang) },
                { HardCodeSportType.SabaTennis, (_, lang) => _cacheRefAdapter.GetSportName(SportType.Tennis, lang) },
                { HardCodeSportType.SabaKabaddi, (_, lang) => _cacheRefAdapter.GetSportName(SportType.Kabaddi, lang) },
                { HardCodeSportType.SabaIceHockey, (_, lang) => _cacheRefAdapter.GetSportName(SportType.IceHockey, lang) },
                { HardCodeSportType.SabaSoccerMarble, (_, _) => ViewResources.SoccerMarble }
            };
        }

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
            if (string.IsNullOrWhiteSpace(sportName))
            {
                return sportTypeId.ToString();
            }

            return $"{prefixText}{sportName}";
        }

        private string GetSabaSportSuffixName(int sportTypeId, string lang)
        {
            var mapperFunc = _sabaSportNameMapper.GetValueOrDefault(sportTypeId, (_, _) => string.Empty);

            return mapperFunc(sportTypeId, lang);
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