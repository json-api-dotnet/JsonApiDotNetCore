using System;
using System.Net;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.RequestAdapters
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

        protected (IIdentifiable resource, ResourceContext resourceContext) ConvertResourceIdentity(IResourceIdentity identity,
            ResourceIdentityRequirements requirements, RequestAdapterState state)
        {
            ArgumentGuard.NotNull(identity, nameof(identity));
            ArgumentGuard.NotNull(requirements, nameof(requirements));
            ArgumentGuard.NotNull(state, nameof(state));

            ResourceContext resourceContext = ConvertType(identity, requirements, state);

            if (state.Request.Kind != EndpointKind.AtomicOperations)
            {
                AssertHasNoLid(identity, state);
            }

            AssertNoIdWithLid(identity, state);

            if (requirements.IdConstraint == JsonElementConstraint.Required)
            {
                AssertHasIdOrLid(identity, state);
            }
            else if (requirements.IdConstraint == JsonElementConstraint.Forbidden)
            {
                AssertHasNoId(identity, state);
            }

            if (requirements.IdValue != null && identity.Id != null && identity.Id != requirements.IdValue)
            {
                using (state.Position.PushElement("id"))
                {
                    if (state.Request.Kind == EndpointKind.AtomicOperations)
                    {
                        throw new DeserializationException(state.Position, "Resource ID mismatch between 'ref.id' and 'data.id' element.",
                            $"Expected resource with ID '{requirements.IdValue}' in 'data.id', instead of '{identity.Id}'.");
                    }

                    throw new JsonApiException(new ErrorObject(HttpStatusCode.Conflict)
                    {
                        Title = "Resource ID mismatch between request body and endpoint URL.",
                        Detail = $"Expected resource ID '{requirements.IdValue}', instead of '{identity.Id}'.",
                        Source = new ErrorSource
                        {
                            Pointer = state.Position.ToSourcePointer()
                        }
                    });
                }
            }

            if (requirements.LidValue != null && identity.Lid != null && identity.Lid != requirements.LidValue)
            {
                using (state.Position.PushElement("lid"))
                {
                    throw new DeserializationException(state.Position, "Resource local ID mismatch between 'ref.lid' and 'data.lid' element.",
                        $"Expected resource with local ID '{requirements.LidValue}' in 'data.lid', instead of '{identity.Lid}'.");
                }
            }

            if (requirements.IdValue != null && identity.Lid != null)
            {
                using (state.Position.PushElement("lid"))
                {
                    throw new DeserializationException(state.Position, "Resource identity mismatch between 'ref.id' and 'data.lid' element.",
                        $"Expected resource with ID '{requirements.IdValue}' in 'data.id', instead of '{identity.Lid}' in 'data.lid'.");
                }
            }

            if (requirements.LidValue != null && identity.Id != null)
            {
                using (state.Position.PushElement("id"))
                {
                    throw new DeserializationException(state.Position, "Resource identity mismatch between 'ref.lid' and 'data.id' element.",
                        $"Expected resource with local ID '{requirements.LidValue}' in 'data.lid', instead of '{identity.Id}' in 'data.id'.");
                }
            }

            IIdentifiable resource = _resourceFactory.CreateInstance(resourceContext.ResourceType);
            AssignStringId(identity, resource, state);
            resource.LocalId = identity.Lid;

            return (resource, resourceContext);
        }

        private ResourceContext ConvertType(IResourceIdentity identity, ResourceIdentityRequirements requirements, RequestAdapterState state)
        {
            if (identity.Type == null)
            {
                string parent = identity is AtomicReference
                    ? "'ref' element"
                    :
                    requirements?.RelationshipName != null &&
                    state.Request.WriteOperation is WriteOperationKind.CreateResource or WriteOperationKind.UpdateResource
                        ?
                        $"'{requirements.RelationshipName}' relationship"
                        : "'data' element";

                throw new DeserializationException(state.Position, "Request body must include 'type' element.", $"Expected 'type' element in {parent}.");
            }

            using IDisposable _ = state.Position.PushElement("type");

            ResourceContext resourceContext = _resourceGraph.TryGetResourceContext(identity.Type);

            AssertIsKnownResourceType(resourceContext, identity.Type, state);

            if (requirements?.ResourceContext != null && !requirements.ResourceContext.ResourceType.IsAssignableFrom(resourceContext.ResourceType))
            {
                if (requirements.UseLegacyError)
                {
                    throw new DeserializationException(state.Position, "Relationship contains incompatible resource type.",
                        $"Relationship '{requirements.RelationshipName}' contains incompatible resource type '{resourceContext.PublicName}'.");
                }

                if (state.Request.Kind == EndpointKind.AtomicOperations)
                {
                    throw new DeserializationException(state.Position, "Resource type mismatch between 'ref.type' and 'data.type' element.",
                        $"Expected resource of type '{requirements.ResourceContext.PublicName}' in 'data.type', instead of '{resourceContext.PublicName}'.");
                }

                string title = requirements.RelationshipName != null ? "Resource type is incompatible with relationship type." :
                    state.Request.Kind == EndpointKind.AtomicOperations ? "Resource type is incompatible with type in ref." :
                    "Resource type is incompatible with endpoint URL.";

                string detail = requirements.RelationshipName != null
                    ? $"Type '{resourceContext.PublicName}' is incompatible with type '{requirements.ResourceContext.PublicName}' of relationship '{requirements.RelationshipName}'."
                    : $"Type '{resourceContext.PublicName}' is incompatible with type '{requirements.ResourceContext.PublicName}'.";

                throw new JsonApiException(new ErrorObject(HttpStatusCode.Conflict)
                {
                    Title = title,
                    Detail = detail,
                    Source = new ErrorSource
                    {
                        Pointer = state.Position.ToSourcePointer()
                    }
                });
            }

            return resourceContext;
        }

        private static void AssertIsKnownResourceType(ResourceContext resourceContext, string typeName, RequestAdapterState state)
        {
            if (resourceContext == null)
            {
                throw new ModelConversionException(state.Position, "Unknown resource type found.", $"Resource type '{typeName}' does not exist.");
            }
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

        private static void AssertHasIdOrLid(IResourceIdentity identity, RequestAdapterState state)
        {
            if (identity.Id == null && identity.Lid == null)
            {
                string message = state.Request.Kind == EndpointKind.AtomicOperations
                    ? "The 'id' or 'lid' element is required."
                    : "The 'id' element is required.";

                throw new ModelConversionException(state.Position, message, null);
            }
        }

        private static void AssertHasNoId(IResourceIdentity identity, RequestAdapterState state)
        {
            if (identity.Id != null)
            {
                using IDisposable _ = state.Position.PushElement("id");
                throw new ForbiddenClientGeneratedIdException(state.Position.ToSourcePointer());
            }
        }

        private void AssignStringId(IResourceIdentity identity, IIdentifiable resource, RequestAdapterState state)
        {
            if (identity.Id != null)
            {
                using IDisposable _ = state.Position.PushElement("id");

                try
                {
                    resource.StringId = identity.Id;
                }
                catch (FormatException exception)
                {
                    throw new DeserializationException(state.Position, null, exception.Message);
                }
            }
        }

        protected static void AssertIsKnownRelationship(RelationshipAttribute relationship, string relationshipName, ResourceContext resourceContext,
            RequestAdapterState state)
        {
            if (relationship == null)
            {
                throw new ModelConversionException(state.Position, "Unknown relationship found.",
                    $"Relationship '{relationshipName}' does not exist on resource type '{resourceContext.PublicName}'.");
            }
        }
    }
}
