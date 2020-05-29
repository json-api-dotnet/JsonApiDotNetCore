using System;
using System.Linq;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Queries.Expressions;
using JsonApiDotNetCore.Internal.Queries.Parsing;
using JsonApiDotNetCore.Models.Annotation;
using JsonApiDotNetCore.RequestServices.Contracts;

namespace JsonApiDotNetCore.Internal.QueryStrings
{
    public abstract class QueryStringParameterReader
    {
        private readonly bool _isCollectionRequest;

        protected IResourceContextProvider ResourceContextProvider { get; }
        protected ResourceFieldChainResolver ChainResolver { get; }
        protected ResourceContext RequestResource { get; }

        protected QueryStringParameterReader(ICurrentRequest currentRequest, IResourceContextProvider resourceContextProvider)
        {
            if (currentRequest == null)
            {
                throw new ArgumentNullException(nameof(currentRequest));
            }

            ResourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
            ChainResolver = new ResourceFieldChainResolver(resourceContextProvider);
            RequestResource = currentRequest.SecondaryResource ?? currentRequest.PrimaryResource;
            _isCollectionRequest = currentRequest.IsCollection;
        }

        protected ResourceContext GetResourceContextForScope(ResourceFieldChainExpression scope)
        {
            if (scope == null)
            {
                return RequestResource;
            }

            var lastField = scope.Fields.Last();
            var type = lastField is RelationshipAttribute relationship ? relationship.RightType : lastField.Property.PropertyType;

            return ResourceContextProvider.GetResourceContext(type);
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
