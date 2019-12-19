using System.Linq;
using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Http;

namespace MvcSample
{
    public class DoNothingCorrelationIdProvider : ICorrelationIdProvider
    {
        public string GenerateCorrelationId(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("X-Correlation-Id", out var sv))
            {
                return sv.FirstOrDefault();
            }

            return null;
        }
    }
}