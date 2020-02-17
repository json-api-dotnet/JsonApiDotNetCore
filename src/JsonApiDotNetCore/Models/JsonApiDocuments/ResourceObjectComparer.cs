using System.Collections.Generic;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Builders
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
