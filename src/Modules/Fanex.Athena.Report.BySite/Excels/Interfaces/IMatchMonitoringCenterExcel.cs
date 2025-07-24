namespace Fanex.Athena.Report.BySite.Excels.Interfaces;

using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;

public interface IMatchMonitoringCenterExcel
{
    string Export(string template, MatchMonitoringCenter data, MatchMonitoringCenterGetMetaModel metaModel);
}
