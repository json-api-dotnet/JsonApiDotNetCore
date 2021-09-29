using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.RequestAdapters
{
    /// <inheritdoc />
    public sealed class RelationshipDataAdapter : IRelationshipDataAdapter
    {
        private static readonly CollectionConverter CollectionConverter = new();

        private readonly IResourceGraph _resourceGraph;
        private readonly IResourceIdentifierObjectAdapter _resourceIdentifierObjectAdapter;

        public RelationshipDataAdapter(IResourceGraph resourceGraph, IResourceIdentifierObjectAdapter resourceIdentifierObjectAdapter)
        {
            ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));
            ArgumentGuard.NotNull(resourceIdentifierObjectAdapter, nameof(resourceIdentifierObjectAdapter));

            _resourceGraph = resourceGraph;
            _resourceIdentifierObjectAdapter = resourceIdentifierObjectAdapter;
        }

        /// <inheritdoc />
        public object Convert(SingleOrManyData<ResourceObject> data, RelationshipAttribute relationship, bool useToManyElementType, RequestAdapterState state)
        {
            SingleOrManyData<ResourceIdentifierObject> identifierData = ToIdentifierData(data);
            return Convert(identifierData, relationship, useToManyElementType, state);
        }

        private static SingleOrManyData<ResourceIdentifierObject> ToIdentifierData(SingleOrManyData<ResourceObject> data)
        {
            if (!data.IsAssigned)
            {
                return default;
            }

            object newValue = null;

            if (data.ManyValue != null)
            {
                newValue = data.ManyValue.Select(resourceObject => new ResourceIdentifierObject
                {
                    Type = resourceObject.Type,
                    Id = resourceObject.Id,
                    Lid = resourceObject.Lid
                });
            }
            else if (data.SingleValue != null)
            {
                newValue = new ResourceIdentifierObject
                {
                    Type = data.SingleValue.Type,
                    Id = data.SingleValue.Id,
                    Lid = data.SingleValue.Lid
                };
            }

            return new SingleOrManyData<ResourceIdentifierObject>(newValue);
        }

        /// <inheritdoc />
        public object Convert(SingleOrManyData<ResourceIdentifierObject> data, RelationshipAttribute relationship, bool useToManyElementType,
            RequestAdapterState state)
        {
            ArgumentGuard.NotNull(relationship, nameof(relationship));
            ArgumentGuard.NotNull(state, nameof(state));

            using IDisposable _ = state.Position.PushElement("data");

            ResourceContext rightResourceContext = _resourceGraph.GetResourceContext(relationship.RightType);

            var requirements = new ResourceIdentityRequirements
            {
                ResourceContext = rightResourceContext,
                IdConstraint = JsonElementConstraint.Required,
                RelationshipName = relationship.PublicName
            };

            return relationship is HasOneAttribute
                ? ConvertToOneRelationshipData(data, relationship, requirements, state)
                : ConvertToManyRelationshipData(data, relationship, requirements, useToManyElementType, state);
        }

        private IIdentifiable ConvertToOneRelationshipData(SingleOrManyData<ResourceIdentifierObject> data, RelationshipAttribute relationship,
            ResourceIdentityRequirements requirements, RequestAdapterState state)
        {
            AssertHasNoManyValue(data, relationship, state);

            return data.SingleValue != null ? _resourceIdentifierObjectAdapter.Convert(data.SingleValue, requirements, state) : null;
        }

        private static void AssertHasNoManyValue(SingleOrManyData<ResourceIdentifierObject> data, RelationshipAttribute relationship, RequestAdapterState state)
        {
            if (data.ManyValue != null)
            {
                throw new DeserializationException(state.Position, "Expected single data element for to-one relationship.",
                    $"Expected single data element for '{relationship.PublicName}' relationship.");
            }
        }

        private IEnumerable ConvertToManyRelationshipData(SingleOrManyData<ResourceIdentifierObject> data, RelationshipAttribute relationship,
            ResourceIdentityRequirements requirements, bool useToManyElementType, RequestAdapterState state)
        {
            AssertHasManyValue(data, relationship, state);

            int arrayIndex = 0;
            var rightResources = new List<IIdentifiable>();

            foreach (ResourceIdentifierObject resourceIdentifierObject in data.ManyValue)
            {
                using IDisposable _ = state.Position.PushArrayIndex(arrayIndex);

                IIdentifiable rightResource = _resourceIdentifierObjectAdapter.Convert(resourceIdentifierObject, requirements, state);
                rightResources.Add(rightResource);

                arrayIndex++;
            }

            if (useToManyElementType)
            {
                return CollectionConverter.CopyToTypedCollection(rightResources, relationship.Property.PropertyType);
            }

            var resourceSet = new HashSet<IIdentifiable>(IdentifiableComparer.Instance);
            resourceSet.AddRange(rightResources);
            return resourceSet;
        }

        private static void AssertHasManyValue(SingleOrManyData<ResourceIdentifierObject> data, RelationshipAttribute relationship, RequestAdapterState state)
        {
            if (data.ManyValue == null)
            {
                throw new DeserializationException(state.Position, "Expected data[] element for to-many relationship.",
                    $"Expected data[] element for '{relationship.PublicName}' relationship.");
            }
        }
    }
}
