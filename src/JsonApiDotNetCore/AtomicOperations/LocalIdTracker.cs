using System;
using System.Collections.Generic;

namespace JsonApiDotNetCore.AtomicOperations
{
    /// <inheritdoc />
    public sealed class LocalIdTracker : ILocalIdTracker
    {
        private readonly IDictionary<string, string> _idsTracked = new Dictionary<string, string>();

        /// <inheritdoc />
        public void AssignValue(string lid, string id)
        {
            if (IsAssigned(lid))
            {
                throw new InvalidOperationException($"Cannot reassign to existing local ID '{lid}'.");
            }

            _idsTracked[lid] = id;
        }

        /// <inheritdoc />
        public string GetAssignedValue(string lid)
        {
            if (!IsAssigned(lid))
            {
                throw new InvalidOperationException($"Use of unassigned local ID '{lid}'.");
            }

            return _idsTracked[lid];
        }

        /// <inheritdoc />
        public bool IsAssigned(string lid)
        {
            return _idsTracked.ContainsKey(lid);
        }
    }
}
