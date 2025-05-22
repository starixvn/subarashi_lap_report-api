
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Common;
using Fanex.Data;

namespace Fanex.Athena.Report.Core.Utility;

public static class ObjectDbExtension
{
    public static ResultInfo GetResult(this IObjectDb objectDb)
    {
        var result = new ResultInfo();
        AssignResult(objectDb, result);
        return result;
    }

    public static void AddResultParam(this IObjectDb objectDb)
    {
        objectDb.SetParameterValue("ErrCode", null);
        objectDb.SetParameterValue("ErrMsg", null);
    }

    static void AssignResult(IObjectDb db, ResultInfo result)
    {
        var errorCode = db.GetParameterValue("ErrCode");
        var errorMessage = db.GetParameterValue("ErrMsg");

        result.Code = Converter.ToInt(errorCode);
        result.Message = Converter.ToString2(errorMessage);
    }
}
