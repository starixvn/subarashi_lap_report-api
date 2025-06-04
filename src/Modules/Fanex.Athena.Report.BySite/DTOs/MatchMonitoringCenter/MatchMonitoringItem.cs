namespace Fanex.Athena.Report.BySite.DTOs;

using System;
using System.Collections.Generic;

public class MatchMonitoringItem
{
    public int LeagueId { get; set; }

    public string LeagueName { get; set; }

    public int MonitoringId { get; set; }

    public int MatchId { get; set; }

    public string MatchName { get; set; }

    public string HomeName { get; set; }

    public string AwayName { get; set; }

    public int HomeId { get; set; }

    public int AwayId { get; set; }

    public bool IsNeutral { get; set; }

    public DateTime KickOffTime { get; set; }

    public string KickOffTimeString => KickOffTime.ToString("yyyy-MM-dd HH:mm:ss");

    public bool LiveIndicator { get; set; }

    public string LiveHomeScore { get; set; }

    public string LiveAwayScore { get; set; }

    public bool IsAlerted { get; set; }

    public List<string> AlertRules { get; set; }

    public bool IsSettled { get; set; }

    public decimal TotalHome { get; set; }

    public decimal TotalAway { get; set; }

    public decimal HomeStake { get; set; }

    public decimal AwayStake { get; set; }

    public decimal ForecastHome { get; set; }

    public decimal ForecastAway { get; set; }
}