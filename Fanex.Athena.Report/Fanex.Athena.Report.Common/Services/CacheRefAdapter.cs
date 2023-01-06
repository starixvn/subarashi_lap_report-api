using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.RemotingRef;
using System;
using System.Globalization;

namespace Fanex.Athena.Report.Common.Services
{
    public class CacheRefAdapter : ICacheRefAdapter
    {
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

            return RemotingRefService.GetSportName(sportId.ToString(), GetCurrentIBCLanguageCode(languageCode), string.Empty);
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

        public string[] GetCombinationTypeCombo(string betTeam, string betCheck, decimal handicap1, decimal handicap2)
        {
            return RemotingRefService.GetCombinationTypeCombo(
                betTeam,
                betCheck,
                Convert.ToInt32(handicap1),
                Convert.ToInt32(handicap2),
                GetCurrentIBCLanguageCode());
        }

        public string[] GetBetBuilderSelectionNames(int[] selectionIds)
        {
            return RemotingRefService.GetBetBuilderSelectionNames(selectionIds, GetCurrentIBCLanguageCode());
        }

        private static bool IsValidId(object id)
        {
            return id != null && id.ToString() != string.Empty && id.ToString() != "0";
        }

        private static string GetCurrentIBCLanguageCode(string languageCode = null)
        {
            return string.IsNullOrEmpty(languageCode) ? CultureInfo.CurrentUICulture.Name : languageCode;
        }

        public string GetMatchName(object homeId, object awayId, bool IsNeutral = false)
        {
            return GetTeamName(homeId) + (IsNeutral ? " (N)" : string.Empty) + " - vs - " + GetTeamName(awayId);
        }

        public string GetTeamName(object teamId)
        {
            if (!IsValidId(teamId))
            {
                return string.Empty;
            }

            return RemotingRefService.GetTeamName(teamId.ToString(), GetCurrentIBCLanguageCode(), string.Empty);
        }

        public string GetBettype(object bettypeId)
        {
            if (!IsValidId(bettypeId))
            {
                return string.Empty;
            }

            return RemotingRefService.GetBetTypeName(bettypeId.ToString(), GetCurrentIBCLanguageCode());
        }
    }
}