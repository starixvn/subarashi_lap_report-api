using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Text;

namespace Fanex.Athena.Report.Common.Extensions
{
    public static class IDataReaderExtension
    {
        public static T GetVal<T>(this IDataReader dataReader, string columnName, T defauleValue = default(T))
        {
            var dbValue = dataReader[columnName];
            if (dbValue == DBNull.Value)
            {
                return defauleValue;
            }

            return Cast<T>(dbValue, defauleValue);
        }

        private static T Cast<T>(object dbValue, T defauleValue = default(T))
        {
            if (dbValue is T result)
            {
                return result;
            }

            if (dbValue is INullable temp && temp.IsNull)
            {
                return defauleValue;
            }

            return (T)Convert.ChangeType(dbValue, typeof(T));
        }
    }
}
