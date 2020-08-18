using System;
using System.Linq;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Queries.Parsing;
using JsonApiDotNetCore.Models.Annotation;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.RequestServices.Contracts;

namespace JsonApiDotNetCore.Internal.QueryStrings
{
    public abstract class QueryStringParameterReader
    {
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly bool _isCollectionRequest;

        protected ResourceContext RequestResource { get; }

        protected QueryStringParameterReader(IJsonApiRequest request, IResourceContextProvider resourceContextProvider)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
            _isCollectionRequest = request.IsCollection;
            RequestResource = request.SecondaryResource ?? request.PrimaryResource;
        }

        protected ResourceContext GetResourceContextForScope(ResourceFieldChainExpression scope)
        {
            if (scope == null)
            {
                return RequestResource;
            }

            var lastField = scope.Fields.Last();
            var type = lastField is RelationshipAttribute relationship ? relationship.RightType : lastField.Property.PropertyType;

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
