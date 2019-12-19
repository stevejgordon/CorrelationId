using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Builder;
using System;

namespace CorrelationId
{
    /// <summary>
    /// Extension methods for the CorrelationIdMiddleware.
    /// </summary>
    public static class CorrelationIdExtensions
    {
        /// <summary>
        /// Enables correlation IDs for the request.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (app.ApplicationServices.GetService(typeof(ICorrelationContextFactory)) == null)
            {
                throw new InvalidOperationException("Unable to find the required services. You must call the appropriate AddCorrelationId/AddDefaultCorrelationId method in ConfigureServices in the application startup code.");
            }

            return app.UseMiddleware<CorrelationIdMiddleware>();
        }
    }
}
