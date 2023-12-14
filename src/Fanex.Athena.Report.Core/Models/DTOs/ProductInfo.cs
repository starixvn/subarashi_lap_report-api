using Fanex.Athena.Report.Resources.Constants;

namespace Fanex.Athena.Report.Core.Models.DTOs
{
    public class ProductInfo
    {
        public ProductInfo()
        {
            ProductCode = Product.SB;
            ProductName = nameof(Product.SB);
            AllowCommission = true;
            AllowWinloss = true;
        }

        public int ProductID { get; set; }

        public string ProductName { get; set; }

        public string ProductCode { get; set; }

        public bool AllowCommission { get; set; }

        public bool AllowWinloss { get; set; }

        public bool IsOfferedSingleWallet { get; set; }
    }
}
