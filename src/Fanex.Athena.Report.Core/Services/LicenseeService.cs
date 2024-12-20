using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.Core.Services
{
    public class LicenseeService : ILicenseeService
    {
        public async Task<bool> HasPermissionOnData(IReadOnlyDictionary<int, int[]> currencyAgents, int custId, long transId)
        {
            if (custId == 0 && transId == 0)
            {
                return true;
            }

            var item = await IsCustomerBelonging(custId, transId);

            return currencyAgents.Any(i => i.Key == item.Item1 && i.Value.Contains(item.Item2));
        }

        public async Task<Tuple<int, int>> IsCustomerBelonging(int custId, long transId)
        {
            using (var db = ObjectDbFactory.CreateInstance("Lrf_CheckBelonging_GetAgentInfo"))
            {
                db.SetParameterValue("transid", transId);
                db.SetParameterValue("custid", custId);
                using (var reader = await db.ExecuteReaderAsync())
                {
                    if (reader.Read())
                    {
                        return new Tuple<int, int>(Convert.ToInt32(reader["CurrencyId"]), Convert.ToInt32(reader["CustId"]));
                    }
                }
            }

            return new Tuple<int, int>(0, 0);
        }

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