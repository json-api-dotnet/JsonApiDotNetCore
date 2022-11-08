using System.Diagnostics.CodeAnalysis;
using System.Net;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Request.Adapters;

/// <summary>
/// Base class for validating and converting objects that represent an identity.
/// </summary>
public abstract class ResourceIdentityAdapter : BaseAdapter
{
    private readonly IResourceGraph _resourceGraph;
    private readonly IResourceFactory _resourceFactory;

    protected ResourceIdentityAdapter(IResourceGraph resourceGraph, IResourceFactory resourceFactory)
    {
        ArgumentGuard.NotNull(resourceGraph);
        ArgumentGuard.NotNull(resourceFactory);

        _resourceGraph = resourceGraph;
        _resourceFactory = resourceFactory;
    }

    protected (IIdentifiable resource, ResourceType resourceType) ConvertResourceIdentity(ResourceIdentity identity, ResourceIdentityRequirements requirements,
        RequestAdapterState state)
    {
        ArgumentGuard.NotNull(identity);
        ArgumentGuard.NotNull(requirements);
        ArgumentGuard.NotNull(state);

        ResourceType resourceType = ResolveType(identity, requirements, state);
        IIdentifiable resource = CreateResource(identity, requirements, resourceType.ClrType, state);

        return (resource, resourceType);
    }

    private ResourceType ResolveType(ResourceIdentity identity, ResourceIdentityRequirements requirements, RequestAdapterState state)
    {
        AssertHasType(identity.Type, state);

        using IDisposable _ = state.Position.PushElement("type");
        ResourceType? resourceType = _resourceGraph.FindResourceType(identity.Type);

        AssertIsKnownResourceType(resourceType, identity.Type, state);

        if (state.Request.WriteOperation is WriteOperationKind.CreateResource or WriteOperationKind.UpdateResource)
        {
            AssertIsNotAbstractType(resourceType, identity.Type, state);
        }

        AssertIsCompatibleResourceType(resourceType, requirements.ResourceType, requirements.RelationshipName, state);

        return resourceType;
    }

    private static void AssertHasType([NotNull] string? identityType, RequestAdapterState state)
    {
        if (identityType == null)
        {
            throw new ModelConversionException(state.Position, "The 'type' element is required.", null);
        }
    }

    private static void AssertIsKnownResourceType([NotNull] ResourceType? resourceType, string typeName, RequestAdapterState state)
    {
        if (resourceType == null)
        {
            throw new ModelConversionException(state.Position, "Unknown resource type found.", $"Resource type '{typeName}' does not exist.");
        }
    }

    private static void AssertIsNotAbstractType(ResourceType resourceType, string typeName, RequestAdapterState state)
    {
        if (resourceType.ClrType.IsAbstract)
        {
            throw new ModelConversionException(state.Position, "Abstract resource type found.", $"Resource type '{typeName}' is abstract.");
        }
    }

    private static void AssertIsCompatibleResourceType(ResourceType actual, ResourceType? expected, string? relationshipName, RequestAdapterState state)
    {
        if (expected != null && !expected.ClrType.IsAssignableFrom(actual.ClrType))
        {
            string message = relationshipName != null
                ? $"Type '{actual.PublicName}' is not convertible to type '{expected.PublicName}' of relationship '{relationshipName}'."
                : $"Type '{actual.PublicName}' is not convertible to type '{expected.PublicName}'.";

            throw new ModelConversionException(state.Position, "Incompatible resource type found.", message, HttpStatusCode.Conflict);
        }
    }

    private IIdentifiable CreateResource(ResourceIdentity identity, ResourceIdentityRequirements requirements, Type resourceClrType, RequestAdapterState state)
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

    private static void AssertHasNoLid(ResourceIdentity identity, RequestAdapterState state)
    {
        if (identity.Lid != null)
        {
            using IDisposable _ = state.Position.PushElement("lid");
            throw new ModelConversionException(state.Position, "The 'lid' element is not supported at this endpoint.", null);
        }
    }

    private static void AssertNoIdWithLid(ResourceIdentity identity, RequestAdapterState state)
    {
        if (identity.Id != null && identity.Lid != null)
        {
            throw new ModelConversionException(state.Position, "The 'id' and 'lid' element are mutually exclusive.", null);
        }
    }

    private static void AssertHasIdOrLid(ResourceIdentity identity, ResourceIdentityRequirements requirements, RequestAdapterState state)
    {
        string? message = null;

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

    private static void AssertHasNoId(ResourceIdentity identity, RequestAdapterState state)
    {
        if (identity.Id != null)
        {
            using IDisposable _ = state.Position.PushElement("id");
            throw new ModelConversionException(state.Position, "The use of client-generated IDs is disabled.", null, HttpStatusCode.Forbidden);
        }
    }

    private static void AssertSameIdValue(ResourceIdentity identity, string? expected, RequestAdapterState state)
    {
        if (expected != null && identity.Id != expected)
        {
            using IDisposable _ = state.Position.PushElement("id");

            throw new ModelConversionException(state.Position, "Conflicting 'id' values found.", $"Expected '{expected}' instead of '{identity.Id}'.",
                HttpStatusCode.Conflict);
        }
    }

    private static void AssertSameLidValue(ResourceIdentity identity, string? expected, RequestAdapterState state)
    {
        if (expected != null && identity.Lid != expected)
        {
            using IDisposable _ = state.Position.PushElement("lid");

            throw new ModelConversionException(state.Position, "Conflicting 'lid' values found.", $"Expected '{expected}' instead of '{identity.Lid}'.",
                HttpStatusCode.Conflict);
        }
    }

    private void AssignStringId(ResourceIdentity identity, IIdentifiable resource, RequestAdapterState state)
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

    protected static void AssertIsKnownRelationship([NotNull] RelationshipAttribute? relationship, string relationshipName, ResourceType resourceType,
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

    internal static void AssertRelationshipChangeNotBlocked(RelationshipAttribute relationship, RequestAdapterState state)
    {
        switch (state.Request.WriteOperation)
        {
            case WriteOperationKind.AddToRelationship:
            {
                AssertAddToRelationshipNotBlocked((HasManyAttribute)relationship, state);
                break;
            }
            case WriteOperationKind.RemoveFromRelationship:
            {
                AssertRemoveFromRelationshipNotBlocked((HasManyAttribute)relationship, state);
                break;
            }
            default:
            {
                AssertSetRelationshipNotBlocked(relationship, state);
                break;
            }
        }
    }

    private static void AssertSetRelationshipNotBlocked(RelationshipAttribute relationship, RequestAdapterState state)
    {
        if (relationship.IsSetBlocked())
        {
            throw new ModelConversionException(state.Position, "Relationship cannot be assigned.",
                $"The relationship '{relationship.PublicName}' on resource type '{relationship.LeftType.PublicName}' cannot be assigned to.");
        }
    }

    private static void AssertAddToRelationshipNotBlocked(HasManyAttribute relationship, RequestAdapterState state)
    {
        if (!relationship.Capabilities.HasFlag(HasManyCapabilities.AllowAdd))
        {
            throw new ModelConversionException(state.Position, "Relationship cannot be added to.",
                $"The relationship '{relationship.PublicName}' on resource type '{relationship.LeftType.PublicName}' cannot be added to.");
        }
    }

    private static void AssertRemoveFromRelationshipNotBlocked(HasManyAttribute relationship, RequestAdapterState state)
    {
        if (!relationship.Capabilities.HasFlag(HasManyCapabilities.AllowRemove))
        {
            throw new ModelConversionException(state.Position, "Relationship cannot be removed from.",
                $"The relationship '{relationship.PublicName}' on resource type '{relationship.LeftType.PublicName}' cannot be removed from.");
        }
    }
}
