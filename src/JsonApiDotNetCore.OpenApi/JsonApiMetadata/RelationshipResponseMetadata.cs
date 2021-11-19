using System;
using System.Collections.Generic;

namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata
{
    internal sealed class RelationshipResponseMetadata : ExpansibleEndpointMetadata, IJsonApiResponseMetadata
    {
        public RelationshipResponseMetadata(IDictionary<string, Type> documentTypesByRelationshipName)
            : base(documentTypesByRelationshipName)
        {
        }
    }
}
