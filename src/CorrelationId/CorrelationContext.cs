using System;

namespace CorrelationId
{
    /// <summary>
    /// Provides access to per request correlation properties.
    /// </summary>
    public class CorrelationContext
    {
        internal CorrelationContext(string correlationId)
        {
            if (string.IsNullOrEmpty(correlationId))
                throw new ArgumentNullException(nameof(correlationId));

            CorrelationId = correlationId;
        }

        /// <summary>
        /// The Correlation ID which is applicable to the current request.
        /// </summary>
        public string CorrelationId { get; }
    }
}
