using Fanex.Athena.Report.Core.Services.Interfaces;
using System;
using System.Collections.Generic;

namespace Fanex.Athena.Report.DbSettingConfig
{
    public interface IShardInfoAccessor
    {
        bool TryUseShardDB(string key, out string dbServer);

        string GetShardSPKey(string key);
    }

    public class ShardInfoAccessor : IShardInfoAccessor
    {
        private const string PrefixShardingDefault = "shard_";

        private readonly string _prefixSharding;

        protected Lazy<IReadOnlyDictionary<int, string>> SiteDBConfig { get; }

        private readonly IContextService _contextService;

        public ShardInfoAccessor(IContextService contextService, string prefixSharding = PrefixShardingDefault)
        {
            _contextService = contextService;

            _prefixSharding = prefixSharding;
        }

        public bool TryUseShardDB(string key, out string dbServer)
        {
            if (IsSharding(key) && !ForceIgnoreSharding && !string.IsNullOrEmpty(SiteDBServer))
            {
                dbServer = SiteDBServer;
                return true;
            }
            dbServer = null;
            return false;
        }

        public string GetShardSPKey(string key) => $"SiteDB_{key.Substring(_prefixSharding.Length)}";

        private bool IsSharding(string key) => key.StartsWith(_prefixSharding, StringComparison.OrdinalIgnoreCase);

        private bool ForceIgnoreSharding => _contextService.ForceIgnoreSharding;

        private string SiteDBServer => _contextService.SiteDBServer;
    }
}