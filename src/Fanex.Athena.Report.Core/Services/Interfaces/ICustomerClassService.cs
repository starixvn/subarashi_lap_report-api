namespace Fanex.Athena.Report.Core.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models.DTOs;

    public interface ICustomerClassService
    {
        Task<IEnumerable<RiskLevel>> GetAllRiskLevel();
    }
}
