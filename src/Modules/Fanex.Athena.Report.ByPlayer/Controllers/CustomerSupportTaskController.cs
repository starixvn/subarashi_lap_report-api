namespace Fanex.Athena.Report.ByPlayer.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.Attributes;
    using Core.Constants;
    using Core.Services.Interfaces;
    using Excels.Interfaces;
    using MetaModels;
    using Microsoft.AspNetCore.Mvc;

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
        public ActionResult Index([FromServices] IContextService contextService)
        {
            return Ok(new
            {
                currencies = contextService.CurrencyList.ToArray()
            });
        }

        [HttpPost]
        [RequestValidation(validateCurrencyList: true)]
        [AutoBindingMetaModel(bindAgent: true)]
        public async Task<FileResult> ExportExcel([FromBody] CustomerSupportTaskMetaModel metaModel)
        {
            var excelHandler = _customerSupportTaskExcels.FirstOrDefault(_ => _.Should(metaModel));
            if (excelHandler == default)
            {
                return File(Array.Empty<byte>(), "application/octet-stream");
            }

            var fileName = await excelHandler.CreateZipFile(metaModel);
            var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
            return File(stream, "application/octet-stream");
        }
    }
}