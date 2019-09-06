namespace Fanex.Athena.Report.ByPlayer.Controllers
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/player/[controller]")]
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
