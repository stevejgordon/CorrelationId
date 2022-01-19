using CorrelationId.Abstractions;

namespace MvcSample;

public class DoNothingCorrelationIdProvider : ICorrelationIdProvider
{
    public string GenerateCorrelationId(HttpContext context)
    {
        return null;
    }
}