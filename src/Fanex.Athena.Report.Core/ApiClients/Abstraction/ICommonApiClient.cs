using System.Threading.Tasks;

namespace Fanex.Athena.Report.Core.ApiClients.Abstraction
{
    public interface ICommonApiClient
    {
        Task<T> GetAsync<T>(string path, object value = null, int timeOut = 100);
    }
}