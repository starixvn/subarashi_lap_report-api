using System;
using System.Threading.Tasks;
using Fanex.Data;

namespace Fanex.Athena.Report.Core.Services
{
    public class LicenseeService : ILicenseeService
    {
        public async Task<bool> CheckProductPermission(string licenseeName, int productId)
        {
            const string UspName = "Lrf_CheckBelonging_ValidateProduct";

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                db.SetParameterValue("productId", productId);
                db.SetParameterValue("licenseeName", licenseeName);

                return Convert.ToBoolean(await db.ExecuteScalarAsync());
            }
        }
    }
}