namespace Fanex.Athena.Report.Common.Services.Interfaces
{
    using System.Threading.Tasks;
    using Fanex.Athena.Report.Common.Model.DTO;

    public interface IBalanceUpService
    {
        Task<BalanceUp> GetBalanceUpAsync();
    }
}
