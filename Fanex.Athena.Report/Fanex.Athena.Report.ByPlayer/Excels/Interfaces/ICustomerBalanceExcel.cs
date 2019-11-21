namespace Fanex.Athena.Report.ByPlayer.Excels.Interfaces
{
    using System.IO;
    using Fanex.Athena.Report.ByPlayer.DTO;
    using Fanex.Athena.Report.ByPlayer.MetaModels;
    using Fanex.Athena.Report.Common.Model.ViewModel;

    public interface ICustomerBalanceExcel
    {
        Stream Export(string template, ResultModel<CustomerBalance> data, CustomerBalanceMetaModel metaModel);
    }
}