using CorrelationId.Abstractions;
using Microsoft.Extensions.Logging;

namespace CorrelationId;

/// <summary>
///     Options for correlation IDs.
/// </summary>
public class CorrelationIdLogLevelOptions
{
    /// <summary>
    ///     <para>
    ///         Defines the log level severity when a correlation Id is found on the header.
    ///     </para>
    ///     <para>Default: LogLevel.Information</para>
    /// </summary>
    public LogLevel FoundCorrelationIdHeader { get; set; } = LogLevel.Information;

    /// <summary>
    ///     <para>
    ///         Defines the log level severity when a correlation Id is missing on the header.
    ///     </para>
    ///     <para>Default: LogLevel.Information</para>
    /// </summary>
    public LogLevel MissingCorrelationIdHeader { get; set; } = LogLevel.Information;
}