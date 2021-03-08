using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Middleware
{
    /// <inheritdoc />
    [PublicAPI]
    public sealed class JsonApiRequest : IJsonApiRequest
    {
        /// <inheritdoc />
        public EndpointKind Kind { get; set; }

        /// <inheritdoc />
        public string BasePath { get; set; }

        /// <inheritdoc />
        public string PrimaryId { get; set; }

        /// <inheritdoc />
        public ResourceContext PrimaryResource { get; set; }

        /// <inheritdoc />
        public ResourceContext SecondaryResource { get; set; }

        /// <inheritdoc />
        public RelationshipAttribute Relationship { get; set; }

        /// <inheritdoc />
        public bool IsCollection { get; set; }

        /// <inheritdoc />
        public bool IsReadOnly { get; set; }

        /// <inheritdoc />
        public OperationKind? OperationKind { get; set; }

        /// <inheritdoc />
        public Guid? TransactionId { get; set; }

        /// <inheritdoc />
        public void CopyFrom(IJsonApiRequest other)
        {
            ArgumentGuard.NotNull(other, nameof(other));

            Kind = other.Kind;
            BasePath = other.BasePath;
            PrimaryId = other.PrimaryId;
            PrimaryResource = other.PrimaryResource;
            SecondaryResource = other.SecondaryResource;
            Relationship = other.Relationship;
            IsCollection = other.IsCollection;
            IsReadOnly = other.IsReadOnly;
            OperationKind = other.OperationKind;
            TransactionId = other.TransactionId;
        }
    }
}
