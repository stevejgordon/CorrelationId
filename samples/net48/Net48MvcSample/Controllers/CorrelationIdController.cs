using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;

namespace Net48MvcSample.Controllers
{
    public class CorrelationIdController : ApiController
    {
        public IHttpActionResult Get()
        {
            var correlationId = HttpContext.Current.Request.Headers["X-Correlation-Id"];

            if (correlationId == null)
            {
                return new StatusCodeResult(HttpStatusCode.NoContent, Request);
            }
            
            return Ok(correlationId);
        }
    }
}
