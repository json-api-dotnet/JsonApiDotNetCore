using JsonApiDotNetCore.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace JsonApiDotNetCore.Hooks.Comparer
{
    /// <summary>
    /// Compares `IIdentifiable` with each other based on ID
    /// </summary>
    /// <typeparam name="TEntity">The type to compare</typeparam>
    public class IdentifiableComparer<TEntity> : IEqualityComparer<TEntity> where TEntity : IIdentifiable<int>
    {
        public bool Equals(TEntity x, TEntity y)
        {
            return x.Id == y.Id;
        }
        public int GetHashCode(TEntity obj)
        {
            return obj.Id;
        }
    }
}
