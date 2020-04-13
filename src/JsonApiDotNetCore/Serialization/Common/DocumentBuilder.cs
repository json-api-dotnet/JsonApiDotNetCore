using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using JsonApiDotNetCore.Models;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization
{
    /// <summary>
    /// Abstract base class for serialization.
    /// Uses <see cref="IResourceObjectBuilder"/> to convert entities in to <see cref="ResourceObject"/>s and wraps them in a <see cref="Document"/>.
    /// </summary>
    public abstract class BaseDocumentBuilder
    {
        protected readonly IResourceObjectBuilder _resourceObjectBuilder;

        protected BaseDocumentBuilder(IResourceObjectBuilder resourceObjectBuilder)
        {
            _resourceObjectBuilder = resourceObjectBuilder;
        }

        /// <summary>
        /// Builds a <see cref="Document"/> for <paramref name="entity"/>.
        /// Adds the attributes and relationships that are enlisted in <paramref name="attributes"/> and <paramref name="relationships"/>
        /// </summary>
        /// <param name="entity">Entity to build a Resource Object for</param>
        /// <param name="attributes">Attributes to include in the building process</param>
        /// <param name="relationships">Relationships to include in the building process</param>
        /// <returns>The resource object that was built</returns>
        protected Document Build(IIdentifiable entity, IReadOnlyCollection<AttrAttribute> attributes, IReadOnlyCollection<RelationshipAttribute> relationships)
        {
            if (entity == null)
                return new Document();

            return new Document { Data = _resourceObjectBuilder.Build(entity, attributes, relationships) };
        }

        /// <summary>
        /// Builds a <see cref="Document"/> for <paramref name="entities"/>.
        /// Adds the attributes and relationships that are enlisted in <paramref name="attributes"/> and <paramref name="relationships"/>
        /// </summary>
        /// <param name="entities">Entity to build a Resource Object for</param>
        /// <param name="attributes">Attributes to include in the building process</param>
        /// <param name="relationships">Relationships to include in the building process</param>
        /// <returns>The resource object that was built</returns>
        protected Document Build(IEnumerable entities, IReadOnlyCollection<AttrAttribute> attributes, IReadOnlyCollection<RelationshipAttribute> relationships)
        {
            var data = new List<ResourceObject>();
            foreach (IIdentifiable entity in entities)
                data.Add(_resourceObjectBuilder.Build(entity, attributes, relationships));

            return new Document { Data = data };
        }

        protected string SerializeObject(object value, JsonSerializerSettings defaultSettings, Action<JsonSerializer> changeSerializer = null)
        {
            JsonSerializer serializer = JsonSerializer.CreateDefault(defaultSettings);
            changeSerializer?.Invoke(serializer);

            using var stringWriter = new StringWriter();
            using (var jsonWriter = new JsonTextWriter(stringWriter))
            {
                serializer.Serialize(jsonWriter, value);
            }

            return stringWriter.ToString();
        }
    }
}
