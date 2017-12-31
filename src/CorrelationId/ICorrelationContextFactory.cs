namespace CorrelationId
{
    /// <summary>
    /// A factory for creating and disposing an instance of a <see cref="CorrelationContext"/>.
    /// </summary>
    public interface ICorrelationContextFactory
    {
        /// <summary>
        /// Creates a new <see cref="CorrelationContext"/> with the correlation ID set for the current request.
        /// </summary>
        /// <param name="correlationId">The correlation ID to set on the context.</param>
        /// <returns>A new instance of a <see cref="CorrelationContext"/>.</returns>
        CorrelationContext Create(string correlationId);

        /// <summary>
        /// Disposes of the <see cref="CorrelationContext"/> for the current request.
        /// </summary>
        void Dispose();
    }
}
