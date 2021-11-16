using System;
using System.Collections.Generic;

namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata
{
    internal sealed class RelationshipResponseMetadata : ExpansibleEndpointMetadata, IJsonApiResponseMetadata
    {
        public override IDictionary<string, Type> ExpansionElements { get; }

        public RelationshipResponseMetadata(IDictionary<string, Type> responseTypesByRelationshipName)
        {
            ArgumentGuard.NotNull(responseTypesByRelationshipName, nameof(responseTypesByRelationshipName));

            ExpansionElements = responseTypesByRelationshipName;
        }
    }
}
