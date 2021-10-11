#nullable disable

using System;
using System.Net;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Request.Adapters
{
    /// <summary>
    /// Base class for validating and converting objects that represent an identity.
    /// </summary>
    public abstract class ResourceIdentityAdapter
    {
        private readonly IResourceGraph _resourceGraph;
        private readonly IResourceFactory _resourceFactory;

        protected ResourceIdentityAdapter(IResourceGraph resourceGraph, IResourceFactory resourceFactory)
        {
            ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));
            ArgumentGuard.NotNull(resourceFactory, nameof(resourceFactory));

            _resourceGraph = resourceGraph;
            _resourceFactory = resourceFactory;
        }

        protected (IIdentifiable resource, ResourceType resourceType) ConvertResourceIdentity(IResourceIdentity identity,
            ResourceIdentityRequirements requirements, RequestAdapterState state)
        {
            ArgumentGuard.NotNull(identity, nameof(identity));
            ArgumentGuard.NotNull(requirements, nameof(requirements));
            ArgumentGuard.NotNull(state, nameof(state));

            ResourceType resourceType = ResolveType(identity, requirements, state);
            IIdentifiable resource = CreateResource(identity, requirements, resourceType.ClrType, state);

            return (resource, resourceType);
        }

        private ResourceType ResolveType(IResourceIdentity identity, ResourceIdentityRequirements requirements, RequestAdapterState state)
        {
            AssertHasType(identity, state);

            using IDisposable _ = state.Position.PushElement("type");
            ResourceType resourceType = _resourceGraph.FindResourceType(identity.Type);

            AssertIsKnownResourceType(resourceType, identity.Type, state);
            AssertIsCompatibleResourceType(resourceType, requirements.ResourceType, requirements.RelationshipName, state);

            return resourceType;
        }

        private static void AssertHasType(IResourceIdentity identity, RequestAdapterState state)
        {
            if (identity.Type == null)
            {
                throw new ModelConversionException(state.Position, "The 'type' element is required.", null);
            }
        }

        private static void AssertIsKnownResourceType(ResourceType resourceType, string typeName, RequestAdapterState state)
        {
            if (resourceType == null)
            {
                throw new ModelConversionException(state.Position, "Unknown resource type found.", $"Resource type '{typeName}' does not exist.");
            }
        }

        private static void AssertIsCompatibleResourceType(ResourceType actual, ResourceType expected, string relationshipName, RequestAdapterState state)
        {
            if (expected != null && !expected.ClrType.IsAssignableFrom(actual.ClrType))
            {
                string message = relationshipName != null
                    ? $"Type '{actual.PublicName}' is incompatible with type '{expected.PublicName}' of relationship '{relationshipName}'."
                    : $"Type '{actual.PublicName}' is incompatible with type '{expected.PublicName}'.";

                throw new ModelConversionException(state.Position, "Incompatible resource type found.", message, HttpStatusCode.Conflict);
            }
        }

        private IIdentifiable CreateResource(IResourceIdentity identity, ResourceIdentityRequirements requirements, Type resourceClrType,
            RequestAdapterState state)
        {
            if (state.Request.Kind != EndpointKind.AtomicOperations)
            {
                AssertHasNoLid(identity, state);
            }

            AssertNoIdWithLid(identity, state);

            if (requirements.IdConstraint == JsonElementConstraint.Required)
            {
                AssertHasIdOrLid(identity, requirements, state);
            }
            else if (requirements.IdConstraint == JsonElementConstraint.Forbidden)
            {
                AssertHasNoId(identity, state);
            }

            AssertSameIdValue(identity, requirements.IdValue, state);
            AssertSameLidValue(identity, requirements.LidValue, state);

            IIdentifiable resource = _resourceFactory.CreateInstance(resourceClrType);
            AssignStringId(identity, resource, state);
            resource.LocalId = identity.Lid;
            return resource;
        }

        private static void AssertHasNoLid(IResourceIdentity identity, RequestAdapterState state)
        {
            if (identity.Lid != null)
            {
                using IDisposable _ = state.Position.PushElement("lid");
                throw new ModelConversionException(state.Position, "The 'lid' element is not supported at this endpoint.", null);
            }
        }

        private static void AssertNoIdWithLid(IResourceIdentity identity, RequestAdapterState state)
        {
            if (identity.Id != null && identity.Lid != null)
            {
                throw new ModelConversionException(state.Position, "The 'id' and 'lid' element are mutually exclusive.", null);
            }
        }

        private static void AssertHasIdOrLid(IResourceIdentity identity, ResourceIdentityRequirements requirements, RequestAdapterState state)
        {
            string message = null;

            if (requirements.IdValue != null && identity.Id == null)
            {
                message = "The 'id' element is required.";
            }
            else if (requirements.LidValue != null && identity.Lid == null)
            {
                message = "The 'lid' element is required.";
            }
            else if (identity.Id == null && identity.Lid == null)
            {
                message = state.Request.Kind == EndpointKind.AtomicOperations ? "The 'id' or 'lid' element is required." : "The 'id' element is required.";
            }

            if (message != null)
            {
                throw new ModelConversionException(state.Position, message, null);
            }
        }

        private static void AssertHasNoId(IResourceIdentity identity, RequestAdapterState state)
        {
            if (identity.Id != null)
            {
                using IDisposable _ = state.Position.PushElement("id");
                throw new ModelConversionException(state.Position, "The use of client-generated IDs is disabled.", null, HttpStatusCode.Forbidden);
            }
        }

        private static void AssertSameIdValue(IResourceIdentity identity, string expected, RequestAdapterState state)
        {
            if (expected != null && identity.Id != expected)
            {
                using IDisposable _ = state.Position.PushElement("id");

                throw new ModelConversionException(state.Position, "Conflicting 'id' values found.", $"Expected '{expected}' instead of '{identity.Id}'.",
                    HttpStatusCode.Conflict);
            }
        }

        private static void AssertSameLidValue(IResourceIdentity identity, string expected, RequestAdapterState state)
        {
            if (expected != null && identity.Lid != expected)
            {
                using IDisposable _ = state.Position.PushElement("lid");

                throw new ModelConversionException(state.Position, "Conflicting 'lid' values found.", $"Expected '{expected}' instead of '{identity.Lid}'.",
                    HttpStatusCode.Conflict);
            }
        }

        private void AssignStringId(IResourceIdentity identity, IIdentifiable resource, RequestAdapterState state)
        {
            if (identity.Id != null)
            {
                try
                {
                    resource.StringId = identity.Id;
                }
                catch (FormatException exception)
                {
                    using IDisposable _ = state.Position.PushElement("id");
                    throw new ModelConversionException(state.Position, "Incompatible 'id' value found.", exception.Message);
                }
            }
        }

        protected static void AssertIsKnownRelationship(RelationshipAttribute relationship, string relationshipName, ResourceType resourceType,
            RequestAdapterState state)
        {
            if (relationship == null)
            {
                throw new ModelConversionException(state.Position, "Unknown relationship found.",
                    $"Relationship '{relationshipName}' does not exist on resource type '{resourceType.PublicName}'.");
            }
        }

        protected internal static void AssertToManyInAddOrRemoveRelationship(RelationshipAttribute relationship, RequestAdapterState state)
        {
            bool requireToManyRelationship = state.Request.WriteOperation is WriteOperationKind.AddToRelationship or WriteOperationKind.RemoveFromRelationship;

            if (requireToManyRelationship && relationship is not HasManyAttribute)
            {
                string message = state.Request.Kind == EndpointKind.AtomicOperations
                    ? "Only to-many relationships can be targeted through this operation."
                    : "Only to-many relationships can be targeted through this endpoint.";

                throw new ModelConversionException(state.Position, message, $"Relationship '{relationship.PublicName}' is not a to-many relationship.",
                    HttpStatusCode.Forbidden);
            }
        }
    }
}
