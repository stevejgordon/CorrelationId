using System.Web.Http;

namespace Net48MvcSample
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            GlobalConfiguration.Configure(CorrelationIdFakeDependencyInjection.Register);
        }
    }
}