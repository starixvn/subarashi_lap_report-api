using System.Threading.Tasks;

namespace Fanex.Athena.Report.Core.ApiClients.Abstraction
{
    public interface IBetListApiClient
    {
        Task<T> PostAsync<T>(string path, object value, string language);

        Task<string> PostStringAsync(string path, object value, string language);

        Task<byte[]> PostByteArrayAsync(string path, object value, string language);
    }
}
