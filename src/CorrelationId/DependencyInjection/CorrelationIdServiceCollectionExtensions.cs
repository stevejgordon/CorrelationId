using System;
using CorrelationId.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CorrelationId.DependencyInjection
{
    /// <summary>
    /// Extensions on the <see cref="IServiceCollection"/> to register the correlation ID services.
    /// </summary>
    public static class CorrelationIdServiceCollectionExtensions
    {
        /// <summary>
        /// Adds required services to support the Correlation ID functionality to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <remarks>
        /// This operation is idempotent - multiple invocations will still only result in a single
        /// instance of the required services in the <see cref="IServiceCollection"/>. It can be invoked
        /// multiple times in order to get access to the <see cref="ICorrelationIdBuilder"/> in multiple places.
        /// </remarks>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the correlation ID services to.</param>
        /// <returns>An instance of <see cref="ICorrelationIdBuilder"/> which to be used to configure correlation ID providers and options.</returns>
        public static ICorrelationIdBuilder AddCorrelationId(this IServiceCollection services)
        {
            services.TryAddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
            services.TryAddTransient<ICorrelationContextFactory, CorrelationContextFactory>();

            return new CorrelationIdBuilder(services);
        }

        /// <summary>
        /// Adds required services to support the Correlation ID functionality to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <remarks>
        /// This operation is idempotent - multiple invocations will still only result in a single
        /// instance of the required services in the <see cref="IServiceCollection"/>. It can be invoked
        /// multiple times in order to get access to the <see cref="ICorrelationIdBuilder"/> in multiple places.
        /// </remarks>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the correlation ID services to.</param>
        /// <param name="configure">The <see cref="Action{CorrelationIdOptions}"/> to configure the provided <see cref="CorrelationIdOptions"/>.</param>
        /// <returns>An instance of <see cref="ICorrelationIdBuilder"/> which to be used to configure correlation ID providers and options.</returns>
        public static ICorrelationIdBuilder AddCorrelationId(this IServiceCollection services, Action<CorrelationIdOptions> configure)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }
            
            services.Configure(configure);

            return services.AddCorrelationId();
        }

        /// <summary>
        /// Adds required services to support the Correlation ID functionality to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <remarks>
        /// This operation is may only be called once to avoid exceptions from attempting to add the default <see cref="ICorrelationIdProvider"/> multiple
        /// times.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this is called multiple times.</exception>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the correlation ID services to.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddDefaultCorrelationId(this IServiceCollection services)
        {
            services.AddCorrelationId().WithGuidProvider();

            return services;
        }

        /// <summary>
        /// Adds required services to support the Correlation ID functionality to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <remarks>
        /// This operation is may only be called once to avoid exceptions from attempting to add the default <see cref="ICorrelationIdProvider"/> multiple
        /// times.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if this is called multiple times.</exception>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the correlation ID services to.</param>
        /// <param name="configure">The <see cref="Action{CorrelationIdOptions}"/> to configure the provided <see cref="CorrelationIdOptions"/>.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddDefaultCorrelationId(this IServiceCollection services, Action<CorrelationIdOptions> configure)
        {
            services.AddDefaultCorrelationId();

            services.Configure(configure);

            return services;
        }
    }
}
