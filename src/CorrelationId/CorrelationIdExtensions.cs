using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using System;

namespace CorrelationId
{
    /// <summary>
    /// Extension methods for the CorrelationIdMiddleware
    /// </summary>
    public static class CorrelationIdExtensions
    {
        /// <summary>
        /// Enables correlation IDs for the request
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseCorrelationId(new CorrelationIdOptions());
        }

        /// <summary>
        /// Enables correlation IDs for the request
        /// </summary>
        /// <param name="app"></param>
        /// <param name="header">The header field name to use for the correlation ID.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app, string header)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseCorrelationId(new CorrelationIdOptions
            {
                Header = header
            });
        }

        /// <summary>
        /// Enables correlation IDs for the request
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app, CorrelationIdOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (app.ApplicationServices.GetService(typeof(ICorrelationContextFactory)) == null)
            {
                throw new InvalidOperationException("Unable to find the required services. You must call the AddCorrelationId method in ConfigureServices in the application startup code.");
            }

            return app.UseMiddleware<CorrelationIdMiddleware>(Options.Create(options));
        }
    }
}
