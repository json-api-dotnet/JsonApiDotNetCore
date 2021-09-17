using System;
using System.Collections.Generic;

namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata
{
    internal sealed class RelationshipRequestMetadata : ExpansibleEndpointMetadata, IJsonApiRequestMetadata
    {
        public IDictionary<string, Type> RequestBodyTypeByRelationshipName { get; init; }

        public override IDictionary<string, Type> ExpansionElements => RequestBodyTypeByRelationshipName;
    }
}
