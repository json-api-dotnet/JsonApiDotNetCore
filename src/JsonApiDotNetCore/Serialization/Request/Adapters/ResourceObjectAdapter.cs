#nullable disable

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Request.Adapters
{
    /// <inheritdoc cref="IResourceObjectAdapter" />
    public sealed class ResourceObjectAdapter : ResourceIdentityAdapter, IResourceObjectAdapter
    {
        private readonly IJsonApiOptions _options;
        private readonly IRelationshipDataAdapter _relationshipDataAdapter;

        public ResourceObjectAdapter(IResourceGraph resourceGraph, IResourceFactory resourceFactory, IJsonApiOptions options,
            IRelationshipDataAdapter relationshipDataAdapter)
            : base(resourceGraph, resourceFactory)
        {
            ArgumentGuard.NotNull(options, nameof(options));
            ArgumentGuard.NotNull(relationshipDataAdapter, nameof(relationshipDataAdapter));

            _options = options;
            _relationshipDataAdapter = relationshipDataAdapter;
        }

        /// <inheritdoc />
        public (IIdentifiable resource, ResourceType resourceType) Convert(ResourceObject resourceObject, ResourceIdentityRequirements requirements,
            RequestAdapterState state)
        {
            ArgumentGuard.NotNull(resourceObject, nameof(resourceObject));
            ArgumentGuard.NotNull(requirements, nameof(requirements));
            ArgumentGuard.NotNull(state, nameof(state));

            (IIdentifiable resource, ResourceType resourceType) = ConvertResourceIdentity(resourceObject, requirements, state);

            ConvertAttributes(resourceObject.Attributes, resource, resourceType, state);
            ConvertRelationships(resourceObject.Relationships, resource, resourceType, state);

            return (resource, resourceType);
        }

        private void ConvertAttributes(IDictionary<string, object> resourceObjectAttributes, IIdentifiable resource, ResourceType resourceType,
            RequestAdapterState state)
        {
            using IDisposable _ = state.Position.PushElement("attributes");

            foreach ((string attributeName, object attributeValue) in resourceObjectAttributes.EmptyIfNull())
            {
                ConvertAttribute(resource, attributeName, attributeValue, resourceType, state);
            }
        }

        private void ConvertAttribute(IIdentifiable resource, string attributeName, object attributeValue, ResourceType resourceType, RequestAdapterState state)
        {
            using IDisposable _ = state.Position.PushElement(attributeName);
            AttrAttribute attr = resourceType.FindAttributeByPublicName(attributeName);

            if (attr == null && _options.AllowUnknownFieldsInRequestBody)
            {
                return;
            }

            AssertIsKnownAttribute(attr, attributeName, resourceType, state);
            AssertNoInvalidAttribute(attributeValue, state);
            AssertNoBlockedCreate(attr, resourceType, state);
            AssertNoBlockedChange(attr, resourceType, state);
            AssertNotReadOnly(attr, resourceType, state);

            attr!.SetValue(resource, attributeValue);
            state.WritableTargetedFields.Attributes.Add(attr);
        }

        [AssertionMethod]
        private static void AssertIsKnownAttribute(AttrAttribute attr, string attributeName, ResourceType resourceType, RequestAdapterState state)
        {
            if (attr == null)
            {
                throw new ModelConversionException(state.Position, "Unknown attribute found.",
                    $"Attribute '{attributeName}' does not exist on resource type '{resourceType.PublicName}'.");
            }
        }

        private static void AssertNoInvalidAttribute(object attributeValue, RequestAdapterState state)
        {
            if (attributeValue is JsonInvalidAttributeInfo info)
            {
                if (info == JsonInvalidAttributeInfo.Id)
                {
                    throw new ModelConversionException(state.Position, "Resource ID is read-only.", null);
                }

                string typeName = info.AttributeType.GetFriendlyTypeName();

                throw new ModelConversionException(state.Position, "Incompatible attribute value found.",
                    $"Failed to convert attribute '{info.AttributeName}' with value '{info.JsonValue}' of type '{info.JsonType}' to type '{typeName}'.");
            }
        }

        private static void AssertNoBlockedCreate(AttrAttribute attr, ResourceType resourceType, RequestAdapterState state)
        {
            if (state.Request.WriteOperation == WriteOperationKind.CreateResource && !attr.Capabilities.HasFlag(AttrCapabilities.AllowCreate))
            {
                throw new ModelConversionException(state.Position, "Attribute value cannot be assigned when creating resource.",
                    $"The attribute '{attr.PublicName}' on resource type '{resourceType.PublicName}' cannot be assigned to.");
            }
        }

        private static void AssertNoBlockedChange(AttrAttribute attr, ResourceType resourceType, RequestAdapterState state)
        {
            if (state.Request.WriteOperation == WriteOperationKind.UpdateResource && !attr.Capabilities.HasFlag(AttrCapabilities.AllowChange))
            {
                throw new ModelConversionException(state.Position, "Attribute value cannot be assigned when updating resource.",
                    $"The attribute '{attr.PublicName}' on resource type '{resourceType.PublicName}' cannot be assigned to.");
            }
        }

        private static void AssertNotReadOnly(AttrAttribute attr, ResourceType resourceType, RequestAdapterState state)
        {
            if (attr.Property.SetMethod == null)
            {
                throw new ModelConversionException(state.Position, "Attribute is read-only.",
                    $"Attribute '{attr.PublicName}' on resource type '{resourceType.PublicName}' is read-only.");
            }
        }

        private void ConvertRelationships(IDictionary<string, RelationshipObject> resourceObjectRelationships, IIdentifiable resource,
            ResourceType resourceType, RequestAdapterState state)
        {
            using IDisposable _ = state.Position.PushElement("relationships");

            foreach ((string relationshipName, RelationshipObject relationshipObject) in resourceObjectRelationships.EmptyIfNull())
            {
                ConvertRelationship(relationshipName, relationshipObject.Data, resource, resourceType, state);
            }
        }

        private void ConvertRelationship(string relationshipName, SingleOrManyData<ResourceIdentifierObject> relationshipData, IIdentifiable resource,
            ResourceType resourceType, RequestAdapterState state)
        {
            using IDisposable _ = state.Position.PushElement(relationshipName);
            RelationshipAttribute relationship = resourceType.FindRelationshipByPublicName(relationshipName);

            if (relationship == null && _options.AllowUnknownFieldsInRequestBody)
            {
                return;
            }

            AssertIsKnownRelationship(relationship, relationshipName, resourceType, state);

            object rightValue = _relationshipDataAdapter.Convert(relationshipData, relationship, true, state);

            relationship!.SetValue(resource, rightValue);
            state.WritableTargetedFields.Relationships.Add(relationship);
        }
    }
}
