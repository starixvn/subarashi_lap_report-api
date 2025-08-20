namespace Fanex.Athena.Report.ByPlayer.Excels.Interfaces
{
    using System.Threading.Tasks;
    using MetaModels;

    public interface ICustomerBalanceExcel
    {
        Task<string> Export(CustomerBalanceMetaModel metaModel);
    }
}