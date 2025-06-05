namespace Fanex.Athena.Report.BySite.Models;

using System;
using System.Collections.Generic;
using Fanex.Athena.Report.Common.Model.DTO;

public class MatchMonitoringCenterViewModel
{
    public IList<League> LeagueList { get; set; }

    public IList<Match> MatchList { get; set; }

    public string BaseCurrency { get; set; }

    public string DefaultBaseCurrency { get; set; }

    public KeyValuePair<int, string>[] Currencies { get; set; }

    public KeyValuePair<int, string>[] BaseCurrencies { get; set; }

    public string CurrencyName { get; set; }

    public int CurrencyId { get; set; }

    public string LeagueIds { get; set; }

    public string MatchIds { get; set; }

    public bool HasSettingPermission { get; set; }

    public DateTime MoveBetsDay { get; set; }

    public string MoveBetsDayString => MoveBetsDay.ToString("MM/dd/yyyy");
}