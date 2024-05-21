using System.Collections.Generic;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Repositories.Abstraction;
using Fanex.Athena.Report.Resources;
using Fanex.Athena.Report.Resources.Helpers;
using Fanex.Data;

namespace Fanex.Athena.Report.Core.Repositories
{
    public class ProductRepository : IProductRepository
    {
        public IList<ProductInfo> GetProductAccessList(int licenseeId)
        {
            // All licensees have product Sb
            var allowedProducts = new List<ProductInfo> { new() { IsOfferedSingleWallet = true, ProductCode = "SB", ProductName = ViewResources.Sportsbook } };

            using var db = ObjectDbFactory.CreateInstance("Lrf_LicenseeProduct3rdSel");
            db.SetParameterValue("licenseeid", licenseeId);

            using var reader = db.ExecuteReader();
            while (reader.Read())
            {
                allowedProducts.Add(new ProductInfo
                {
                    ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
                    ProductName = DataHelper.GetProductName(reader.GetString(reader.GetOrdinal("productprefix"))),
                    ProductCode = reader.GetString(reader.GetOrdinal("productprefix")),
                    AllowCommission = reader.GetBoolean(reader.GetOrdinal("comm")),
                    AllowWinloss = reader.GetBoolean(reader.GetOrdinal("winloss")),
                    IsOfferedSingleWallet = reader.GetBoolean(reader.GetOrdinal("IsOfferedSingleWallet"))
                });
            }

            return allowedProducts;
        }
    }
}