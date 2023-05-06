using System.Net.Http;
using CorrelationId.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace CorrelationId.HttpClient
{
    /// <summary>
    /// A <see cref="DelegatingHandler"/> which adds the correlation ID header from the <see cref="CorrelationContext"/> onto outgoing HTTP requests.
    /// And on its response if configured.
    /// </summary>
    internal sealed class CorrelationIdHandler : DelegatingHandler
    {
        private readonly ICorrelationContextAccessor _correlationContextAccessor;
        private readonly IOptions<CorrelationIdOptions> _correlationIdOptions;

        public CorrelationIdHandler(ICorrelationContextAccessor correlationContextAccessor,
            IOptions<CorrelationIdOptions> correlationIdOptions)
        {
            _correlationContextAccessor = correlationContextAccessor;
            _correlationIdOptions = correlationIdOptions;
        }

        /// <inheritdoc cref="DelegatingHandler"/>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var correlationId = _correlationContextAccessor?.CorrelationContext?.CorrelationId;
            if (!string.IsNullOrEmpty(correlationId) 
                && !request.Headers.Contains(_correlationContextAccessor.CorrelationContext.Header))
            {
                request.Headers.Add(_correlationContextAccessor.CorrelationContext.Header,
                    _correlationContextAccessor.CorrelationContext.CorrelationId);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (_correlationIdOptions.Value.IncludeInResponse &&
                !string.IsNullOrEmpty(correlationId)
                && !response.Headers.Contains(_correlationIdOptions.Value.ResponseHeader))
            {
                response.Headers.Add(_correlationIdOptions.Value.ResponseHeader,
                    _correlationContextAccessor.CorrelationContext.CorrelationId);
            }

            return response;
        }
    }
}
