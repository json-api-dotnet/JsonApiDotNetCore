using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Errors;

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
            ArgumentGuard.NotNullNorEmpty(localId, nameof(localId));
            ArgumentGuard.NotNullNorEmpty(resourceType, nameof(resourceType));

            AssertIsNotDeclared(localId);

            _idsTracked[localId] = new LocalIdState(resourceType);
        }

        private void AssertIsNotDeclared(string localId)
        {
            if (_idsTracked.ContainsKey(localId))
            {
                throw new DuplicateLocalIdValueException(localId);
            }
        }

        /// <inheritdoc />
        public void Assign(string localId, string resourceType, string stringId)
        {
            ArgumentGuard.NotNullNorEmpty(localId, nameof(localId));
            ArgumentGuard.NotNullNorEmpty(resourceType, nameof(resourceType));
            ArgumentGuard.NotNullNorEmpty(stringId, nameof(stringId));

            AssertIsDeclared(localId);

            LocalIdState item = _idsTracked[localId];

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
            ArgumentGuard.NotNullNorEmpty(localId, nameof(localId));
            ArgumentGuard.NotNullNorEmpty(resourceType, nameof(resourceType));

            AssertIsDeclared(localId);

            LocalIdState item = _idsTracked[localId];

            AssertSameResourceType(resourceType, item.ResourceType, localId);

            if (item.ServerId == null)
            {
                throw new LocalIdSingleOperationException(localId);
            }

            return item.ServerId;
        }

        private void AssertIsDeclared(string localId)
        {
            if (!_idsTracked.ContainsKey(localId))
            {
                throw new UnknownLocalIdValueException(localId);
            }
        }

        private static void AssertSameResourceType(string currentType, string declaredType, string localId)
        {
            if (declaredType != currentType)
            {
                throw new IncompatibleLocalIdTypeException(localId, declaredType, currentType);
            }
        }

        private sealed class LocalIdState
        {
            public string ResourceType { get; }
            public string? ServerId { get; set; }

            public LocalIdState(string resourceType)
            {
                ResourceType = resourceType;
            }
        }
    }
}
