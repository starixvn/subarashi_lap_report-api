using Fanex.Athena.Report.BySite.Excels.Interfaces;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;
using Fanex.Athena.Report.BySite.Services.Interfaces;
using Fanex.Athena.Report.Core.Attributes;
using Fanex.Athena.Report.Core.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using System.Linq;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.BySite.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.PlayerSabaCoins)]
    [RequestValidation(validateCurrencyList: true)]
    [AutoBindingMetaModel(bindAgent: true)]
    public class PlayerSabaCoinsController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> GetData([FromBody] PlayerSabaCoinsMetaModel metaModel, [FromServices] IPlayerSabaCoinsService service)
        {
            var result = await service.GetDataAsync(metaModel).ConfigureAwait(false);

            return Ok(new PlayerSabaCoinsResultModel
            {
                Data = result.DataList.ToList(),
                TotalSummary = result.TotalData,
                TotalRows = result.TotalRows
            });
        }

        [HttpPost]
        public async Task<FileContentResult> ExportExcel(
            [FromServices] IPlayerSabaCoinsService service,
            [FromServices] IFileProvider file,
            [FromServices] IPlayerSabaCoinsExcel excel,
            [FromBody] PlayerSabaCoinsMetaModel metaModel)
        {
            var template = file.GetFileInfo("/ExcelTemplate/PlayerSabaCoins.xls").PhysicalPath;
            var result = await service.GetDataAsync(metaModel).ConfigureAwait(false);
            var fileContent = excel.Export(template, result, metaModel);

            return new FileContentResult(fileContent, "application/octet-stream");
        }
    }
}