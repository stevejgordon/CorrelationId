using System;

namespace CorrelationId
{
    /// <inheritdoc />
    public class CorrelationContext : ICorrelationContext
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

        /// <inheritdoc />
        public string CorrelationId { get; }

        /// <inheritdoc />
        public string Header { get; }
    }
}
