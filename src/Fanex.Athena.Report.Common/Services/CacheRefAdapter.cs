using System;
using System.Globalization;
using System.Linq;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Resources;
using Fanex.Athena.Report.Resources.Constants;
using Fanex.RemotingRef;

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

        public string GetLeagueName(object leagueId)
        {
            if (!IsValidId(leagueId))
            {
                return string.Empty;
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

        public string GetMatchName(object homeId, object awayId, bool IsNeutral = false) => GetTeamName(homeId) +
            (IsNeutral ? " (N)" : string.Empty) + " - vs - " + GetTeamName(awayId);

        public string GetTeamName(object teamId)
        {
            if (!IsValidId(teamId))
            {
                return string.Empty;
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
            var betTypeName = GetBettype(betType, liveIndicator);

            if (productCode == Product.VS)
            {
                return betTypeName + " (" + ViewResources.ProductVS + ")";
            }

            return betTypeName;
        }

        private static bool IsValidId(object id) => id != null && id.ToString() != string.Empty && id.ToString() != "0";

        private static string GetCurrentIBCLanguageCode(string languageCode = null) =>
            string.IsNullOrEmpty(languageCode) ? CultureInfo.CurrentUICulture.Name : languageCode;
    }
}
