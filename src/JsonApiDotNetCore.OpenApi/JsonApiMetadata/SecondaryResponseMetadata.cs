using System;
using System.Collections.Generic;

namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata
{
    internal sealed class SecondaryResponseMetadata : ExpansibleEndpointMetadata, IJsonApiResponseMetadata
    {
        public SecondaryResponseMetadata(IDictionary<string, Type> documentTypesByRelationshipName)
            : base(documentTypesByRelationshipName)
        {
        }
    }
}
