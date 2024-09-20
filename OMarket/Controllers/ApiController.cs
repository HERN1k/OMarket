using Microsoft.AspNetCore.Mvc;

namespace OMarket.Controllers
{
    [ApiController]
    [Route("api")]
    public class ApiController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok(new { Result = "OK" });

        [HttpGet("uptime")]
        public IActionResult Uptime() => Ok(new { Uptime = DateTime.UtcNow - Program.StartupTime });

        [HttpGet("time")]
        public IActionResult Time() => Ok(new { Time = DateTime.Now });

        [HttpGet("timeutc")]
        public IActionResult TimeUtc() => Ok(new { TimeUtc = DateTime.UtcNow });
    }
}