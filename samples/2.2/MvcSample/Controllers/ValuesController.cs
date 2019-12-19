using System.Collections.Generic;
using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace MvcSample.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly ICorrelationContextAccessor _correlationContext;

        public ValuesController(ICorrelationContextAccessor correlationContext)

        {
            _correlationContext = correlationContext;
        }

        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            var correlation = _correlationContext.CorrelationContext.CorrelationId;

            return new []
            {
                $"DirectAccessor={correlation}",
                $"TraceIdentifier={HttpContext.TraceIdentifier}"
            };
        }
    }
}
