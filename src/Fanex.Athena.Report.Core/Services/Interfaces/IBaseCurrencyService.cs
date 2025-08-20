namespace Fanex.Athena.Report.Core.Services.Interfaces
{
    using System.Collections.Generic;

    public interface IBaseCurrencyService
    {
        IDictionary<int, string> GetBaseCurrencyList(bool supportOnceCurrencyToBase = false);
    }
}
