using CorrelationId;
using CorrelationId.DependencyInjection;
using CorrelationId.HttpClient;
using MvcSample;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseNLog();

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddTransient<NoOpDelegatingHandler>();

builder.Services.AddHttpClient("MyClient")
    .AddCorrelationIdForwarding() // add the handler to attach the correlation ID to outgoing requests for this named client
    .AddHttpMessageHandler<NoOpDelegatingHandler>();

// Example of adding default correlation ID (using the GUID generator) services
// As shown here, options can be configured via the configure delegate overload
builder.Services.AddDefaultCorrelationId(options =>
{
    options.CorrelationIdGenerator = () => "Foo";
    options.AddToLoggingScope = true;
    options.EnforceHeader = false; //set true to enforce the correlation ID
    options.IgnoreRequestHeader = false;
    options.IncludeInResponse = true;
    options.RequestHeader = "My-Custom-Correlation-Id";
    options.ResponseHeader = "X-Correlation-Id";
    options.UpdateTraceIdentifier = false;
    options.LogLevelOptions = new CorrelationIdLogLevelOptions
    {
        //set log level severity
        FoundCorrelationIdHeader = LogLevel.Debug,  
        MissingCorrelationIdHeader = LogLevel.Debug
    };
});

// Example of registering a custom correlation ID provider
//builder.Services.AddCorrelationId().WithCustomProvider<DoNothingCorrelationIdProvider>();

//Setup NLog
LogManager.Setup().LoadConfigurationFromSection(builder.Configuration);

var app = builder.Build();

app.UseCorrelationId(); // adds the correlation ID middleware

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();