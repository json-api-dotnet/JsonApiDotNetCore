using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using System;
using System.Linq;

namespace JsonApiDotNetCore.Internal.Query
{
    public class AttrQuery
    {
        private readonly IJsonApiContext _jsonApiContext;

        public AttrQuery(IJsonApiContext jsonApiContext, QueryAttribute query)
        {
            _jsonApiContext = jsonApiContext;
            Attribute = GetAttribute(query.Attribute);
        }

        public AttrAttribute Attribute { get; }

        private AttrAttribute GetAttribute(string attribute)
        {
            try
            {
                return _jsonApiContext
                    .RequestEntity
                    .Attributes
                    .Single(attr => attr.Is(attribute));
            }
            catch (InvalidOperationException e)
            {
                throw new JsonApiException(400, $"Attribute '{attribute}' does not exist on resource '{_jsonApiContext.RequestEntity.EntityName}'", e);
            }
        }

    }
}
