using JsonApiDotNetCore.Models;
using System.Collections.Generic;

namespace JsonApiDotNetCore.Internal
{
    /// <summary>
    /// Compares `IIdentifiable` with each other based on ID
    /// </summary>
    public sealed class IdentifiableComparer : IEqualityComparer<IIdentifiable>
    {
        internal static readonly IdentifiableComparer Instance = new IdentifiableComparer();

        public bool Equals(IIdentifiable x, IIdentifiable y)
        {
            return x.StringId == y.StringId;
        }
        public int GetHashCode(IIdentifiable obj)
        {
            return obj.StringId.GetHashCode();
        }
    }
}
