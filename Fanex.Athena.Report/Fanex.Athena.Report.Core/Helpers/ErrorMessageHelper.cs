namespace Fanex.Athena.Report.Core.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text;
    using Fanex.Data;
    using Microsoft.AspNetCore.Http;

    public static class ErrorMessageHelper
    {
        public static string GenerateMessage(HttpContext context, Exception exception)
        {
            var messageBuilder = new StringBuilder();

            messageBuilder.AppendLine($"ErrMsg: {exception.Message}");

            AddRequestInfo(context, messageBuilder);

            AddDatabaseInfo(messageBuilder, exception);

            const int limitedNumOfInnerException = 3;
            AddExceptionInfo(messageBuilder, exception, limitedNumOfInnerException);

            return messageBuilder.ToString();
        }

        private static void AddRequestInfo(HttpContext context, StringBuilder builder)
        {
            builder.AppendLine();
            builder.AppendLine("REQUEST INFO");

            var webInfo = GetRequestInfo(context);

            foreach (var item in webInfo)
            {
                builder.AppendLine($"{item.Key}: {item.Value}");
            }
        }

        private static void AddDatabaseInfo(StringBuilder builder, Exception exception)
        {
            builder.AppendLine();
            builder.AppendLine("DATABASE INFO");

            var dbInfo = GetObjectDbExceptionInfo(exception);

            foreach (var item in dbInfo)
            {
                builder.AppendLine($"{item.Key}: {item.Value}");
            }
        }

        private static void AddExceptionInfo(StringBuilder builder, Exception exception, int limitedInnerException)
        {
            builder.AppendLine();
            builder.AppendLine("EXCEPTION INFO");

            builder.AppendLine($"Source: {exception.Source}");
            builder.AppendLine($"TargetSite: {exception.TargetSite}");
            builder.AppendLine($"StackTrace: {exception.StackTrace}");

            if (exception.InnerException != null && limitedInnerException > 0)
            {
                AddExceptionInfo(builder, exception.InnerException, --limitedInnerException);
            }
        }

        private static IDictionary<string, object> GetObjectDbExceptionInfo(Exception ex)
        {
            if (ex is ObjectDbException)
            {
                var dbException = ex as ObjectDbException;
                var connString = new SqlConnectionStringBuilder(dbException.ConnectionString);

                return new Dictionary<string, object>
                {
                    ["Server"] = connString.DataSource,
                    [nameof(dbException.DbName)] = dbException.DbName,
                    [nameof(connString.UserID)] = connString.UserID,
                    [nameof(dbException.SpName)] = dbException.SpName,
                    ["Parameters"] = GetParameters(dbException.Parameters),
                    [nameof(dbException.CommandTimeout)] = dbException.CommandTimeout
                };
            }
            return new Dictionary<string, object>();
        }

        private static string GetParameters(IDictionary<string, object> parameters)
           => parameters.Count > 0
               ? parameters.Select(parameter => $"{parameter.Key}={parameter.Value}")
                        .Aggregate((firstPair, secondPair) => $"{firstPair};{secondPair}")
               : string.Empty;

        private static IDictionary<string, object> GetRequestInfo(HttpContext context)
        {
            var request = context.Request;
            return request == null
                    ? new Dictionary<string, object>()
                    : new Dictionary<string, object>
                    {
                        [nameof(request.Host)] = request.Host,
                        [nameof(request.Path)] = request.Path,
                        [nameof(request.Body)] = request.Body,
                        ["Remote IP"] = context.Connection.RemoteIpAddress
                    };
        }
    }
}
