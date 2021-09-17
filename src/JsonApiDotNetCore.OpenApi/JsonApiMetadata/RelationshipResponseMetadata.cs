using System;
using System.Collections.Generic;

namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata
{
    internal sealed class RelationshipResponseMetadata : ExpansibleEndpointMetadata, IJsonApiResponseMetadata
    {
        public IDictionary<string, Type> ResponseTypesByRelationshipName { get; init; }

        public override IDictionary<string, Type> ExpansionElements => ResponseTypesByRelationshipName;
    }
}
