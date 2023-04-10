using System.Web;
using System.Web.Http;
using CorrelationId;

namespace Net48MvcSample.Controllers
{
    public class CorrelationIdController : ApiController
    {
        public string Get()
        {
            var correlationId = HttpContext.Current.Request.Headers["X-Correlation-Id"];
            return correlationId;
        }
    }
}
