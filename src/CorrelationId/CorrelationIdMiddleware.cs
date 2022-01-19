using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace CorrelationId;

/// <summary>
///     Middleware which attempts to reads / creates a Correlation ID that can then be used in logs and
///     passed to upstream requests.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly ICorrelationIdProvider _correlationIdProvider;
    private readonly ILogger _logger;
    private readonly RequestDelegate _next;
    private readonly CorrelationIdOptions _options;

    /// <summary>
    ///     Creates a new instance of the CorrelationIdMiddleware.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The <see cref="ILogger" /> instance to log to.</param>
    /// <param name="options">The configuration options.</param>
    /// <param name="correlationIdProvider"></param>
    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger,
        IOptions<CorrelationIdOptions> options, ICorrelationIdProvider correlationIdProvider = null)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _correlationIdProvider = correlationIdProvider;
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    ///     Processes a request to synchronise TraceIdentifier and Correlation ID headers. Also creates a
    ///     <see cref="CorrelationContext" /> for the current request and disposes of it when the request is completing.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext" /> for the current request.</param>
    /// <param name="correlationContextFactory">
    ///     The <see cref="ICorrelationContextFactory" /> which can create a
    ///     <see cref="CorrelationContext" />.
    /// </param>
    public async Task Invoke(HttpContext httpContext, ICorrelationContextFactory correlationContextFactory)
    {
        Log.CorrelationIdProcessingBegin(_logger);

        if (_correlationIdProvider is null)
        {
            Log.MissingCorrelationIdProvider(_logger);

            throw new InvalidOperationException(
                "No 'ICorrelationIdProvider' has been registered. You must either add the correlation ID services" +
                " using the 'AddDefaultCorrelationId' extension method or you must register a suitable provider using the" +
                " 'ICorrelationIdBuilder'.");
        }

        var hasCorrelationIdHeader = httpContext.Request.Headers.TryGetValue(_options.RequestHeader, out var cid) &&
                                     !StringValues.IsNullOrEmpty(cid);

        if (!hasCorrelationIdHeader && _options.EnforceHeader)
        {
            Log.EnforcedCorrelationIdHeaderMissing(_logger);

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsync(
                $"The '{_options.RequestHeader}' request header is required, but was not found.");
            return;
        }

        var correlationId = hasCorrelationIdHeader ? cid.FirstOrDefault() : null;

        if (hasCorrelationIdHeader)
            Log.FoundCorrelationIdHeader(_logger, _options.LogLevelOptions.FoundCorrelationIdHeader, correlationId);
        else
            Log.MissingCorrelationIdHeader(_logger, _options.LogLevelOptions.MissingCorrelationIdHeader);

        if (_options.IgnoreRequestHeader || RequiresGenerationOfCorrelationId(hasCorrelationIdHeader, cid))
            correlationId = GenerateCorrelationId(httpContext);

        if (!string.IsNullOrEmpty(correlationId) && _options.UpdateTraceIdentifier)
        {
            Log.UpdatingTraceIdentifier(_logger);

            httpContext.TraceIdentifier = correlationId;
        }

        Log.CreatingCorrelationContext(_logger);
        correlationContextFactory.Create(correlationId, _options.RequestHeader);

        if (_options.IncludeInResponse && !string.IsNullOrEmpty(correlationId))
            // apply the correlation ID to the response header for client side tracking
            httpContext.Response.OnStarting(() =>
            {
                if (httpContext.Response.Headers.ContainsKey(_options.ResponseHeader))
                    return Task.CompletedTask;
                Log.WritingCorrelationIdResponseHeader(_logger, _options.ResponseHeader, correlationId);
                httpContext.Response.Headers.Add(_options.ResponseHeader, correlationId);

                return Task.CompletedTask;
            });

        if (_options.AddToLoggingScope && !string.IsNullOrEmpty(_options.LoggingScopeKey) &&
            !string.IsNullOrEmpty(correlationId))
        {
            using (_logger.BeginScope(new Dictionary<string, object>
                   {
                       [_options.LoggingScopeKey] = correlationId
                   }))
            {
                Log.CorrelationIdProcessingEnd(_logger, correlationId);
                await _next(httpContext);
            }
        }
        else
        {
            Log.CorrelationIdProcessingEnd(_logger, correlationId);
            await _next(httpContext);
        }

        Log.DisposingCorrelationContext(_logger);
        correlationContextFactory.Dispose();
    }

    private static bool RequiresGenerationOfCorrelationId(bool idInHeader, StringValues idFromHeader)
    {
        return !idInHeader || StringValues.IsNullOrEmpty(idFromHeader);
    }

    private string GenerateCorrelationId(HttpContext ctx)
    {
        string correlationId;

        if (_options.CorrelationIdGenerator != null)
        {
            correlationId = _options.CorrelationIdGenerator();
            Log.GeneratedHeaderUsingGeneratorFunction(_logger, correlationId);
            return correlationId;
        }

        correlationId = _correlationIdProvider.GenerateCorrelationId(ctx);
        Log.GeneratedHeaderUsingProvider(_logger, correlationId, _correlationIdProvider.GetType());
        return correlationId;
    }

    internal static class EventIds
    {
        public static readonly EventId CorrelationIdProcessingBegin = new(100, "CorrelationIdProcessingBegin");
        public static readonly EventId CorrelationIdProcessingEnd = new(101, "CorrelationIdProcessingEnd");

        public static readonly EventId MissingCorrelationIdProvider = new(103, "MissingCorrelationIdProvider");

        public static readonly EventId EnforcedCorrelationIdHeaderMissing =
            new(104, "EnforcedCorrelationIdHeaderMissing");

        public static readonly EventId FoundCorrelationIdHeader = new(105, "EnforcedCorrelationIdHeaderMissing");
        public static readonly EventId MissingCorrelationIdHeader = new(106, "MissingCorrelationIdHeader");

        public static readonly EventId GeneratedHeaderUsingGeneratorFunction =
            new(107, "GeneratedHeaderUsingGeneratorFunction");

        public static readonly EventId GeneratedHeaderUsingProvider = new(108, "GeneratedHeaderUsingProvider");

        public static readonly EventId UpdatingTraceIdentifier = new(109, "UpdatingTraceIdentifier");
        public static readonly EventId CreatingCorrelationContext = new(110, "CreatingCorrelationContext");
        public static readonly EventId DisposingCorrelationContext = new(111, "DisposingCorrelationContext");

        public static readonly EventId WritingCorrelationIdResponseHeader =
            new(112, "WritingCorrelationIdResponseHeader");
    }

    private static class Log
    {
        public static void CorrelationIdProcessingBegin(ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
                logger.Log(LogLevel.Debug, EventIds.CorrelationIdProcessingBegin, "Running correlation ID processing");
        }

        public static void CorrelationIdProcessingEnd(ILogger logger, string correlationId)
        {
            if (logger.IsEnabled(LogLevel.Debug))
                logger.Log(LogLevel.Debug, EventIds.CorrelationIdProcessingEnd,
                    "Correlation ID processing was completed with a final correlation ID {CorrelationId}", correlationId);
        }

        public static void MissingCorrelationIdProvider(ILogger logger)
        {
            logger.Log(LogLevel.Error, EventIds.MissingCorrelationIdProvider,
                "Correlation ID middleware was called when no ICorrelationIdProvider had been configured");
        }

        public static void EnforcedCorrelationIdHeaderMissing(ILogger logger)
        {
            logger.Log(LogLevel.Warning,
                EventIds.EnforcedCorrelationIdHeaderMissing,
                "Correlation ID header is enforced but no Correlation ID was not found in the request headers");
        }

        public static void FoundCorrelationIdHeader(ILogger logger, LogLevel logLevel, string correlationId)
        {
            logger.Log(logLevel, EventIds.FoundCorrelationIdHeader,
                "Correlation ID {CorrelationId} was found in the request headers", correlationId);
        }

        public static void MissingCorrelationIdHeader(ILogger logger, LogLevel logLevel)
        {
            logger.Log(logLevel, EventIds.MissingCorrelationIdHeader, "No correlation ID was found in the request headers");
        }

        public static void GeneratedHeaderUsingGeneratorFunction(ILogger logger, string correlationId)
        {
            logger.Log(LogLevel.Debug, EventIds.GeneratedHeaderUsingGeneratorFunction,
                "Generated a correlation ID {CorrelationId} using the configured generator function", correlationId);
        }

        public static void GeneratedHeaderUsingProvider(ILogger logger, string correlationId, Type type)
        {
            logger.Log(LogLevel.Debug, EventIds.GeneratedHeaderUsingProvider,
                "Generated a correlation ID {CorrelationId} using the {Type} provider", correlationId, type);
        }

        public static void UpdatingTraceIdentifier(ILogger logger)
        {
            logger.Log(LogLevel.Debug, EventIds.UpdatingTraceIdentifier,
                "Updating the TraceIdentifier value on the HttpContext");
        }

        public static void CreatingCorrelationContext(ILogger logger)
        {
            logger.Log(LogLevel.Debug, EventIds.CreatingCorrelationContext,
                "Creating the correlation context for this request");
        }

        public static void DisposingCorrelationContext(ILogger logger)
        {
            logger.Log(LogLevel.Debug, EventIds.DisposingCorrelationContext,
                "Disposing the correlation context for this request");
        }

        public static void WritingCorrelationIdResponseHeader(ILogger logger, string headerName, string correlationId)
        {
            logger.Log(LogLevel.Debug, EventIds.WritingCorrelationIdResponseHeader,
                "Writing correlation ID response header {ResponseHeader} with value {CorrelationId}", headerName,
                correlationId);
        }
    }
}