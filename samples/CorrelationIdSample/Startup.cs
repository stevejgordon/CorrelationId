using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using CorrelationId;

namespace CorrelationIdSample
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app, ILoggerFactory factory, IHostingEnvironment host)
        {
            // Displays all log levels
            factory.AddConsole(LogLevel.Debug);

            app.UseCorrelationId();
        }

        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseKestrel()
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
