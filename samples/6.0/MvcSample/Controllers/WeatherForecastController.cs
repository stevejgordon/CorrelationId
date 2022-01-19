using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace MvcSample.Controllers;

[ApiController]
[Route("weather-forecast")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] _summaries =
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ICorrelationContextAccessor _contextAccessor;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger,
        IHttpClientFactory httpClientFactory, ICorrelationContextAccessor contextAccessor)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _contextAccessor = contextAccessor;
    }

    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
    {
        _logger.LogInformation("CorrelationId: {CorrelationId}", _contextAccessor.CorrelationContext.CorrelationId);

        var client =
            _httpClientFactory.CreateClient("MyClient"); // this client will attach the correlation ID header

        client.GetAsync("https://www.example.com");

        var rng = new Random();
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = _summaries[rng.Next(_summaries.Length)]
            })
            .ToArray();
    }
}