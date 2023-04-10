using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MvcSample
{
    public class NoOpDelegatingHandler : DelegatingHandler
    {
        private readonly ILogger<NoOpDelegatingHandler> _logger;

        public NoOpDelegatingHandler(ILogger<NoOpDelegatingHandler> logger) => _logger = logger;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Headers.TryGetValues("X-Correlation-Id", out var headerEnumerable))
            {
                _logger.LogInformation("Request has the following correlation ID header {CorrelationId}.", headerEnumerable.FirstOrDefault());
            }
            else
            {
                _logger.LogInformation("Request does not have a correlation ID header.");
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK);

            return Task.FromResult(response);
        }
    }
}
