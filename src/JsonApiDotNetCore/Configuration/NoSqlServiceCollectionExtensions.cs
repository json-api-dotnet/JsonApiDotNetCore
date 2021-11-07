using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// Provides extension methods for the registration of services and other injectables
    /// with the service container.
    /// </summary>
    [PublicAPI]
    public static class NoSqlServiceCollectionExtensions
    {
        /// <summary>
        /// For each resource annotated with the <see cref="NoSqlResourceAttribute" />, adds a
        /// scoped service with a service type of <see cref="IResourceService{TResource, TId}" />
        /// and an implementation type of <see cref="NoSqlResourceService{TResource,TId}" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" />.</param>
        /// <returns>The <see cref="IServiceCollection" />.</returns>
        public static IServiceCollection AddNoSqlResourceServices(this IServiceCollection services)
        {
            return services.AddNoSqlResourceServices(Assembly.GetCallingAssembly());
        }

        /// <summary>
        /// For each resource annotated with the <see cref="NoSqlResourceAttribute" />, adds a
        /// scoped service with a service type of <see cref="IResourceService{TResource, TId}" />
        /// and an implementation type of <see cref="NoSqlResourceService{TResource,TId}" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" />.</param>
        /// <param name="assembly">The <see cref="Assembly" /> containing the annotated resources.</param>
        /// <returns>The <see cref="IServiceCollection" />.</returns>
        public static IServiceCollection AddNoSqlResourceServices(this IServiceCollection services, Assembly assembly)
        {
            services.AddScoped<INoSqlQueryLayerComposer, NoSqlQueryLayerComposer>();

            foreach (Type resourceType in assembly.ExportedTypes.Where(IsNoSqlResource))
            {
                if (TryGetIdType(resourceType, out Type? idType))
                {
                    services.AddScoped(typeof(IResourceService<,>).MakeGenericType(resourceType, idType),
                        typeof(NoSqlResourceService<,>).MakeGenericType(resourceType, idType));
                }
            }

            return services;
        }

        private static bool IsNoSqlResource(Type type)
        {
            return Attribute.GetCustomAttribute(type, typeof(NoSqlResourceAttribute)) is not null && type.GetInterfaces().Any(IsGenericIIdentifiable);
        }

        private static bool IsGenericIIdentifiable(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IIdentifiable<>);
        }

        private static bool TryGetIdType(Type resourceType, [NotNullWhen(true)] out Type? idType)
        {
            Type? identifiableInterface = resourceType.GetInterfaces().FirstOrDefault(IsGenericIIdentifiable);
            idType = identifiableInterface?.GetGenericArguments()[0];
            return idType is not null;
        }
    }
}
