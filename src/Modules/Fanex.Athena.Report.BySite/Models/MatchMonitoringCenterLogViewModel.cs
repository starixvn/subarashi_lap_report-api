using System.Collections.Generic;
using Fanex.Athena.Report.Common.Model.DTO;

namespace Fanex.Athena.Report.BySite.Models;

public class MatchMonitoringCenterLogViewModel
{
    public IEnumerable<SelectItem> ActionOptions { get; set; }

    public IEnumerable<SelectItem> LogTypeOptions { get; set; }
}