using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Text;

namespace Fanex.Athena.Report.Common.Extensions
{
    public static class IDataReaderExtension
    {
        public static T GetDef<T>(this IDataReader dataReader, string columnName, T defauleValue = default(T))
        {
            var dbValue = dataReader[columnName];
            if (dbValue == DBNull.Value || (dbValue is INullable temp && temp.IsNull))
            {
                return defauleValue;
            }
            return (T)dbValue;
        }
        public static T? GetVal<T>(this IDataReader dataReader, string columnName) where T : struct
        {
            var dbValue = dataReader[columnName];
            if (dbValue == DBNull.Value)
            {
                return null;
            }
            if (dbValue is INullable temp && temp.IsNull)
            {
                return (T?)null;
            }

            return (T)dbValue;
        }
    }
}
