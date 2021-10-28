namespace Fanex.Athena.Report.BySite.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.Excels.Interfaces;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Models;
    using Fanex.Athena.Report.BySite.Services.Interfaces;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class LeagueWinLossController : ControllerBase
    {
        [HttpPost]
        public async Task<LeagueWinLossResultModel> GetMainData(LeagueWinLossMetaModel metaModel, [FromServices] ILeagueWinLossService service)
        {
            var result = await service.GetDataAsync(metaModel).ConfigureAwait(false);

            if (!result.DataList.Any())
            {
                return GetEmptyResult();
            }

            return new LeagueWinLossResultModel
            {
                Data = result.DataList.ToList(),
                Total = result.TotalData
            };
        }

        private static dynamic GetEmptyResult()
        {
            return new LeagueWinLossResultModel
            {
                Data = new List<LeagueWinLoss>(),
                Total = new LeagueWinLoss()
            };
        }

        [HttpPost]
        public async Task<LeagueWinLossResultModel> GetCurrencyData(LeagueWinLossMetaModel metaModel, [FromServices] ILeagueWinLossService service)
        {
            var result = await service.GetCurrencyDataAsync(metaModel).ConfigureAwait(false);

            if (!result.DataList.Any())
            {
                return GetEmptyResult();
            }

            return new LeagueWinLossResultModel
            {
                Data = result.DataList.ToList()
            };
        }

        [HttpPost]
        public async Task<FileContentResult> ExportExcel(
            [FromServices] ILeagueWinLossService service,
            [FromServices] IFileProvider file,
            [FromServices] ILeagueWinLossExcel excel,
            LeagueWinLossMetaModel metaModel)
        {
            var template = file.GetFileInfo("/ExcelTemplate/LeagueWinLoss.xls").PhysicalPath;

            var result = await service.GetDataAsync(metaModel).ConfigureAwait(false);

            var fileContent = excel.Export(template, result, metaModel);

            return new FileContentResult(fileContent, "application/octet-stream");
        }
    }
}