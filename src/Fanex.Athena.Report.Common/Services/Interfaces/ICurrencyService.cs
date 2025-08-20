namespace Fanex.Athena.Report.Common.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Model.DTO;

    public interface ICurrencyService
    {
        Task<IEnumerable<Currency>> GetCurrencyListAsync();
    }
}