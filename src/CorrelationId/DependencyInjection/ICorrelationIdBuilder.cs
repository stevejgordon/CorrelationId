using Microsoft.Extensions.DependencyInjection;

namespace CorrelationId.DependencyInjection
{
    /// <summary>
    /// A builder used to configure the correlation ID services.
    /// </summary>
    public interface ICorrelationIdBuilder
    {
        /// <summary>
        /// Gets the <see cref="IServiceCollection"/> into which the correlation ID services will be registered.
        /// </summary>
        IServiceCollection Services { get; }
    }
}
