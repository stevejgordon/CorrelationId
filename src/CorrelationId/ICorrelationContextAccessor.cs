namespace CorrelationId
{
    /// <summary>
    /// Provides access to the <see cref="ICorrelationContext"/> for the current request.
    /// </summary>
    public interface ICorrelationContextAccessor
    {
        /// <summary>
        /// The <see cref="ICorrelationContext"/> for the current request.
        /// </summary>
        ICorrelationContext CorrelationContext { get; set; }
    }
}
