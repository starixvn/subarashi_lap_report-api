namespace Fanex.Athena.Report.BySite.DTOs;

using System.Collections.Generic;

public class MatchMonitoringCenter
{
    public IList<MatchMonitoringItem> Running { get; set; }

    public IList<MatchMonitoringItem> Settled { get; set; }
}