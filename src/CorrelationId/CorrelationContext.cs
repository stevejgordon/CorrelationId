using System;

namespace CorrelationId
{
    /// <summary>
    /// Provides access to per request correlation properties.
    /// </summary>
    public class CorrelationContext
    {
        internal CorrelationContext(string correlationId, string header)
        {
            if (string.IsNullOrEmpty(correlationId))
                throw new ArgumentNullException(nameof(correlationId));

            if (string.IsNullOrEmpty(header))
                throw new ArgumentNullException(nameof(header));

            CorrelationId = correlationId;
            Header = header;
        }

        /// <summary>
        /// The Correlation ID which is applicable to the current request.
        /// </summary>
        public string CorrelationId { get; set;}

        /// <summary>
        /// The name of the header from which the Correlation ID is read/written.
        /// </summary>
        public string Header { get; set;}
    }
}
