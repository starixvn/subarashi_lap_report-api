using Fanex.Athena.Report.Common.Helpers;
using Fanex.Athena.Report.Common.Model.DataDomain;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Resources;
using Fanex.Athena.Report.Resources.Constants;
using Fanex.RemotingRef;
using System;
using System.Globalization;
using System.Linq;

namespace Fanex.Athena.Report.Common.Services
{
    public class CacheRefAdapter : ICacheRefAdapter
    {
        private static readonly string[] SabaBetTypeMap =
        {
            "228",
            "229"
        };

        public CacheRefAdapter(IRemotingRefService remotingRefService)
        {
            RemotingRefService = remotingRefService;
        }

        public IRemotingRefService RemotingRefService { get; }

        public string GetSportName(object sportId, string languageCode = null)
        {
            if (!IsValidId(sportId))
            {
                return string.Empty;
            }

            return RemotingRefService.GetSportName(sportId.ToString(), GetCurrentIBCLanguageCode(languageCode),
                string.Empty);
        }

        public string GetLeagueName(object leagueId, int sportId = 0, string langCode = null)
        {
            if (!IsValidId(leagueId))
            {
                return string.Empty;
            }

            if (SportHelper.IsLeague3rdSports(sportId))
            {
                return GetLeagueName3rd(leagueId);
            }

            return RemotingRefService.GetLeagueName(leagueId.ToString(), GetCurrentIBCLanguageCode(), string.Empty);
        }

        public string GetLeagueName3rd(object leagueId)
        {
            if (!IsValidId(leagueId))
            {
                return string.Empty;
            }

            return RemotingRefService.GetLeagueName3rd(leagueId.ToString(), GetCurrentIBCLanguageCode(), string.Empty);
        }

        public string[] GetCombinationTypeCombo(string betTeam, string betCheck, decimal handicap1,
                                                decimal handicap2) =>
            RemotingRefService.GetCombinationTypeCombo(
                betTeam,
                betCheck,
                Convert.ToInt32(handicap1),
                Convert.ToInt32(handicap2),
                GetCurrentIBCLanguageCode());

        public string[] GetBetBuilderSelectionNames(int[] selectionIds) =>
            RemotingRefService.GetBetBuilderSelectionNames(selectionIds, GetCurrentIBCLanguageCode());

        public string GetMatchName(object homeId, object awayId, bool IsNeutral = false, int sportId = 0) => GetTeamName(homeId, sportId) +
            (IsNeutral ? " (N)" : string.Empty) + " - vs - " + GetTeamName(awayId, sportId);

        public string GetTeamName(object teamId, int sportId = 0)
        {
            if (!IsValidId(teamId))
            {
                return string.Empty;
            }

            if (SportHelper.IsMatch3rdSports(sportId))
            {
                return RemotingRefService.GetTeamName3rd(teamId.ToString(), GetCurrentIBCLanguageCode(), string.Empty);
            }

            return RemotingRefService.GetTeamName(teamId.ToString(), GetCurrentIBCLanguageCode(), string.Empty);
        }

        public string GetBettype(object bettypeId, bool isHideSaba)
        {
            if (!IsValidId(bettypeId))
            {
                return string.Empty;
            }

            if (isHideSaba && SabaBetTypeMap.Contains(bettypeId.ToString()))
            {
                return ViewResources.ResourceManager.GetString($"BetType_{bettypeId}");
            }

            return RemotingRefService.GetBetTypeName(bettypeId.ToString(), GetCurrentIBCLanguageCode());
        }

        public string GetBettype(object bettypeId)
        {
            if (!IsValidId(bettypeId))
            {
                return string.Empty;
            }

            return RemotingRefService.GetBetTypeName(bettypeId.ToString(), GetCurrentIBCLanguageCode());
        }

        public string GetBetTypeWithLiveIndicator(int bettypeId, bool liveIndicator)
        {
            const int SBMinOfMaxBettype = 120;
            if (bettypeId == 0)
            {
                return string.Empty;
            }

            var betTypeName = RemotingRefService.GetBetTypeName(bettypeId.ToString(CultureInfo.InvariantCulture), GetCurrentIBCLanguageCode());

            if (bettypeId == BetType.NgNumberGroup)
            {
                betTypeName += liveIndicator ? " (" + ViewResources.LiveGame + ")" : " (" + ViewResources.PreGame + ")";
                return betTypeName;
            }

            if (bettypeId >= BetType.NG1StBallOU && bettypeId < SBMinOfMaxBettype)
            {
                return GetNumberGameBettypeName(bettypeId, liveIndicator, betTypeName);
            }

            return betTypeName;
        }

        public string GetBetTypeName(object bettypeId, string languageCode, bool isHideSaba)
        {
            if (!IsValidId(bettypeId))
            {
                return string.Empty;
            }

            if (isHideSaba && SabaBetTypeMap.Contains(bettypeId.ToString()))
            {
                return ViewResources.ResourceManager.GetString($"BetType_{bettypeId}");
            }

            return RemotingRefService.GetBetTypeName(bettypeId.ToString(), languageCode);
        }

        public string GetBettypeWithProductName(int betType, string productCode = "", bool liveIndicator = false)
        {
            var betTypeName = GetBetTypeWithLiveIndicator(betType, liveIndicator);

            if (productCode == Product.VS)
            {
                return betTypeName + " (" + ViewResources.ProductVS + ")";
            }

            return betTypeName;
        }

        private static bool IsValidId(object id) => id != null && id.ToString() != string.Empty && id.ToString() != "0";

        private static string GetCurrentIBCLanguageCode(string languageCode = null) =>
            string.IsNullOrEmpty(languageCode) ? CultureInfo.CurrentUICulture.Name : languageCode;

        private static string GetNumberGameBettypeName(int betType, bool liveIndicator, string betTypeName)
        {
            if ((betType == BetType.NgOuFt || betType == BetType.NgOeFt) && liveIndicator)
            {
                return ViewResources.Next + " " + betTypeName;
            }

            if (betType == (int)BetType.NgOeFt)
            {
                return ViewResources.FT + " " + betTypeName;
            }

            return betTypeName;
        }
    }
}