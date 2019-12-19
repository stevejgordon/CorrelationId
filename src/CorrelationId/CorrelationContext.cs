using System;

namespace CorrelationId
{
    /// <summary>
    /// Provides access to per request correlation properties.
    /// </summary>
    public class CorrelationContext
    {
        /// <summary>
        /// Create a <see cref="CorrelationContext"/> instance.
        /// </summary>
        /// <param name="correlationId">The correlation ID on the context.</param>
        /// <param name="header">The name of the header from which the Correlation ID was read/written.</param>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="correlationId"/> or <paramref name="header"/> are null or empty.</exception>
        public CorrelationContext(string correlationId, string header)
        {
            if (string.IsNullOrEmpty(correlationId))
                throw new ArgumentException("A correlation ID must be provided.", nameof(correlationId));

            if (string.IsNullOrEmpty(header))
                throw new ArgumentException("A header must be provided.", nameof(header));

            CorrelationId = correlationId;
            Header = header;
        }

        /// <summary>
        /// The Correlation ID which is applicable to the current request.
        /// </summary>
        public string CorrelationId { get; }

        /// <summary>
        /// The name of the header from which the Correlation ID was read/written.
        /// </summary>
        public string Header { get; }
    }
}
