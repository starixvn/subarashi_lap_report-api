namespace Fanex.Athena.Report.Core.ApiClients.Abstraction
{
    using System.Threading.Tasks;

    public interface ICommonApiClient
    {
        Task<T> GetAsync<T>(string path, object value = null, int timeOut = 100);
    }
}