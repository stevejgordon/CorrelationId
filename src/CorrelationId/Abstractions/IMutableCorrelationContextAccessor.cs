namespace CorrelationId.Abstractions
{
    internal interface IMutableCorrelationContextAccessor : ICorrelationContextAccessor
    {
        /// <summary>
        /// The settable <see cref="CorrelationContext"/> for the current request.
        /// </summary>
        new CorrelationContext CorrelationContext { set; }
    }
}