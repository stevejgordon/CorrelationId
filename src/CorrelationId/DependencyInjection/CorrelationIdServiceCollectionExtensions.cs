using System;
using System.Linq;
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
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

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
        /// <typeparam name="T">The <see cref="ICorrelationIdProvider"/> implementation type.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the correlation ID services to.</param>
        /// <returns>An instance of <see cref="ICorrelationIdBuilder"/> which to be used to configure correlation ID providers and options.</returns>
        public static ICorrelationIdBuilder AddCorrelationId<T>(this IServiceCollection services) where T : class, ICorrelationIdProvider
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (services.Any(x => x.ServiceType == typeof(ICorrelationIdProvider)))
            {
                throw new InvalidOperationException("A provider has already been added.");
            }

            var builder = AddCorrelationId(services);

            builder.Services.TryAddSingleton<ICorrelationIdProvider, T>();

            return builder;
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
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configure is null)
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
        /// This operation is idempotent - multiple invocations will still only result in a single
        /// instance of the required services in the <see cref="IServiceCollection"/>. It can be invoked
        /// multiple times in order to get access to the <see cref="ICorrelationIdBuilder"/> in multiple places.
        /// </remarks>
        /// <typeparam name="T">The <see cref="ICorrelationIdProvider"/> implementation type.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the correlation ID services to.</param>
        /// <param name="configure">The <see cref="Action{CorrelationIdOptions}"/> to configure the provided <see cref="CorrelationIdOptions"/>.</param>
        /// <returns>An instance of <see cref="ICorrelationIdBuilder"/> which to be used to configure correlation ID providers and options.</returns>
        public static ICorrelationIdBuilder AddCorrelationId<T>(this IServiceCollection services, Action<CorrelationIdOptions> configure) where T : class, ICorrelationIdProvider
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (services.Any(x => x.ServiceType == typeof(ICorrelationIdProvider)))
            {
                throw new InvalidOperationException("A provider has already been added.");
            }

            if (configure is null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            services.Configure(configure);
            
            return services.AddCorrelationId<T>();
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
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

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
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configure is null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            services.AddDefaultCorrelationId();

            services.Configure(configure);

            return services;
        }
    }
}
