using System.Collections.Generic;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Building
{
    internal sealed class ResourceObjectComparer : IEqualityComparer<ResourceObject>
    {
        public bool Equals(ResourceObject x, ResourceObject y)
        {
            return x.Id.Equals(y.Id) && x.Type.Equals(y.Type);
        }

        public int GetHashCode(ResourceObject ro)
        {
            return ro.GetHashCode();
        }
    }
}
