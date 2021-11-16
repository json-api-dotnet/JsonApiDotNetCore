using System;
using System.Collections.Generic;

namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata
{
    internal sealed class SecondaryResponseMetadata : ExpansibleEndpointMetadata, IJsonApiResponseMetadata
    {
        public override IDictionary<string, Type> ExpansionElements { get; }

        public SecondaryResponseMetadata(IDictionary<string, Type> responseTypesByRelationshipName)
        {
            ArgumentGuard.NotNull(responseTypesByRelationshipName, nameof(responseTypesByRelationshipName));

            ExpansionElements = responseTypesByRelationshipName;
        }
    }
}
