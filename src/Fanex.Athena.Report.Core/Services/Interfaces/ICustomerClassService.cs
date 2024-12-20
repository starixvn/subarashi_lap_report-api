using Fanex.Athena.Report.Core.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.Core.Services.Interfaces
{
    public interface ICustomerClassService
    {
        Task<IEnumerable<RiskLevel>> GetAllRiskLevel();
    }
}
