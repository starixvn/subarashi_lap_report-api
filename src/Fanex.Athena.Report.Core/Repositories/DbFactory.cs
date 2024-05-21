using Fanex.Athena.Report.Core.Repositories.Abstraction;
using Fanex.Data;

namespace Fanex.Athena.Report.Core.Repositories
{
    public class DbFactory : IDbFactory
    {
        public IObjectDb GetObjectDb(string spName, int timeout = 120)
            => ObjectDbFactory.CreateInstance(spName, timeout);
    }
}