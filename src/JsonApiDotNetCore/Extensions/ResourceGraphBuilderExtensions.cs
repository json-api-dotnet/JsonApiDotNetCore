using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Extensions.EntityFrameworkCore
{
    /// <summary>
    /// Extensions for configuring JsonApiDotNetCore with EF Core
    /// </summary>
    internal static class ResourceGraphBuilderExtensions
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
}
