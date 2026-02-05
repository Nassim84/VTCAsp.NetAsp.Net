using Microsoft.AspNetCore.Mvc;

namespace Uber.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Ping()
        {
            return Ok(new
            {
                status = "OK",
                time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }
    }
}