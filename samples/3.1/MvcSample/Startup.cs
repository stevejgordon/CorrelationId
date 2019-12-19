using CorrelationId;
using CorrelationId.DependencyInjection;
using CorrelationId.HttpClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MvcSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<NoOpDelegatingHandler>();

            services.AddHttpClient("MyClient")
                .AddCorrelationIdForwarding() // add the handler to attach the correlation ID to outgoing requests for this named client
                .AddHttpMessageHandler<NoOpDelegatingHandler>();
                       
            // Example of adding default correlation ID (using the GUID generator) services
            // As shown here, options can be configured via the configure degelate overload
            services.AddDefaultCorrelationId(options =>
            { 
                options.CorrelationIdGenerator = () => "Foo";
                options.AddToLoggingScope = true;
                options.EnforceHeader = true;
                options.IgnoreRequestHeader = false;
                options.IncludeInResponse = true;
                options.RequestHeader = "My-Custom-Correlation-Id";
                options.ResponseHeader = "X-Correlation-Id";
                options.UpdateTraceIdentifier = false;
            });

            // Example of registering a custom correlation ID provider
            //services.AddCorrelationId().WithCustomProvider<DoNothingCorrelationIdProvider>();
            
            services.AddControllers();
            }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseCorrelationId(); // adds the correlation ID middleware

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
