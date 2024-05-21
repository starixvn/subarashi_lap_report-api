using Fanex.Data;

namespace Fanex.Athena.Report.Core.Repositories.Abstraction
{
    public interface IDbFactory
    {
        IObjectDb GetObjectDb(string spName, int timeout = 120);
    }
}