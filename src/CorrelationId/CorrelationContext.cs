using System;

namespace CorrelationId
{
    public class CorrelationContext
    {
        public CorrelationContext(string correlationId)
        {
            if (string.IsNullOrEmpty(correlationId))
                throw new ArgumentNullException(nameof(correlationId));

            CorrelationId = correlationId;
        }

        public string CorrelationId { get; }
    }
}
