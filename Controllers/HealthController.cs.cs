using Microsoft.AspNetCore.Mvc;

namespace MonBackendVTC.Controllers
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
                status = "healthy", 
                timestamp = DateTime.UtcNow, 
                service = "VTC Backend"
            });
        }
    }
}