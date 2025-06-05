namespace Fanex.Athena.Report.BySite.Excels.Interfaces;

using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels.Options;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;
using System.Collections.Generic;

public interface IMatchMonitoringCenterExcel
{
    string Export(string template, MatchMonitoringCenter data, MatchMonitoringCenterGetMetaModel metaModel);
}
