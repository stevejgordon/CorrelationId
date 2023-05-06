using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Http;

namespace MvcSample
{
    public class DoNothingCorrelationIdProvider : ICorrelationIdProvider
    {
        public string GenerateCorrelationId(HttpContext context)
        {
            return null;
        }
    }
}