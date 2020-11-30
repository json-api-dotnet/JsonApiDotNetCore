using System;
using System.Collections.Generic;
using System.Net;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.AtomicOperations
{
    /// <inheritdoc />
    public sealed class LocalIdTracker : ILocalIdTracker
    {
        private readonly IDictionary<string, LocalItem> _idsTracked = new Dictionary<string, LocalItem>();

        /// <inheritdoc />
        public void Declare(string lid, string type)
        {
            AssertIsNotDeclared(lid);

            _idsTracked[lid] = new LocalItem(type);
        }

        private void AssertIsNotDeclared(string lid)
        {
            if (_idsTracked.ContainsKey(lid))
            {
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                {
                    Title = "Another local ID with the same name is already defined at this point.",
                    Detail = $"Another local ID with name '{lid}' is already defined at this point."
                });
            }
        }

        /// <inheritdoc />
        public void Assign(string lid, string type, string id)
        {
            AssertIsDeclared(lid);

            var item = _idsTracked[lid];

            AssertSameResourceType(type, item.Type, lid);

            if (item.IdValue != null)
            {
                throw new InvalidOperationException($"Cannot reassign to existing local ID '{lid}'.");
            }

            item.IdValue = id;
        }

        /// <inheritdoc />
        public string GetValue(string lid, string type)
        {
            AssertIsDeclared(lid);

            var item = _idsTracked[lid];

            AssertSameResourceType(type, item.Type, lid);

            if (item.IdValue == null)
            {
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                {
                    Title = "Local ID cannot be both defined and used within the same operation.",
                    Detail = $"Local ID '{lid}' cannot be both defined and used within the same operation."
                });
            }

            return item.IdValue;
        }

        private void AssertIsDeclared(string lid)
        {
            if (!_idsTracked.ContainsKey(lid))
            {
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                {
                    Title = "Server-generated value for local ID is not available at this point.",
                    Detail = $"Server-generated value for local ID '{lid}' is not available at this point."
                });
            }
        }

        private static void AssertSameResourceType(string currentType, string declaredType, string lid)
        {
            if (declaredType != currentType)
            {
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                {
                    Title = "Type mismatch in local ID usage.",
                    Detail = $"Local ID '{lid}' belongs to resource type '{declaredType}' instead of '{currentType}'."
                });
            }
        }

        private sealed class LocalItem
        {
            public string Type { get; }
            public string IdValue { get; set; }

            public LocalItem(string type)
            {
                Type = type;
            }
        }
    }
}
