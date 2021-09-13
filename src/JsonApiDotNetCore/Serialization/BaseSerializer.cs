using System;
using System.Collections.Generic;
using System.Text.Json;
using JsonApiDotNetCore.Diagnostics;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Building;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// Abstract base class for serialization. Uses <see cref="IResourceObjectBuilder" /> to convert resources into <see cref="ResourceObject" />s and wraps
    /// them in a <see cref="Document" />.
    /// </summary>
    public abstract class BaseSerializer
    {
        protected IResourceObjectBuilder ResourceObjectBuilder { get; }

        protected BaseSerializer(IResourceObjectBuilder resourceObjectBuilder)
        {
            ArgumentGuard.NotNull(resourceObjectBuilder, nameof(resourceObjectBuilder));

            ResourceObjectBuilder = resourceObjectBuilder;
        }

        /// <summary>
        /// Builds a <see cref="Document" /> for <paramref name="resource" />. Adds the attributes and relationships that are enlisted in
        /// <paramref name="attributes" /> and <paramref name="relationships" />.
        /// </summary>
        /// <param name="resource">
        /// Resource to build a <see cref="ResourceObject" /> for.
        /// </param>
        /// <param name="attributes">
        /// Attributes to include in the building process.
        /// </param>
        /// <param name="relationships">
        /// Relationships to include in the building process.
        /// </param>
        /// <returns>
        /// The resource object that was built.
        /// </returns>
        protected Document Build(IIdentifiable resource, IReadOnlyCollection<AttrAttribute> attributes,
            IReadOnlyCollection<RelationshipAttribute> relationships)
        {
            using IDisposable _ = CodeTimingSessionManager.Current.Measure("Serializer.Build (single)");

            ResourceObject resourceObject = resource != null ? ResourceObjectBuilder.Build(resource, attributes, relationships) : null;

            return new Document
            {
                Data = new SingleOrManyData<ResourceObject>(resourceObject)
            };
        }

        /// <summary>
        /// Builds a <see cref="Document" /> for <paramref name="resources" />. Adds the attributes and relationships that are enlisted in
        /// <paramref name="attributes" /> and <paramref name="relationships" />.
        /// </summary>
        /// <param name="resources">
        /// Resource to build a <see cref="ResourceObject" /> for.
        /// </param>
        /// <param name="attributes">
        /// Attributes to include in the building process.
        /// </param>
        /// <param name="relationships">
        /// Relationships to include in the building process.
        /// </param>
        /// <returns>
        /// The resource object that was built.
        /// </returns>
        protected Document Build(IReadOnlyCollection<IIdentifiable> resources, IReadOnlyCollection<AttrAttribute> attributes,
            IReadOnlyCollection<RelationshipAttribute> relationships)
        {
            ArgumentGuard.NotNull(resources, nameof(resources));

            using IDisposable _ = CodeTimingSessionManager.Current.Measure("Serializer.Build (list)");

            var resourceObjects = new List<ResourceObject>();

            foreach (IIdentifiable resource in resources)
            {
                resourceObjects.Add(ResourceObjectBuilder.Build(resource, attributes, relationships));
            }

            return new Document
            {
                Data = new SingleOrManyData<ResourceObject>(resourceObjects)
            };
        }

        protected string SerializeObject(object value, JsonSerializerOptions serializerOptions)
        {
            ArgumentGuard.NotNull(serializerOptions, nameof(serializerOptions));

            using IDisposable _ =
                CodeTimingSessionManager.Current.Measure("JsonSerializer.Serialize", MeasurementSettings.ExcludeJsonSerializationInPercentages);

            return JsonSerializer.Serialize(value, serializerOptions);
        }
    }
}
