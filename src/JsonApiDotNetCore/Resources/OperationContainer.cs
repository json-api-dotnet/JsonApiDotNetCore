using System;
using JsonApiDotNetCore.Middleware;

namespace JsonApiDotNetCore.Resources
{
    /// <summary>
    /// Represents a write operation on a JSON:API resource.
    /// </summary>
    public sealed class OperationContainer
    {
        public OperationKind Kind { get; }
        public IIdentifiable Resource { get; }
        public ITargetedFields TargetedFields { get; }
        public IJsonApiRequest Request { get; }

        public OperationContainer(OperationKind kind, IIdentifiable resource, ITargetedFields targetedFields,
            IJsonApiRequest request)
        {
            Kind = kind;
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
            TargetedFields = targetedFields ?? throw new ArgumentNullException(nameof(targetedFields));
            Request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public void SetTransactionId(Guid transactionId)
        {
            ((JsonApiRequest) Request).TransactionId = transactionId;
        }
    }
}
