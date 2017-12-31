using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace CorrelationId
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IOptions<CorrelationIdOptions> _options;

        /// <summary>
        /// Creates a new instance of the CorrelationIdMiddleware.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="options">The configuration options.</param>
        public CorrelationIdMiddleware(RequestDelegate next, IOptions<CorrelationIdOptions> options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Processes a request to synchronise TraceIdentifier and Correlation ID headers. Also creates a 
        /// <see cref="CorrelationContext"/> for the current request and disposes of it when the request is completing.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
        /// <param name="correlationContextFactory">The <see cref="ICorrelationContextFactory"/> which can create a <see cref="CorrelationContext"/>.</param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context, ICorrelationContextFactory correlationContextFactory)
        {
            if (context.Request.Headers.TryGetValue(_options.Value.Header, out var correlationId))
            {
                context.TraceIdentifier = correlationId;
            }

            correlationContextFactory.Create(context.TraceIdentifier);

            if (_options.Value.IncludeInResponse)
            {
                // apply the correlation ID to the response header for client side tracking
                context.Response.OnStarting(() =>
                {
                    if (!context.Response.Headers.ContainsKey(_options.Value.Header))
                    {
                        context.Response.Headers.Add(_options.Value.Header, context.TraceIdentifier);
                    }
                   
                    return Task.CompletedTask;
                });
            }

            await _next(context);

            correlationContextFactory.Dispose();
        }
    }
}
