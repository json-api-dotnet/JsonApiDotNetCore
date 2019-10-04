using System;

namespace JsonApiDotNetCore.Query
{
    public class AttributeBehaviourService : IAttributeBehaviourService
    {
        public bool? OmitNullValuedAttributes { get; set; }
        public bool? OmitDefaultValuedAttributes { get; set; }

        public string Name => throw new NotImplementedException();

        public void Parse(string value)
        {
            throw new NotImplementedException();
        }
    }
}
