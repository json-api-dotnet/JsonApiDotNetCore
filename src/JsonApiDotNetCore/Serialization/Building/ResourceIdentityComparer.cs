using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Building
{
    internal sealed class ResourceIdentityComparer : IEqualityComparer<IResourceIdentity>
    {
        public static readonly ResourceIdentityComparer Instance = new();

        private ResourceIdentityComparer()
        {
        }

        public bool Equals(IResourceIdentity x, IResourceIdentity y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null || x.GetType() != y.GetType())
            {
                return false;
            }

            return x.Type == y.Type && x.Id == y.Id && x.Lid == y.Lid;
        }

        public int GetHashCode(IResourceIdentity obj)
        {
            return HashCode.Combine(obj.Type, obj.Id, obj.Lid);
        }
    }
}
