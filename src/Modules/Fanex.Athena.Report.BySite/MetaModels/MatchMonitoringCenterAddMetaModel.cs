namespace Fanex.Athena.Report.BySite.MetaModels;

public class MatchMonitoringCenterAddMetaModel
{
    public int MatchId { get; set; }

    public bool LiveIndicator { get; set; }

    public int LiveHomeScore { get; set; }

    public int LiveAwayScore { get; set; }

    public int BetType { get; set; }

    public int LicenseeId { get; set; }

    public int UserId { get; set; }

    public string IP { get; set; }

    public string Language { get; set; }
}

