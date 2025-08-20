namespace Fanex.Athena.Report.Core.Repositories
{
    using System.Collections.Generic;
    using Abstraction;
    using Data;
    using Models.DTOs;

    public class ProductRepository : IProductRepository
    {
        public IList<ProductInfo> GetProductAccessList(int licenseeId)
        {
            // All licensees have product Sb
            var allowedProducts = new List<ProductInfo> { new() { IsOfferedSingleWallet = true, ProductCode = "SB" } };

            using var db = ObjectDbFactory.CreateInstance("Lrf_LicenseeProduct3rdSel");
            db.SetParameterValue("licenseeid", licenseeId);

            using var reader = db.ExecuteReader();
            while (reader.Read())
            {
                allowedProducts.Add(new ProductInfo
                {
                    ProductID = reader.GetInt32(reader.GetOrdinal("ProductID")),
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