using Fanex.Data;
using System.Data.SqlClient;

namespace Fanex.Athena.Report
{
    public class ShardDbSettingProvider : XmlDbSettingProvider
    {
        private readonly IShardInfoAccessor _shardInfoAccessor;

        public ShardDbSettingProvider(IShardInfoAccessor shardInfoAccessor, string resourcePath)
            : base(resourcePath)
        {
            _shardInfoAccessor = shardInfoAccessor;
        }

        public override IDbSetting GetSetting(string key)
        {
            if (_shardInfoAccessor.TryUseShardDB(key, out string dbServer))
            {
                var shardSPKey = _shardInfoAccessor.GetShardSPKey(key);
                var originalShardSetting = base.GetSetting(shardSPKey);

                var connectionStr = new SqlConnectionStringBuilder(originalShardSetting.ConnectionString)
                {
                    DataSource = dbServer
                };

                var dbSetting = new DbSetting(shardSPKey, originalShardSetting.DbName, connectionStr.ToString(), originalShardSetting.SpName,
                    originalShardSetting.CommandTimeout, originalShardSetting.ColumnMaps, originalShardSetting.FirmParameters);
                return dbSetting;
            }
            return base.GetSetting(key);
        }
    }
}
