using Fanex.RemotingRef;

namespace Fanex.Athena.Report.Common.Services.Interfaces
{
    public interface ICacheRefAdapter
    {
        IRemotingRefService RemotingRefService { get; }

        string[] GetCombinationTypeCombo(string betTeam, string betCheck, decimal handicap1, decimal handicap2);

        string GetLeagueName(object leagueId, int sportId = 0, string langCode = null);

        string GetMatchName(object homeId, object awayId, bool IsNeutral = false);

        string GetTeamName(object teamId, int sportId = 0);

        string GetLeagueName3rd(object leagueId);

        string GetSportName(object sportId, string languageCode = null);

        string[] GetBetBuilderSelectionNames(int[] selectionIds);

        string GetBettype(object bettypeId, bool isHideSaba);

        string GetBettype(object bettypeId);

        string GetBetTypeName(object bettypeId, string languageCode, bool isHideSaba);

        string GetBettypeWithProductName(int betType, string productCode = "", bool liveIndicator = false);
    }
}
