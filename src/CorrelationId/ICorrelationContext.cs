namespace CorrelationId
{
    /// <summary>
    /// Provides access to per request correlation properties.
    /// </summary>
    public interface ICorrelationContext
    {
        /// <summary>
        /// The Correlation ID which is applicable to the current request.
        /// </summary>
        string CorrelationId { get; }

        /// <summary>
        /// The name of the header from which the Correlation ID is read/written.
        /// </summary>
        string Header { get; }
    }
}