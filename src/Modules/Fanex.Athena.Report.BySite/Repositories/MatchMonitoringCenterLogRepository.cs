namespace Fanex.Athena.Report.BySite.Repositories;

using System.Collections.Generic;
using System.Threading.Tasks;
using Data;
using Data.Extensions.AdoNet;
using DTOs;
using Interfaces;
using Models;
using Resources;

public class MatchMonitoringCenterLogRepository : IMatchMonitoringCenterLogRepository
{
    public async Task<IEnumerable<LogAction>> GetLogActions()
    {
        const string uspName = "Lrf_LogAction_LicSel";
        using var db = ObjectDbFactory.CreateInstance(uspName);

        var result = new List<LogAction>();
        using var reader = await db.ExecuteReaderAsync().ConfigureAwait(false);

        while (reader.Read())
        {
            var actionTypeID = reader.ToInt("ActionTypeID");
            var actionID = reader.ToInt("ActionID");
            var actionTypeName = ViewResources.ResourceManager.GetString($"ActionType_{actionTypeID}") ?? reader.ToString("ActionTypeName");
            var actionName = ViewResources.ResourceManager.GetString($"Action_{actionID}") ?? reader.ToString("ActionName");

            result.Add(new LogAction
            {
                ActionTypeID = actionTypeID,
                ActionTypeName = actionTypeName,
                ActionID = actionID,
                ActionName = actionName
            });
        }

        return result;
    }

    public async Task<IEnumerable<MatchMonitoringCenterLogRawItem>> GetLogData(MatchMonitoringCenterGetLogMetaModel model)
    {
        const string uspName = "Lrf_LicenseeMatchMonitoringLog_Sel";
        using var db = ObjectDbFactory.CreateInstance(uspName);

        db.SetParameterValue("LicenseeId", model.LicenseeId);
        db.SetParameterValue("LicenseeName", model.LicenseeName);
        db.SetParameterValue("ActionIDs", model.Actions);
        db.SetParameterValue("MonitoringID", model.MonitoringID);

        var result = new List<MatchMonitoringCenterLogRawItem>();
        using var reader = await db.ExecuteReaderAsync().ConfigureAwait(false);

        while (reader.Read())
        {
            result.Add(new MatchMonitoringCenterLogRawItem
            {
                LogId = reader.ToInt("LogId"),
                LogDate = reader.ToDateTime("LogDate"),
                ActionID = reader.ToInt("ActionID"),
                Operator = reader.ToString("Operator"),
                Data = reader.ToString("Data"),
                OldData = reader.ToString("OldData")
            });
        }

        return result;
    }
}