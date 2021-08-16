using System;
using System.Threading.Tasks;
using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Http;

namespace CorrelationId.Providers
{
    /// <summary>
    /// Generates a correlation ID using a new GUID.
    /// </summary>
    public class GuidCorrelationIdProvider : ICorrelationIdProvider
    {
        /// <inheritdoc />
        public Task<string> GenerateCorrelationId(HttpContext _) => Task.FromResult(Guid.NewGuid().ToString());
    }
}