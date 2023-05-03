using Microsoft.AspNetCore.Mvc;

namespace MvcSample.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CorrelationIdController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            var correlationId = Request.Headers["X-Correlation-Id"];
            return correlationId;
        }
    }
}
