namespace Fanex.Athena.Report.ByPlayer.Controllers
{
    using System.Threading.Tasks;
    using Core.Helpers;
    using Core.Services.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    using Service.Interfaces;

    [ApiController, Route("api/[controller]/[action]"),]
    public class SearchCustomerController : ControllerBase
    {
        private readonly IContextService _contextService;

        private readonly ISearchCustomerService _searchCustomerService;

        public SearchCustomerController(
            ISearchCustomerService searchCustomerService, IContextService contextService)
        {
            _searchCustomerService = searchCustomerService;
            _contextService = contextService;
        }

        [HttpGet]
        public async Task<IActionResult> SearchCustomer(string query)
        {
            var result = await Search(query, false);

            return result;
        }

        private async Task<IActionResult> Search(string query, bool isIncludeDormant)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return null;
            }

            var userName = query.Trim();
            var customerNames = await _searchCustomerService.SearchCustomers(_contextService.UserId, userName,
                AuthParameterHelper.GenerateAgentListXml(_contextService.AgentList), isIncludeDormant);

            var searchResult = new
            {
                data = new[] { string.Empty, },
                query = userName,
                suggestions = customerNames
            };

            return Ok(searchResult);
        }
    }
}
