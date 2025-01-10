namespace Fanex.Athena.Report.ByPlayer.Excels.Interfaces
{
    using Fanex.Athena.Report.ByPlayer.MetaModels;
    using System.Threading.Tasks;

    public interface ICustomerBalanceExcel
    {
        Task<string> Export(CustomerBalanceMetaModel metaModel);
    }
}