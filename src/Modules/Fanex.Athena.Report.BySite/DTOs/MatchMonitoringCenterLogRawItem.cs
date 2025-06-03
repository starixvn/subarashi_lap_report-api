namespace Fanex.Athena.Report.BySite.DTOs;

using System;

public class MatchMonitoringCenterLogRawItem
{
    public int LogId { get; set; }

    public DateTime LogDate { get; set; }

    public int ActionID { get; set; }

    public string Operator { get; set; }

    public string Data { get; set; }

    public string OldData { get; set; }
}