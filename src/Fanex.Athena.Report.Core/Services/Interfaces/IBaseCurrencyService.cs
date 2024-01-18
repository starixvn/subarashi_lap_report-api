using System.Collections.Generic;

namespace Fanex.Athena.Report.Core.Services.Interfaces
{
    public interface IBaseCurrencyService
    {
        IDictionary<int, string> GetBaseCurrencyList(bool supportOnceCurrencyToBase = false);
    }
}
