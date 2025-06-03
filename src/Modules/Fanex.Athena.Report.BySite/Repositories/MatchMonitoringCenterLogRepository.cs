namespace Fanex.Athena.Report.BySite.Repositories;

using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Models;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Common.Extensions;
using Fanex.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            result.Add(new LogAction
            {
                ActionTypeID = reader.ToInt("ActionTypeID"),
                ActionTypeName = reader.ToString("ActionTypeName"),
                ActionID = reader.ToInt("ActionID"),
                ActionName = reader.ToString("ActionName")
            });
        }

        return result;
    }

    public async Task<IEnumerable<MatchMonitoringCenterLogRawItem>> GetLogData(MatchMonitoringCenterGetLogMetaModel model)
    {
        const string uspName = "Lrf_UserLog_LicenseeSetting_Sel";
        using var db = ObjectDbFactory.CreateInstance(uspName);

        db.SetParameterValue("LicenseeId", model.LicenseeId);
        db.SetParameterValue("LicenseeName", model.LicenseeName);
        db.SetParameterValue("ActionIDs", model.Actions);

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