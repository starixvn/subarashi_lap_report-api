namespace Fanex.Athena.Report.BySite.DTOs;

using System;
using System.Collections.Generic;

public class TriggeredAlertHistory
{
    public DateTime Time { get; set; }

    public string TimeString => Time.ToString("yyyy-MM-dd HH:mm:ss");

    public List<string> DetectedInfos { get; set; }

    public List<string> AlertRules { get; set; }
}
