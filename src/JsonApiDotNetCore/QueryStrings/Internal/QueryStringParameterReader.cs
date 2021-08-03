using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Queries.Internal.Parsing;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.QueryStrings.Internal
{
    public abstract class QueryStringParameterReader
    {
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly bool _isCollectionRequest;

        protected ResourceContext RequestResource { get; }
        protected bool IsAtomicOperationsRequest { get; }

        protected QueryStringParameterReader(IJsonApiRequest request, IResourceContextProvider resourceContextProvider)
        {
            ArgumentGuard.NotNull(request, nameof(request));
            ArgumentGuard.NotNull(resourceContextProvider, nameof(resourceContextProvider));

            _resourceContextProvider = resourceContextProvider;
            _isCollectionRequest = request.IsCollection;
            RequestResource = request.SecondaryResource ?? request.PrimaryResource;
            IsAtomicOperationsRequest = request.Kind == EndpointKind.AtomicOperations;
        }

        protected ResourceContext GetResourceContextForScope(ResourceFieldChainExpression scope)
        {
            if (scope == null)
            {
                return RequestResource;
            }

            ResourceFieldAttribute lastField = scope.Fields[^1];
            Type type = lastField is RelationshipAttribute relationship ? relationship.RightType : lastField.Property.PropertyType;

            return _resourceContextProvider.GetResourceContext(type);
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
