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
                if (dbSetType.GetTypeInfo().IsGenericType
                    && dbSetType.GetGenericTypeDefinition() == typeof(DbSet<>))
                {
                    var entityType = dbSetType.GetGenericArguments()[0];
                    var (isJsonApiResource, idType) = builder.GetIdType(entityType);
                    if (isJsonApiResource)
                        builder._entities.Add(builder.GetEntity(builder.GetResourceNameFromDbSetProperty(property, entityType), entityType, idType));
                }
            }
            return resourceGraphBuilder;
        }
    }

    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Enabling JsonApiDotNetCore using the EF Core DbContext to build the ResourceGraph.
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <param name="resources"></param>
        /// <returns></returns>
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
