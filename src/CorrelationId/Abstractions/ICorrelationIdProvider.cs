using Microsoft.AspNetCore.Http;

namespace CorrelationId.Abstractions
{
    /// <summary>
    /// Defines a provider which can be used to generate correlation IDs.
    /// </summary>
    public interface ICorrelationIdProvider
    {
        /// <summary>
        /// Generates a correlation ID string for the current request.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> of the current request.</param>
        /// <returns>A string representing the correlation ID.</returns>
        string GenerateCorrelationId(HttpContext context);
    }
}
