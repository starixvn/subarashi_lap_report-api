using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;

namespace Fanex.Athena.Report.ByPlayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WinLossController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

    }
}
