using System.Collections.Generic;
using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace MvcSample.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly ScopedClass _scoped;
        private readonly TransientClass _transient;
        private readonly SingletonClass _singleton;
        private readonly ICorrelationContextAccessor _correlationContext;

        public ValuesController(ScopedClass scoped, TransientClass transient, SingletonClass singleton, ICorrelationContextAccessor correlationContext)

        {
            _scoped = scoped;
            _transient = transient;
            _singleton = singleton;
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
                $"Transient={_transient.GetCorrelationFromScoped}",
                $"Scoped={_scoped.GetCorrelationFromScoped}",
                $"Singleton={_singleton.GetCorrelationFromScoped}",
                $"TraceIdentifier={HttpContext.TraceIdentifier}"
            };
        }
    }
}
