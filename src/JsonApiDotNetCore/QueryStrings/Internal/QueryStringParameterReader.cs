using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal.Parsing;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.QueryStrings.Internal
{
    public abstract class QueryStringParameterReader
    {
        private readonly IResourceGraph _resourceGraph;
        private readonly bool _isCollectionRequest;

        protected ResourceType RequestResourceType { get; }
        protected bool IsAtomicOperationsRequest { get; }

        protected QueryStringParameterReader(IJsonApiRequest request, IResourceGraph resourceGraph)
        {
            ArgumentGuard.NotNull(request, nameof(request));
            ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));

            _resourceGraph = resourceGraph;
            _isCollectionRequest = request.IsCollection;
            // There are currently no query string readers that work with operations, so non-nullable for convenience.
            RequestResourceType = (request.SecondaryResourceType ?? request.PrimaryResourceType)!;
            IsAtomicOperationsRequest = request.Kind == EndpointKind.AtomicOperations;
        }

        protected ResourceType GetResourceTypeForScope(ResourceFieldChainExpression? scope)
        {
            if (scope == null)
            {
                return RequestResourceType;
            }

            ResourceFieldAttribute lastField = scope.Fields[^1];

            if (lastField is RelationshipAttribute relationship)
            {
                return relationship.RightType;
            }

            return _resourceGraph.GetResourceType(lastField.Property.PropertyType);
        }

        protected void AssertIsCollectionRequest()
        {
            if (!_isCollectionRequest)
            {
                throw new QueryParseException("This query string parameter can only be used on a collection of resources (not on a single resource).");
            }
        }
    }
}
