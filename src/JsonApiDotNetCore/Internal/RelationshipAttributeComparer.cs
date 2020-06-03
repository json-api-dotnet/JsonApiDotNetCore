using JsonApiDotNetCore.Models;
using System.Collections.Generic;

namespace JsonApiDotNetCore.Internal
{
    /// <summary>
    /// Compares `RelationshipAttribute` with each other based on PropertyInfo
    /// </summary>
    public sealed class RelationshipAttributeComparer : IEqualityComparer<RelationshipAttribute>
    {
        internal static readonly RelationshipAttributeComparer Instance = new RelationshipAttributeComparer();

        public bool Equals(RelationshipAttribute lh, RelationshipAttribute rh)
        {
            return lh.PropertyInfo.Equals(rh.PropertyInfo);
        }

        public int GetHashCode(RelationshipAttribute obj)
        {
            return obj.PropertyInfo.GetHashCode();
        }
    }
}
