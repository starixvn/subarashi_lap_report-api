namespace Fanex.Athena.Report.Common.Services.Interfaces
{
    using System.Threading.Tasks;
    using Model.DTO;

    public interface IBalanceUpService
    {
        Task<BalanceUp> GetBalanceUpAsync();
    }
}