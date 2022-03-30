using System.Collections.Immutable;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Resources.Internal;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Response;

/// <inheritdoc />
[PublicAPI]
public class ResponseModelAdapter : IResponseModelAdapter
{
    private static readonly CollectionConverter CollectionConverter = new();

    private readonly IJsonApiRequest _request;
    private readonly IJsonApiOptions _options;
    private readonly ILinkBuilder _linkBuilder;
    private readonly IMetaBuilder _metaBuilder;
    private readonly IResourceDefinitionAccessor _resourceDefinitionAccessor;
    private readonly IEvaluatedIncludeCache _evaluatedIncludeCache;
    private readonly IRequestQueryStringAccessor _requestQueryStringAccessor;
    private readonly ISparseFieldSetCache _sparseFieldSetCache;

    // Ensures that at most one ResourceObject (and one tree node) is produced per resource instance.
    private readonly Dictionary<IIdentifiable, ResourceObjectTreeNode> _resourceToTreeNodeCache = new(IdentifiableComparer.Instance);

    public ResponseModelAdapter(IJsonApiRequest request, IJsonApiOptions options, ILinkBuilder linkBuilder, IMetaBuilder metaBuilder,
        IResourceDefinitionAccessor resourceDefinitionAccessor, IEvaluatedIncludeCache evaluatedIncludeCache, ISparseFieldSetCache sparseFieldSetCache,
        IRequestQueryStringAccessor requestQueryStringAccessor)
    {
        ArgumentGuard.NotNull(request, nameof(request));
        ArgumentGuard.NotNull(options, nameof(options));
        ArgumentGuard.NotNull(linkBuilder, nameof(linkBuilder));
        ArgumentGuard.NotNull(metaBuilder, nameof(metaBuilder));
        ArgumentGuard.NotNull(resourceDefinitionAccessor, nameof(resourceDefinitionAccessor));
        ArgumentGuard.NotNull(evaluatedIncludeCache, nameof(evaluatedIncludeCache));
        ArgumentGuard.NotNull(sparseFieldSetCache, nameof(sparseFieldSetCache));
        ArgumentGuard.NotNull(requestQueryStringAccessor, nameof(requestQueryStringAccessor));

        _request = request;
        _options = options;
        _linkBuilder = linkBuilder;
        _metaBuilder = metaBuilder;
        _resourceDefinitionAccessor = resourceDefinitionAccessor;
        _evaluatedIncludeCache = evaluatedIncludeCache;
        _sparseFieldSetCache = sparseFieldSetCache;
        _requestQueryStringAccessor = requestQueryStringAccessor;
    }

    /// <inheritdoc />
    public Document Convert(object? model)
    {
        _sparseFieldSetCache.Reset();
        _resourceToTreeNodeCache.Clear();

        var document = new Document();

        IncludeExpression? include = _evaluatedIncludeCache.Get();
        IImmutableSet<IncludeElementExpression> includeElements = include?.Elements ?? ImmutableHashSet<IncludeElementExpression>.Empty;

        var rootNode = ResourceObjectTreeNode.CreateRoot();

        if (model is IEnumerable<IIdentifiable> resources)
        {
            ResourceType resourceType = (_request.SecondaryResourceType ?? _request.PrimaryResourceType)!;

            foreach (IIdentifiable resource in resources)
            {
                TraverseResource(resource, resourceType, _request.Kind, includeElements, rootNode, null);
            }

            PopulateRelationshipsInTree(rootNode, _request.Kind);

            IEnumerable<ResourceObject> resourceObjects = rootNode.GetResponseData();
            document.Data = new SingleOrManyData<ResourceObject>(resourceObjects);
        }
        else if (model is IIdentifiable resource)
        {
            ResourceType resourceType = (_request.SecondaryResourceType ?? _request.PrimaryResourceType)!;

            TraverseResource(resource, resourceType, _request.Kind, includeElements, rootNode, null);
            PopulateRelationshipsInTree(rootNode, _request.Kind);

            ResourceObject resourceObject = rootNode.GetResponseData().Single();
            document.Data = new SingleOrManyData<ResourceObject>(resourceObject);
        }
        else if (model == null)
        {
            document.Data = new SingleOrManyData<ResourceObject>(null);
        }
        else if (model is IEnumerable<OperationContainer?> operations)
        {
            using var _ = new RevertRequestStateOnDispose(_request, null);
            document.Results = operations.Select(operation => ConvertOperation(operation, includeElements)).ToList();
        }
        else if (model is IEnumerable<ErrorObject> errorObjects)
        {
            document.Errors = errorObjects.ToArray();
        }
        else if (model is ErrorObject errorObject)
        {
            document.Errors = errorObject.AsArray();
        }
        else
        {
            throw new InvalidOperationException("Data being returned must be resources, operations, errors or null.");
        }

        document.JsonApi = GetApiObject();
        document.Links = _linkBuilder.GetTopLevelLinks();
        document.Meta = _metaBuilder.Build();
        document.Included = GetIncluded(rootNode);

        return document;
    }

    protected virtual AtomicResultObject ConvertOperation(OperationContainer? operation, IImmutableSet<IncludeElementExpression> includeElements)
    {
        ResourceObject? resourceObject = null;

        if (operation != null)
        {
            _request.CopyFrom(operation.Request);

            ResourceType resourceType = (operation.Request.SecondaryResourceType ?? operation.Request.PrimaryResourceType)!;
            var rootNode = ResourceObjectTreeNode.CreateRoot();

            TraverseResource(operation.Resource, resourceType, operation.Request.Kind, includeElements, rootNode, null);
            PopulateRelationshipsInTree(rootNode, operation.Request.Kind);

            resourceObject = rootNode.GetResponseData().Single();

            _sparseFieldSetCache.Reset();
            _resourceToTreeNodeCache.Clear();
        }

        return new AtomicResultObject
        {
            Data = resourceObject == null ? default : new SingleOrManyData<ResourceObject>(resourceObject)
        };
    }

    private void TraverseResource(IIdentifiable resource, ResourceType resourceType, EndpointKind kind, IImmutableSet<IncludeElementExpression> includeElements,
        ResourceObjectTreeNode parentTreeNode, RelationshipAttribute? parentRelationship)
    {
        ResourceObjectTreeNode treeNode = GetOrCreateTreeNode(resource, resourceType, kind);

        if (parentRelationship != null)
        {
            parentTreeNode.AttachRelationshipChild(parentRelationship, treeNode);
        }
        else
        {
            parentTreeNode.AttachDirectChild(treeNode);
        }

        if (kind != EndpointKind.Relationship)
        {
            TraverseRelationships(resource, treeNode, includeElements, kind);
        }
    }

    private ResourceObjectTreeNode GetOrCreateTreeNode(IIdentifiable resource, ResourceType resourceType, EndpointKind kind)
    {
        if (!_resourceToTreeNodeCache.TryGetValue(resource, out ResourceObjectTreeNode? treeNode))
        {
            // In case of resource inheritance, prefer the derived resource type over the base type.
            ResourceType effectiveResourceType = GetEffectiveResourceType(resource, resourceType);

            ResourceObject resourceObject = ConvertResource(resource, effectiveResourceType, kind);
            treeNode = new ResourceObjectTreeNode(resource, effectiveResourceType, resourceObject);

            _resourceToTreeNodeCache.Add(resource, treeNode);
        }

        return treeNode;
    }

    private static ResourceType GetEffectiveResourceType(IIdentifiable resource, ResourceType declaredType)
    {
        Type runtimeResourceType = resource.GetClrType();

        if (declaredType.ClrType == runtimeResourceType)
        {
            return declaredType;
        }

        ResourceType? derivedType = declaredType.GetAllConcreteDerivedTypes().FirstOrDefault(type => type.ClrType == runtimeResourceType);

        if (derivedType == null)
        {
            throw new InvalidConfigurationException($"Type '{runtimeResourceType}' does not exist in the resource graph.");
        }

        return derivedType;
    }

    protected virtual ResourceObject ConvertResource(IIdentifiable resource, ResourceType resourceType, EndpointKind kind)
    {
        bool isRelationship = kind == EndpointKind.Relationship;

        if (!isRelationship)
        {
            _resourceDefinitionAccessor.OnSerialize(resource);
        }

        var resourceObject = new ResourceObject
        {
            Type = resourceType.PublicName,
            Id = resource.StringId
        };

        if (!isRelationship)
        {
            IImmutableSet<ResourceFieldAttribute> fieldSet = _sparseFieldSetCache.GetSparseFieldSetForSerializer(resourceType);

            resourceObject.Attributes = ConvertAttributes(resource, resourceType, fieldSet);
            resourceObject.Links = _linkBuilder.GetResourceLinks(resourceType, resource);
            resourceObject.Meta = _resourceDefinitionAccessor.GetMeta(resourceType, resource);
        }

        return resourceObject;
    }

    protected virtual IDictionary<string, object?>? ConvertAttributes(IIdentifiable resource, ResourceType resourceType,
        IImmutableSet<ResourceFieldAttribute> fieldSet)
    {
        var attrMap = new Dictionary<string, object?>(resourceType.Attributes.Count);

        foreach (AttrAttribute attr in resourceType.Attributes)
        {
            if (!fieldSet.Contains(attr) || attr.Property.Name == nameof(Identifiable<object>.Id))
            {
                continue;
            }

            object? value = attr.GetValue(resource);

            if (_options.SerializerOptions.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull && value == null)
            {
                continue;
            }

            if (_options.SerializerOptions.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingDefault &&
                Equals(value, RuntimeTypeConverter.GetDefaultValue(attr.Property.PropertyType)))
            {
                continue;
            }

            attrMap.Add(attr.PublicName, value);
        }

        return attrMap.Any() ? attrMap : null;
    }

    private void TraverseRelationships(IIdentifiable leftResource, ResourceObjectTreeNode leftTreeNode, IImmutableSet<IncludeElementExpression> includeElements,
        EndpointKind kind)
    {
        foreach (IncludeElementExpression includeElement in includeElements)
        {
            TraverseRelationship(includeElement.Relationship, leftResource, leftTreeNode, includeElement, kind);
        }
    }

    private void TraverseRelationship(RelationshipAttribute relationship, IIdentifiable leftResource, ResourceObjectTreeNode leftTreeNode,
        IncludeElementExpression includeElement, EndpointKind kind)
    {
        if (!relationship.LeftType.ClrType.IsAssignableFrom(leftTreeNode.ResourceType.ClrType))
        {
            // Skipping over relationship that is declared on another derived type.
            return;
        }

        // In case of resource inheritance, prefer the relationship on derived type over the one on base type.
        RelationshipAttribute effectiveRelationship = !leftTreeNode.ResourceType.Equals(relationship.LeftType)
            ? leftTreeNode.ResourceType.GetRelationshipByPropertyName(relationship.Property.Name)
            : relationship;

        object? rightValue = effectiveRelationship.GetValue(leftResource);
        ICollection<IIdentifiable> rightResources = CollectionConverter.ExtractResources(rightValue);

        leftTreeNode.EnsureHasRelationship(effectiveRelationship);

        foreach (IIdentifiable rightResource in rightResources)
        {
            TraverseResource(rightResource, effectiveRelationship.RightType, kind, includeElement.Children, leftTreeNode, effectiveRelationship);
        }
    }

    private void PopulateRelationshipsInTree(ResourceObjectTreeNode rootNode, EndpointKind kind)
    {
        if (kind != EndpointKind.Relationship)
        {
            foreach (ResourceObjectTreeNode treeNode in rootNode.GetUniqueNodes())
            {
                PopulateRelationshipsInResourceObject(treeNode);
            }
        }
    }

    private void PopulateRelationshipsInResourceObject(ResourceObjectTreeNode treeNode)
    {
        IImmutableSet<ResourceFieldAttribute> fieldSet = _sparseFieldSetCache.GetSparseFieldSetForSerializer(treeNode.ResourceType);

        foreach (RelationshipAttribute relationship in treeNode.ResourceType.Relationships)
        {
            if (fieldSet.Contains(relationship))
            {
                PopulateRelationshipInResourceObject(treeNode, relationship);
            }
        }
    }

    private void PopulateRelationshipInResourceObject(ResourceObjectTreeNode treeNode, RelationshipAttribute relationship)
    {
        SingleOrManyData<ResourceIdentifierObject> data = GetRelationshipData(treeNode, relationship);
        RelationshipLinks? links = _linkBuilder.GetRelationshipLinks(relationship, treeNode.Resource);

        if (links != null || data.IsAssigned)
        {
            var relationshipObject = new RelationshipObject
            {
                Links = links,
                Data = data
            };

            treeNode.ResourceObject.Relationships ??= new Dictionary<string, RelationshipObject?>();
            treeNode.ResourceObject.Relationships.Add(relationship.PublicName, relationshipObject);
        }
    }

    private static SingleOrManyData<ResourceIdentifierObject> GetRelationshipData(ResourceObjectTreeNode treeNode, RelationshipAttribute relationship)
    {
        ISet<ResourceObjectTreeNode>? rightNodes = treeNode.GetRightNodesInRelationship(relationship);

        if (rightNodes != null)
        {
            IEnumerable<ResourceIdentifierObject> resourceIdentifierObjects = rightNodes.Select(rightNode => new ResourceIdentifierObject
            {
                Type = rightNode.ResourceType.PublicName,
                Id = rightNode.ResourceObject.Id
            });

            return relationship is HasOneAttribute
                ? new SingleOrManyData<ResourceIdentifierObject>(resourceIdentifierObjects.SingleOrDefault())
                : new SingleOrManyData<ResourceIdentifierObject>(resourceIdentifierObjects);
        }

        return default;
    }

    protected virtual JsonApiObject? GetApiObject()
    {
        if (!_options.IncludeJsonApiVersion)
        {
            return null;
        }

        var jsonApiObject = new JsonApiObject
        {
            Version = "1.1"
        };

        if (_request.Kind == EndpointKind.AtomicOperations)
        {
            jsonApiObject.Ext = new List<string>
            {
                "https://jsonapi.org/ext/atomic"
            };
        }

        return jsonApiObject;
    }

    private IList<ResourceObject>? GetIncluded(ResourceObjectTreeNode rootNode)
    {
        IList<ResourceObject> resourceObjects = rootNode.GetResponseIncluded();

        if (resourceObjects.Any())
        {
            return resourceObjects;
        }

        return _requestQueryStringAccessor.Query.ContainsKey("include") ? Array.Empty<ResourceObject>() : null;
    }
}
