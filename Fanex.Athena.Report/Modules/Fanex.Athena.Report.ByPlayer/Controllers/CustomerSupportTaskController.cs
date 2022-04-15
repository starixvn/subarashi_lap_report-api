using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Athena.Report.ByPlayer.Excels.Interfaces;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.Core.Attributes;
using Fanex.Athena.Report.Core.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Fanex.Athena.Report.ByPlayer.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.CustomerSupportTask)]
    public class CustomerSupportTaskController : ControllerBase
    {
        private readonly IEnumerable<ICustomerSupportTaskExcel> _customerSupportTaskExcels;

        public CustomerSupportTaskController(IEnumerable<ICustomerSupportTaskExcel> customerSupportTaskExcels)
        {
            _customerSupportTaskExcels = customerSupportTaskExcels;
        }

        [HttpPost]
        [RequestValidation(validateCurrencyList: true)]
        [AutoBindingMetaModel(bindAgent: true)]
        public async Task<FileResult> ExportExcel([FromBody] CustomerSupportTaskMetaModel metaModel)
        {
            var excelHandler = _customerSupportTaskExcels.FirstOrDefault(_ => _.Should(metaModel));
            if(excelHandler == default)
            {
                return File(new byte[] { }, "application/octet-stream");
            }

            var fileName = await excelHandler.CreateZipFile(metaModel);
            var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
            return File(stream, "application/octet-stream");
        }
    }
}