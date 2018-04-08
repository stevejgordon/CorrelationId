using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace CorrelationId
{
    /// <summary>
    /// Middleware which attempts to reads / creates a Correlation ID that can then be used in logs and 
    /// passed to upstream requests.
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly CorrelationIdOptions _options;

        /// <summary>
        /// Creates a new instance of the CorrelationIdMiddleware.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="options">The configuration options.</param>
        public CorrelationIdMiddleware(RequestDelegate next, IOptions<CorrelationIdOptions> options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Processes a request to synchronise TraceIdentifier and Correlation ID headers. Also creates a 
        /// <see cref="CorrelationContext"/> for the current request and disposes of it when the request is completing.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
        /// <param name="correlationContextFactory">The <see cref="ICorrelationContextFactory"/> which can create a <see cref="CorrelationContext"/>.</param>
        public async Task Invoke(HttpContext context, ICorrelationContextFactory correlationContextFactory)
        {
            var correlationId = SetCorrelationId(context);

            if (_options.UpdateTraceIdentifier)
                context.TraceIdentifier = correlationId;

            correlationContextFactory.Create(correlationId, _options.Header);

            if (_options.IncludeInResponse)
            {
                // apply the correlation ID to the response header for client side tracking
                context.Response.OnStarting(() =>
                {
                    if (!context.Response.Headers.ContainsKey(_options.Header))
                    {
                        context.Response.Headers.Add(_options.Header, correlationId);
                    }

                    return Task.CompletedTask;
                });
            }

            await _next(context);

            correlationContextFactory.Dispose();
        }

        private StringValues SetCorrelationId(HttpContext context)
        {
            var correlationIdFoundInRequestHeader = context.Request.Headers.TryGetValue(_options.Header, out var correlationId);

            if (RequiresGenerationOfCorrelationId(correlationIdFoundInRequestHeader, correlationId))
                correlationId = GenerateCorrelationId(context.TraceIdentifier);

            return correlationId;
        }

        private static bool RequiresGenerationOfCorrelationId(bool idInHeader, StringValues idFromHeader) => 
            !idInHeader || StringValues.IsNullOrEmpty(idFromHeader);

        private StringValues GenerateCorrelationId(string traceIdentifier) => 
            _options.UseGuidForCorrelationId || string.IsNullOrEmpty(traceIdentifier) ? Guid.NewGuid().ToString() : traceIdentifier;
    }
}
