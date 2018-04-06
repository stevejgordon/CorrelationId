namespace CorrelationId
{
    /// <summary>
    /// A factory for creating and disposing an instance of a <see cref="ICorrelationContext"/>.
    /// </summary>
    public interface ICorrelationContextFactory
    {
        /// <summary>
        /// Creates a new <see cref="ICorrelationContext"/> with the correlation ID set for the current request.
        /// </summary>
        /// <param name="correlationId">The correlation ID to set on the context.</param>
        /// /// <param name="header">The header used to hold the correlation ID.</param>
        /// <returns>A new instance of a <see cref="ICorrelationContext"/>.</returns>
        ICorrelationContext Create(string correlationId, string header);

        /// <summary>
        /// Disposes of the <see cref="ICorrelationContext"/> for the current request.
        /// </summary>
        void Dispose();
    }
}
