namespace Fanex.Athena.Report.Core.Extensions
{
    using System.Collections.Generic;
    using Fanex.Data;
    using Fanex.Data.SqlServer;
    using Microsoft.Extensions.Configuration;

    public static class FanexDataSetting
    {
        public static void Setup(IConfiguration configuration)
        {
            DbSettingProviderManager
            .StartNewSession()
            .WithSqlServer(resourcePath: "~/SpConfig/FanexSpName.xml")
            .Use(GetFanexConnectStrings(configuration))
            .Run();
        }

        private static Dictionary<string, ConnectionConfiguration> GetFanexConnectStrings(IConfiguration configuration)
        {
            var connectStrDic = new Dictionary<string, ConnectionConfiguration>();
            var connectStrings = configuration.GetSection("ConnectionStrings");

            foreach (var connectStr in connectStrings.GetChildren())
            {
                connectStrDic.Add(connectStr.Key, new ConnectionConfiguration(connectStr.Key, connectStr.Value, "System.Data.SqlClient"));
            }

            return connectStrDic;
        }
    }
}