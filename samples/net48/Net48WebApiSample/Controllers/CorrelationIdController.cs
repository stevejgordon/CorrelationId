using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using CorrelationId;
using Microsoft.Extensions.Options;

namespace Net48WebApiSample.Controllers
{
    public class CorrelationIdController : ApiController
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptions<CorrelationIdOptions> _correlationIdOptions;

        public CorrelationIdController(IHttpClientFactory httpClientFactory,
            IOptions<CorrelationIdOptions> correlationIdOptions)
        {
            _httpClientFactory = httpClientFactory;
            _correlationIdOptions = correlationIdOptions;
        }

        public async Task<IHttpActionResult> Get()
        {
            Request.Headers.TryGetValues(_correlationIdOptions.Value.RequestHeader, out var correlationIds);
            var correlationId = correlationIds.SingleOrDefault();

            if (correlationId == null)
            {
                return new StatusCodeResult(HttpStatusCode.NoContent, Request);
            }

            var client =
                _httpClientFactory.CreateClient("MyClient"); // this client will attach the correlation ID header

            var innerResponse = await client.GetAsync("https://www.example.com");

            innerResponse.Headers.TryGetValues(_correlationIdOptions.Value.RequestHeader,
                out var innerResponseCorrelationIds);
            var innerResponseCorrelationId = innerResponseCorrelationIds.SingleOrDefault();

            if (innerResponseCorrelationId != correlationId)
            {
                return Conflict();
            }

            return Ok(correlationId);
        }
    }
}