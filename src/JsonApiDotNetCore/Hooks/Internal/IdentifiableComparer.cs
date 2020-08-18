using System.Collections.Generic;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Hooks.Internal
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
