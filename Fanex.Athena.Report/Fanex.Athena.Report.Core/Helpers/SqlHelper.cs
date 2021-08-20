namespace Fanex.Athena.Report.Core.Helpers
{
    public static class SqlHelper
    {
        public static string ToSqlTextSearch(string text)
            => string.IsNullOrEmpty(text) ? text : text.Trim().Replace("_", "[_]");
    }
}