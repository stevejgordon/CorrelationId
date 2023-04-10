using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CorrelationId;
using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Net48CorrelationId
{
    /// <summary>
    /// We don't have any middleware in .net framework so we wrap it in a HttpMessageHandler instead
    /// </summary>
    public class CorrelationIdMiddlewareWrapper : DelegatingHandler
    {
        private readonly CorrelationIdMiddleware _correlationIdMiddleware;
        private readonly ICorrelationContextFactory _correlationContextFactory;
        private readonly IOptions<CorrelationIdOptions> _correlationIdOptions;

        public CorrelationIdMiddlewareWrapper(CorrelationIdMiddleware correlationIdMiddleware,
            ICorrelationContextFactory correlationContextFactory, IOptions<CorrelationIdOptions> correlationIdOptions)
        {
            _correlationIdMiddleware = correlationIdMiddleware;
            _correlationContextFactory = correlationContextFactory;
            _correlationIdOptions = correlationIdOptions;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var httpContext = GetHttpContext();
            await _correlationIdMiddleware.Invoke(httpContext, _correlationContextFactory);

            var correlationIds =
                httpContext.Request.Headers[_correlationIdOptions.Value.RequestHeader];

            foreach (var correlationId in correlationIds)
            {
                request.Headers.Add(_correlationIdOptions.Value.RequestHeader, correlationId);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (!_correlationIdOptions.Value.IncludeInResponse)
            {
                return response;
            }

            response.Headers.TryGetValues(_correlationIdOptions.Value.ResponseHeader, out var existingCorrelationIdResponseHeaders);
            var existingCorrelationIdResponseHeadersList = existingCorrelationIdResponseHeaders?.ToList();

            foreach (var correlationId in correlationIds)
            {
                // CorrelationId might be added both on server and client part
                if (existingCorrelationIdResponseHeadersList != null &&
                    existingCorrelationIdResponseHeadersList.Any(x => x == correlationId))
                {
                    continue;
                }
                
                response.Headers.Add(_correlationIdOptions.Value.ResponseHeader, correlationId);
            }

            return response;
        }

        private HttpContext GetHttpContext()
        {
            var httpContext = new DefaultHttpContext();

            if (System.Web.HttpContext.Current == null)
            {
                return httpContext;
            }

            var correlationIds =
                System.Web.HttpContext.Current.Request.Headers
                    .GetValues(_correlationIdOptions.Value.RequestHeader);

            if (correlationIds == null)
            {
                return httpContext;
            }

            foreach (var correlationId in correlationIds)
            {
                httpContext.Request.Headers
                    .Add(_correlationIdOptions.Value.RequestHeader, correlationId);
            }

            return httpContext;
        }
    }
}