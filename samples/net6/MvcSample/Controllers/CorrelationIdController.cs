using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CorrelationId;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MvcSample.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CorrelationIdController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptions<CorrelationIdOptions> _correlationIdOptions;

        public CorrelationIdController(IHttpClientFactory httpClientFactory,
            IOptions<CorrelationIdOptions> correlationIdOptions)
        {
            _httpClientFactory = httpClientFactory;
            _correlationIdOptions = correlationIdOptions;
        }

        [HttpGet]
        public async Task<IResult> Get()
        {
            var correlationId = Request.Headers[_correlationIdOptions.Value.RequestHeader].SingleOrDefault();

            var client =
                _httpClientFactory.CreateClient("MyClient"); // this client will attach the correlation ID header

            var innerResponse = await client.GetAsync("https://www.example.com");

            innerResponse.Headers.TryGetValues(_correlationIdOptions.Value.RequestHeader,
                out var innerResponseCorrelationIds);
            var innerResponseCorrelationId = innerResponseCorrelationIds?.SingleOrDefault();

            if (innerResponseCorrelationId != correlationId)
            {
                return Results.Conflict();
            }

            return Results.Ok(correlationId);
        }
    }
}