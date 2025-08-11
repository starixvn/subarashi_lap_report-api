namespace Fanex.Athena.Report.BySite.Excels.Interfaces;

using DTOs;
using MetaModels;

public interface IMatchMonitoringCenterExcel
{
    string Export(string template, MatchMonitoringCenter data, MatchMonitoringCenterGetMetaModel metaModel);
}
