using System;
using System.Collections.Generic;

namespace JsonApiDotNetCore.OpenApi.JsonApiMetadata
{
    internal abstract class ExpansibleEndpointMetadata
    {
        public abstract IDictionary<string, Type> ExpansionElements { get; }
    }
}
