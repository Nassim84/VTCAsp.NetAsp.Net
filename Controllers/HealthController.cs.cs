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
                status = "healthy", // ou "OK" comme tu préfères
                timestamp = DateTime.UtcNow, // UTC pour cohérence
                service = "VTC Backend"
            });
        }
    }
}