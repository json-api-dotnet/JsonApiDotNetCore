using System;
using System.Collections.Generic;

namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata
{
    internal sealed class SecondaryResponseMetadata : NonPrimaryEndpointMetadata, IJsonApiResponseMetadata
    {
        public SecondaryResponseMetadata(IDictionary<string, Type> documentTypesByRelationshipName)
            : base(documentTypesByRelationshipName)
        {
        }
    }
}
