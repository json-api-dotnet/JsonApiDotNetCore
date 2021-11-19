using System;
using System.Collections.Generic;

namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata
{
    internal abstract class ExpansibleEndpointMetadata
    {
        public IDictionary<string, Type> DocumentTypesByRelationshipName { get; }

        protected ExpansibleEndpointMetadata(IDictionary<string, Type> documentTypesByRelationshipName)
        {
            ArgumentGuard.NotNull(documentTypesByRelationshipName, nameof(documentTypesByRelationshipName));

            DocumentTypesByRelationshipName = documentTypesByRelationshipName;
        }
    }
}
