using System.Threading.Tasks;

namespace Fanex.Athena.Report.Core.Services
{
    public interface ILicenseeService
    {
        Task<bool> CheckProductBelongsToFromDB(string licenseeName, int productId);

    }
}
