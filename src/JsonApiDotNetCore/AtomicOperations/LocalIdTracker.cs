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
        private readonly IDictionary<string, LocalIdState> _idsTracked = new Dictionary<string, LocalIdState>();

        /// <inheritdoc />
        public void Reset()
        {
            _idsTracked.Clear();
        }

        /// <inheritdoc />
        public void Declare(string localId, string resourceType)
        {
            if (localId == null) throw new ArgumentNullException(nameof(localId));
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));

            AssertIsNotDeclared(localId);

            _idsTracked[localId] = new LocalIdState(resourceType);
        }

        private void AssertIsNotDeclared(string localId)
        {
            if (_idsTracked.ContainsKey(localId))
            {
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                {
                    Title = "Another local ID with the same name is already defined at this point.",
                    Detail = $"Another local ID with name '{localId}' is already defined at this point."
                });
            }
        }

        /// <inheritdoc />
        public void Assign(string localId, string resourceType, string stringId)
        {
            if (localId == null) throw new ArgumentNullException(nameof(localId));
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));
            if (stringId == null) throw new ArgumentNullException(nameof(stringId));

            AssertIsDeclared(localId);

            var item = _idsTracked[localId];

            AssertSameResourceType(resourceType, item.ResourceType, localId);

            if (item.ServerId != null)
            {
                throw new InvalidOperationException($"Cannot reassign to existing local ID '{localId}'.");
            }

            item.ServerId = stringId;
        }

        /// <inheritdoc />
        public string GetValue(string localId, string resourceType)
        {
            if (localId == null) throw new ArgumentNullException(nameof(localId));
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));

            AssertIsDeclared(localId);

            var item = _idsTracked[localId];

            AssertSameResourceType(resourceType, item.ResourceType, localId);

            if (item.ServerId == null)
            {
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                {
                    Title = "Local ID cannot be both defined and used within the same operation.",
                    Detail = $"Local ID '{localId}' cannot be both defined and used within the same operation."
                });
            }

            return item.ServerId;
        }

        private void AssertIsDeclared(string localId)
        {
            if (!_idsTracked.ContainsKey(localId))
            {
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                {
                    Title = "Server-generated value for local ID is not available at this point.",
                    Detail = $"Server-generated value for local ID '{localId}' is not available at this point."
                });
            }
        }

        private static void AssertSameResourceType(string currentType, string declaredType, string localId)
        {
            if (declaredType != currentType)
            {
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                {
                    Title = "Type mismatch in local ID usage.",
                    Detail = $"Local ID '{localId}' belongs to resource type '{declaredType}' instead of '{currentType}'."
                });
            }
        }

        private sealed class LocalIdState
        {
            public string ResourceType { get; }
            public string ServerId { get; set; }

            public LocalIdState(string resourceType)
            {
                ResourceType = resourceType;
            }
        }
    }
}
