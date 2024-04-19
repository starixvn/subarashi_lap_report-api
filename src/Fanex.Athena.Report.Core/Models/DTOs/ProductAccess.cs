using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Athena.Report.Resources.Constants;

namespace Fanex.Athena.Report.Core.Models.DTOs
{
    [Serializable]
    public class ProductAccess
    {
        public ProductAccess(IList<ProductInfo> productList)
        {
            ProductList = new ReadOnlyCollection<ProductInfo>(productList);
        }

        public ReadOnlyCollection<ProductInfo> ProductList { get; private set; }

        public IList<ProductOption> SelectProductOptions(Func<ProductInfo, bool> predicate = null)
        {
            predicate ??= _ => true;

            return ProductList
                  .Where(predicate)
                  .Select(i => new ProductOption
                   {
                       Code = i.ProductCode,
                       Name = i.ProductName,
                       Status = true
                   })
                  .ToList();
        }

        public bool IsAllowNg {
            get { return ProductList.Any(p => p.ProductCode == Product.NG); }
        }

        public bool IsAllowVs {
            get { return ProductList.Any(p => p.ProductCode == Product.VS); }
        }

        public bool IsAllowBa
        {
            get { return ProductList.Any(p => p.ProductCode == Product.BA); }
        }

        public bool IsAllowSb
        {
            get { return ProductList.Any(p => p.ProductCode == Product.SB); }
        }


        public bool IsAllowKn
        {
            get
            {
                return ProductList.Any(p => p.ProductCode == Product.KN);
            }
        }

        public bool IsAllow(int productId)
        {
            return ProductList.Any(p => p.ProductID == productId);
        }

        public bool IsAllow(string productCode)
        {
            return ProductList.Any(p => p.ProductCode == productCode);
        }
    }
}
