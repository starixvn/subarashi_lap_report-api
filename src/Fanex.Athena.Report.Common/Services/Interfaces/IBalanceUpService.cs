namespace Fanex.Athena.Report.Common.Services.Interfaces
{
    using Fanex.Athena.Report.Common.Model.DTO;
    using System.Threading.Tasks;

    public interface IBalanceUpService
    {
        Task<BalanceUp> GetBalanceUpAsync();
    }
}