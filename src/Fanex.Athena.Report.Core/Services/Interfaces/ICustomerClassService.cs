using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Athena.Report.Core.Models.DTOs;

namespace Fanex.Athena.Report.Core.Services.Interfaces
{
    public interface ICustomerClassService
    {
        Task<IEnumerable<RiskLevel>> GetAllRiskLevel();
    }
}
