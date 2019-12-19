using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CorrelationId.HttpClient
{
    /// <summary>
    /// Extension methods for <see cref="IHttpClientBuilder"/>.
    /// </summary>
    public static class HttpClientBuilderExtensions
    {
        /// <summary>
        /// Adds a handler which forwards the correlation ID by attaching it to the headers on outgoing requests.
        /// </summary>
        /// <remarks>
        /// The header name will match the name of the incoming request header.
        /// </remarks>
        /// <param name="builder">The <see cref="IHttpClientBuilder"/>.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IHttpClientBuilder AddCorrelationIdForwarding(this IHttpClientBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.TryAddTransient<CorrelationIdHandler>();
            builder.AddHttpMessageHandler<CorrelationIdHandler>();
            
            return builder;
        }
    }
}