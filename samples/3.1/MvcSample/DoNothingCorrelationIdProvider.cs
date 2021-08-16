using System.Threading.Tasks;
using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Http;

namespace MvcSample
{
    public class DoNothingCorrelationIdProvider : ICorrelationIdProvider
    {
        public Task<string> GenerateCorrelationId(HttpContext _) => null;
    }
}