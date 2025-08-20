namespace Fanex.Athena.Report.Core.Services.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ILicenseeService
    {
        Task<bool> HasPermissionOnData(IReadOnlyDictionary<int, int[]> currencyAgents, int custId, long transId);

        Task<bool> CheckProductPermission(string licenseeName, int productId);

        Task<Tuple<int, int>> IsCustomerBelonging(int custId, long transId);
    }
}