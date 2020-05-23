using JsonApiDotNetCore.Models;
using System.Collections.Generic;

namespace JsonApiDotNetCore.Internal
{
    /// <summary>
    /// Compares `EagerLoadAttribute` with each other based on PropertyInfo
    /// </summary>
    public sealed class EagerLoadAttributeComparer: IEqualityComparer<EagerLoadAttribute>
    {
        internal static readonly EagerLoadAttributeComparer Instance = new EagerLoadAttributeComparer();

        public bool Equals(EagerLoadAttribute lh, EagerLoadAttribute rh)
        {
            return lh.Property.Equals(rh.Property);
        }
        public int GetHashCode(EagerLoadAttribute obj)
        {
            return obj.Property.GetHashCode();
        }
    }
}
