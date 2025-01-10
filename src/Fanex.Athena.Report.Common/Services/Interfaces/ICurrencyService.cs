namespace Fanex.Athena.Report.Common.Services.Interfaces
{
    using Fanex.Athena.Report.Common.Model.DTO;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ICurrencyService
    {
        Task<IEnumerable<Currency>> GetCurrencyListAsync();
    }
}