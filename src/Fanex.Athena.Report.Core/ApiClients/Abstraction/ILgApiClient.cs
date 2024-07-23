using System.Threading.Tasks;

namespace Fanex.Athena.Report.Core.ApiClients.Abstraction
{
    public interface ILgApiClient
    {
        Task<T> PostAsync<T>(string path, object value, string language);
    }
}
