namespace Fanex.Athena.Report.BySite.Models;

using System.Collections.Generic;
using Common.Model.DTO;

public class MatchMonitoringCenterLogViewModel
{
    public IEnumerable<SelectItem> ActionOptions { get; set; }

    public IEnumerable<SelectItem> LogTypeOptions { get; set; }
}