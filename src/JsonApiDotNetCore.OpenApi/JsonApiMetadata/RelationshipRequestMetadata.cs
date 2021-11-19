using System;
using System.Collections.Generic;

namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata
{
    internal sealed class RelationshipRequestMetadata : ExpansibleEndpointMetadata, IJsonApiRequestMetadata
    {
        public RelationshipRequestMetadata(IDictionary<string, Type> documentTypesByRelationshipName)
            : base(documentTypesByRelationshipName)
        {
        }
    }
}
