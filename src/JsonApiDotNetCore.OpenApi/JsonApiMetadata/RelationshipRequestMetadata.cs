using System;
using System.Collections.Generic;

namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata
{
    internal sealed class RelationshipRequestMetadata : ExpansibleEndpointMetadata, IJsonApiRequestMetadata
    {
        public override IDictionary<string, Type> ExpansionElements { get; }

        public RelationshipRequestMetadata(IDictionary<string, Type> requestBodyTypeByRelationshipName)
        {
            ArgumentGuard.NotNull(requestBodyTypeByRelationshipName, nameof(requestBodyTypeByRelationshipName));

            ExpansionElements = requestBodyTypeByRelationshipName;
        }
    }
}
