using JsonApiDotNetCore.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace JsonApiDotNetCore.Internal
{
    /// <summary>
    /// Compares `IIdentifiable` with each other based on ID
    /// </summary>
    /// <typeparam name="TEntity">The type to compare</typeparam>
    public class IdentifiableComparer : IEqualityComparer<IIdentifiable>
    {
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
