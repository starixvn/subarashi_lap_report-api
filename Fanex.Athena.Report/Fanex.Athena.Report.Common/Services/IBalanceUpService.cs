using System.Threading.Tasks;

using Fanex.Athena.ExtService.Common.Model.DTO;

namespace Fanex.Athena.Report.Common.Services
{
    public interface IBalanceUpService
    {
        Task<BalanceUp> GetBalanceUpAsync();

    }
}
