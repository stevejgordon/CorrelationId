using System.Threading.Tasks;
using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Http;

namespace CorrelationId.Providers
{
    /// <summary>
    /// Sets the correlation ID to match the TraceIdentifier set on the <see cref="HttpContext"/>.
    /// </summary>
    public class TraceIdCorrelationIdProvider : ICorrelationIdProvider
    {
        /// <inheritdoc />
        public Task<string> GenerateCorrelationId(HttpContext ctx) => Task.FromResult(ctx.TraceIdentifier);
    }
}