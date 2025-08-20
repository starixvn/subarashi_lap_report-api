namespace Fanex.Athena.Report.DbSettingConfig
{
    using System;
    using System.Collections.Generic;
    using Core.Services.Interfaces;

    public interface IShardInfoAccessor
    {
        bool TryUseShardDB(string key, out string dbServer);
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

        private bool IsSharding(string key) => key.StartsWith(_prefixSharding, StringComparison.OrdinalIgnoreCase);

        private bool ForceIgnoreSharding => _contextService.ForceIgnoreSharding;

        private string SiteDBServer => _contextService.SiteDBServer;
    }
}