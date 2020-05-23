using JsonApiDotNetCore.Models;
using System.Collections.Generic;

namespace JsonApiDotNetCore.Internal
{
    /// <summary>
    /// Compares `AttrAttribute` with each other based on PropertyInfo
    /// </summary>
    public sealed class AttrAttributeComparer: IEqualityComparer<AttrAttribute>
    {
        internal static readonly AttrAttributeComparer Instance = new AttrAttributeComparer();

        public bool Equals(AttrAttribute lh, AttrAttribute rh)
        {
            return lh.PropertyInfo.Equals(rh.PropertyInfo);
        }
        public int GetHashCode(AttrAttribute obj)
        {
            return obj.PropertyInfo.GetHashCode();
        }
    }
}
