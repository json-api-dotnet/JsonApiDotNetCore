using System;
using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Graph;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Data;

namespace JsonApiDotNetCore.Extensions.EntityFrameworkCore
{

    /// <summary>
    /// Extensions for configuring JsonApiDotNetCore with EF Core
    /// </summary>
    public static class IResourceGraphBuilderExtensions
    {
        /// <summary>
        /// Add all the models that are part of the provided <see cref="DbContext" /> 
        /// that also implement <see cref="IIdentifiable"/>
        /// </summary>
        /// <typeparam name="TDbContext">The <see cref="DbContext"/> implementation type.</typeparam>
        public static IResourceGraphBuilder AddDbContext<TDbContext>(this IResourceGraphBuilder resourceGraphBuilder) where TDbContext : DbContext
        {
            var builder = (ResourceGraphBuilder)resourceGraphBuilder;
            var contextType = typeof(TDbContext);
            var contextProperties = contextType.GetProperties();
            foreach (var property in contextProperties)
            {
                var dbSetType = property.PropertyType;
                if (dbSetType.IsGenericType
                    && dbSetType.GetGenericTypeDefinition() == typeof(DbSet<>))
                {
                    var resourceType = dbSetType.GetGenericArguments()[0];
                    builder.AddResource(resourceType, pluralizedTypeName: GetResourceNameFromDbSetProperty(property, resourceType));
                }
            }
            return resourceGraphBuilder;
        }

        private static string GetResourceNameFromDbSetProperty(PropertyInfo property, Type resourceType)
        {
            // this check is actually duplicated in the ResourceNameFormatter
            // however, we perform it here so that we allow class attributes to be prioritized over
            // the DbSet attribute. Eventually, the DbSet attribute should be deprecated.
            //
            // check the class definition first
            // [Resource("models"] public class Model : Identifiable { /* ... */ }
            if (resourceType.GetCustomAttribute(typeof(ResourceAttribute)) is ResourceAttribute classResourceAttribute)
                return classResourceAttribute.ResourceName;

            // check the DbContext member next
            // [Resource("models")] public DbSet<Model> Models { get; set; }
            if (property.GetCustomAttribute(typeof(ResourceAttribute)) is ResourceAttribute resourceAttribute)
                return resourceAttribute.ResourceName;

            return null;
        }
    }

    /// <summary>
    /// Extensions for configuring JsonApiDotNetCore with EF Core
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Enabling JsonApiDotNetCore using the EF Core DbContext to build the ResourceGraph.
        /// </summary>
        public static IServiceCollection AddJsonApi<TDbContext>(this IServiceCollection services,
                                                    Action<JsonApiOptions> options = null,
                                                    Action<IServiceDiscoveryFacade> discovery = null,
                                                    Action<IResourceGraphBuilder> resources = null,
                                                    IMvcCoreBuilder mvcBuilder = null)
            where TDbContext : DbContext
        {
            var application = new JsonApiApplicationBuilder(services, mvcBuilder ?? services.AddMvcCore());
            if (options != null)
                application.ConfigureJsonApiOptions(options);
            application.ConfigureMvc();
            if (discovery != null)
                application.AutoDiscover(discovery);
            application.ConfigureResources<TDbContext>(resources);
            application.ConfigureServices();
            return services;
        }
    }

    /// <summary>
    /// Extensions for configuring JsonApiDotNetCore with EF Core
    /// </summary>
    public static class JsonApiApplicationBuildExtensions
    {
        /// <summary>
        /// Executes the action provided by the user to configure the resources using <see cref="IResourceGraphBuilder"/>.
        /// Additionally, inspects the EF core database context for models that implement IIdentifiable.
        /// </summary>
        public static void ConfigureResources<TContext>(this JsonApiApplicationBuilder builder, Action<IResourceGraphBuilder> resourceGraphBuilder) where TContext : DbContext
        {
            builder._resourceGraphBuilder.AddDbContext<TContext>();
            builder._usesDbContext = true;
            builder._services.AddScoped<IDbContextResolver, DbContextResolver<TContext>>();
            resourceGraphBuilder?.Invoke(builder._resourceGraphBuilder);
        }
    }
}
