namespace Fanex.Athena.Report.ByPlayer.Excels.Interfaces
{
    using System.Threading.Tasks;
    using Fanex.Athena.Report.ByPlayer.MetaModels;

    public interface ICustomerBalanceExcel
    {
        Task<string> Export(CustomerBalanceMetaModel metaModel);
    }
}