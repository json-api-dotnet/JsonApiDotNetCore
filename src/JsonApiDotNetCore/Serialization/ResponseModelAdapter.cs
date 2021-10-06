using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;
using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Resources.Internal;
using JsonApiDotNetCore.Serialization.Building;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization
{
    /// <inheritdoc />
    public sealed class ResponseModelAdapter : IResponseModelAdapter
    {
        private static readonly CollectionConverter CollectionConverter = new();

        private readonly IJsonApiRequest _request;
        private readonly IJsonApiOptions _options;
        private readonly IResourceGraph _resourceGraph;
        private readonly ILinkBuilder _linkBuilder;
        private readonly IMetaBuilder _metaBuilder;
        private readonly IResourceDefinitionAccessor _resourceDefinitionAccessor;
        private readonly IEvaluatedIncludeCache _evaluatedIncludeCache;
        private readonly IRequestQueryStringAccessor _requestQueryStringAccessor;
        private readonly ISparseFieldSetCache _sparseFieldSetCache;

        public ResponseModelAdapter(IJsonApiRequest request, IJsonApiOptions options, IResourceGraph resourceGraph, ILinkBuilder linkBuilder,
            IMetaBuilder metaBuilder, IResourceDefinitionAccessor resourceDefinitionAccessor, IEvaluatedIncludeCache evaluatedIncludeCache,
            ISparseFieldSetCache sparseFieldSetCache, IRequestQueryStringAccessor requestQueryStringAccessor)
        {
            ArgumentGuard.NotNull(request, nameof(request));
            ArgumentGuard.NotNull(options, nameof(options));
            ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));
            ArgumentGuard.NotNull(linkBuilder, nameof(linkBuilder));
            ArgumentGuard.NotNull(metaBuilder, nameof(metaBuilder));
            ArgumentGuard.NotNull(resourceDefinitionAccessor, nameof(resourceDefinitionAccessor));
            ArgumentGuard.NotNull(evaluatedIncludeCache, nameof(evaluatedIncludeCache));
            ArgumentGuard.NotNull(sparseFieldSetCache, nameof(sparseFieldSetCache));
            ArgumentGuard.NotNull(requestQueryStringAccessor, nameof(requestQueryStringAccessor));

            _request = request;
            _options = options;
            _resourceGraph = resourceGraph;
            _linkBuilder = linkBuilder;
            _metaBuilder = metaBuilder;
            _resourceDefinitionAccessor = resourceDefinitionAccessor;
            _evaluatedIncludeCache = evaluatedIncludeCache;
            _sparseFieldSetCache = sparseFieldSetCache;
            _requestQueryStringAccessor = requestQueryStringAccessor;
        }

        /// <inheritdoc />
        public Document Convert(object model)
        {
            _sparseFieldSetCache.Reset();

            var document = new Document();

            IncludeExpression include = _evaluatedIncludeCache.Get();
            IImmutableSet<IncludeElementExpression> includeElements = include?.Elements ?? ImmutableHashSet<IncludeElementExpression>.Empty;

            var includedCollection = new IncludedCollection();

            if (model is IEnumerable<IIdentifiable> resources)
            {
                IEnumerable<ResourceObject> resourceObjects =
                    resources.Select(resource => ConvertResource(resource, _request.Kind, includeElements, includedCollection, false));

                document.Data = new SingleOrManyData<ResourceObject>(resourceObjects);
            }
            else if (model is IIdentifiable resource)
            {
                ResourceObject resourceObject = ConvertResource(resource, _request.Kind, includeElements, includedCollection, false);
                document.Data = new SingleOrManyData<ResourceObject>(resourceObject);
            }
            else if (model == null)
            {
                document.Data = new SingleOrManyData<ResourceObject>(null);
            }
            else if (model is IEnumerable<OperationContainer> operations)
            {
                using var _ = new RevertRequestStateOnDispose(_request, null);
                document.Results = operations.Select(operation => ConvertOperation(operation, includeElements, includedCollection)).ToList();
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
            document.Included = GetIncluded(includedCollection);

            return document;
        }

        private AtomicResultObject ConvertOperation(OperationContainer operation, IImmutableSet<IncludeElementExpression> includeElements,
            IncludedCollection includedCollection)
        {
            ResourceObject resourceObject = null;

            if (operation != null)
            {
                _request.CopyFrom(operation.Request);

                resourceObject = ConvertResource(operation.Resource, operation.Request.Kind, includeElements, includedCollection, false);

                _sparseFieldSetCache.Reset();
            }

            return new AtomicResultObject
            {
                Data = resourceObject == null ? default : new SingleOrManyData<ResourceObject>(resourceObject)
            };
        }

        private ResourceObject ConvertResource(IIdentifiable resource, EndpointKind requestKind, IImmutableSet<IncludeElementExpression> includeElements,
            IncludedCollection includedCollection, bool isInclude)
        {
            ResourceContext resourceContext = _resourceGraph.GetResourceContext(resource.GetType());
            IImmutableSet<ResourceFieldAttribute> fieldSet = null;

            if (requestKind != EndpointKind.Relationship)
            {
                _resourceDefinitionAccessor.OnSerialize(resource);

                fieldSet = _sparseFieldSetCache.GetSparseFieldSetForSerializer(resourceContext);
            }

            var resourceObject = new ResourceObject();

            if (isInclude)
            {
                resourceObject = includedCollection.AddOrUpdate(resource, resourceObject);
            }

            bool isRelationship = requestKind == EndpointKind.Relationship;

            resourceObject.Type = resourceContext.PublicName;
            resourceObject.Id = resource.StringId;
            resourceObject.Attributes = ConvertAttributes(resource, resourceContext, fieldSet);
            resourceObject.Relationships = ConvertRelationships(resource, resourceContext, fieldSet, requestKind, includeElements, includedCollection);
            resourceObject.Links = isRelationship ? null : _linkBuilder.GetResourceLinks(resourceContext.PublicName, resource.StringId);
            resourceObject.Meta = isRelationship ? null : _resourceDefinitionAccessor.GetMeta(resource.GetType(), resource);

            return resourceObject;
        }

        private IDictionary<string, object> ConvertAttributes(IIdentifiable resource, ResourceContext resourceContext,
            IImmutableSet<ResourceFieldAttribute> fieldSet)
        {
            if (fieldSet != null)
            {
                var attrMap = new Dictionary<string, object>(resourceContext.Attributes.Count);

                foreach (AttrAttribute attr in resourceContext.Attributes)
                {
                    if (!fieldSet.Contains(attr) || attr.Property.Name == nameof(Identifiable.Id))
                    {
                        continue;
                    }

                    object value = attr.GetValue(resource);

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

                if (attrMap.Any())
                {
                    return attrMap;
                }
            }

            return null;
        }

        private IDictionary<string, RelationshipObject> ConvertRelationships(IIdentifiable resource, ResourceContext resourceContext,
            IImmutableSet<ResourceFieldAttribute> fieldSet, EndpointKind requestKind, IImmutableSet<IncludeElementExpression> includeElements,
            IncludedCollection includedCollection)
        {
            if (fieldSet != null)
            {
                var relationshipMap = new Dictionary<string, RelationshipObject>(resourceContext.Relationships.Count);

                foreach (RelationshipAttribute relationship in resourceContext.Relationships)
                {
                    IncludeElementExpression includeElement = GetFirstOrDefault(includeElements, relationship,
                        (element, nextRelationship) => element.Relationship.Equals(nextRelationship));

                    RelationshipObject relationshipObject = ConvertRelationship(relationship, resource, requestKind, includeElement, includedCollection);

                    if (relationshipObject != null && fieldSet.Contains(relationship))
                    {
                        relationshipMap.Add(relationship.PublicName, relationshipObject);
                    }
                }

                if (relationshipMap.Any())
                {
                    return relationshipMap;
                }
            }

            return null;
        }

        private static TSource GetFirstOrDefault<TSource, TContext>(IEnumerable<TSource> source, TContext context, Func<TSource, TContext, bool> condition)
        {
            // PERF: This replacement for Enumerable.FirstOrDefault() doesn't allocate a compiler-generated closure class <>c__DisplayClass.
            // https://www.jetbrains.com/help/resharper/2021.2/Fixing_Issues_Found_by_DPA.html#closures-in-lambda-expressions

            foreach (TSource item in source)
            {
                if (condition(item, context))
                {
                    return item;
                }
            }

            return default;
        }

        private RelationshipObject ConvertRelationship(RelationshipAttribute relationship, IIdentifiable leftResource, EndpointKind requestKind,
            IncludeElementExpression includeElement, IncludedCollection includedCollection)
        {
            SingleOrManyData<ResourceIdentifierObject> data = default;

            if (includeElement != null)
            {
                object rightValue = relationship.GetValue(leftResource);
                ICollection<IIdentifiable> rightResources = CollectionConverter.ExtractResources(rightValue);

                var resourceIdentifierObjects = new List<ResourceIdentifierObject>(rightResources.Count);

                foreach (IIdentifiable rightResource in rightResources)
                {
                    var resourceIdentifierObject = new ResourceIdentifierObject
                    {
                        Type = _resourceGraph.GetResourceContext(rightResource.GetType()).PublicName,
                        Id = rightResource.StringId
                    };

                    resourceIdentifierObjects.Add(resourceIdentifierObject);

                    ResourceObject includeResource = ConvertResource(rightResource, requestKind, includeElement.Children, includedCollection, true);
                    includedCollection.AddOrUpdate(rightResource, includeResource);
                }

                data = relationship is HasOneAttribute
                    ? new SingleOrManyData<ResourceIdentifierObject>(resourceIdentifierObjects.SingleOrDefault())
                    : new SingleOrManyData<ResourceIdentifierObject>(resourceIdentifierObjects);
            }

            RelationshipLinks links = _linkBuilder.GetRelationshipLinks(relationship, leftResource);

            return links == null && !data.IsAssigned
                ? null
                : new RelationshipObject
                {
                    Links = links,
                    Data = data
                };
        }

        private JsonApiObject GetApiObject()
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

        private IList<ResourceObject> GetIncluded(IncludedCollection includedCollection)
        {
            if (includedCollection.ResourceObjects.Any())
            {
                return includedCollection.ResourceObjects;
            }

            return _requestQueryStringAccessor.Query.ContainsKey("include") ? Array.Empty<ResourceObject>() : null;
        }

        private sealed class IncludedCollection
        {
            private readonly List<ResourceObject> _includes = new();
            private readonly Dictionary<IIdentifiable, int> _resourceToIncludeIndexMap = new(IdentifiableComparer.Instance);

            public IList<ResourceObject> ResourceObjects => _includes;

            public ResourceObject AddOrUpdate(IIdentifiable resource, ResourceObject resourceObject)
            {
                if (!_resourceToIncludeIndexMap.ContainsKey(resource))
                {
                    _includes.Add(resourceObject);
                    _resourceToIncludeIndexMap.Add(resource, _includes.Count - 1);
                }
                else
                {
                    if (resourceObject.Type != null)
                    {
                        int existingIndex = _resourceToIncludeIndexMap[resource];
                        ResourceObject existingVersion = _includes[existingIndex];

                        if (existingVersion != resourceObject)
                        {
                            MergeRelationships(resourceObject, existingVersion);

                            return existingVersion;
                        }
                    }
                }

                return resourceObject;
            }

            private static void MergeRelationships(ResourceObject incomingVersion, ResourceObject existingVersion)
            {
                // The code below handles the case where one resource is added through different include chains with different relationships.
                // We enrich the existing resource object with the added relationships coming from the second chain, to ensure correct resource linkage.
                //
                // This is best explained using an example. Consider the next inclusion chains:
                //
                // 1. reviewer.loginAttempts
                // 2. author.preferences
                // 
                // Where the relationships `reviewer` and `author` are of the same resource type `people`. Then the next rules apply:
                //
                // A. People that were included as reviewers from inclusion chain (1) should come with their `loginAttempts` included, but not those from chain (2).
                // B. People that were included as authors from inclusion chain (2) should come with their `preferences` included, but not those from chain (1).
                // C. For a person that was included as both an reviewer and author (i.e. targeted by both chains), both `loginAttempts` and `preferences` need
                //    to be present.
                //
                // For rule (C), the related resources will be included as usual, but we need to fix resource linkage here by merging the relationship objects.
                //
                // Note that this implementation breaks the overall depth-first ordering of included objects. So solve that, we'd need to use a dependency graph
                // for included objects instead of a flat list, which may affect performance. Since the ordering is not guaranteed anyway, keeping it simple for now.

                foreach ((string relationshipName, RelationshipObject relationshipObject) in existingVersion.Relationships.EmptyIfNull())
                {
                    if (!relationshipObject.Data.IsAssigned)
                    {
                        SingleOrManyData<ResourceIdentifierObject> incomingRelationshipData = incomingVersion.Relationships[relationshipName].Data;

                        if (incomingRelationshipData.IsAssigned)
                        {
                            relationshipObject.Data = incomingRelationshipData;
                        }
                    }
                }
            }
        }
    }
}
