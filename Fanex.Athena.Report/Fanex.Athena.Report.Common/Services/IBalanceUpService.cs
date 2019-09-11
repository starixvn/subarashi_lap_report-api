namespace Fanex.Athena.Report.Common.Services
{
    using System.Threading.Tasks;
    using Fanex.Athena.ExtService.Common.Model.DTO;

    public interface IBalanceUpService
    {
        Task<BalanceUp> GetBalanceUpAsync();
    }
}
