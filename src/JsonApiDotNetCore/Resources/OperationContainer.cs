using System;
using System.Collections.Generic;
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
            ArgumentGuard.NotNull(resource, nameof(resource));
            ArgumentGuard.NotNull(targetedFields, nameof(targetedFields));
            ArgumentGuard.NotNull(request, nameof(request));

            Kind = kind;
            Resource = resource;
            TargetedFields = targetedFields;
            Request = request;
        }

        public void SetTransactionId(Guid transactionId)
        {
            ((JsonApiRequest) Request).TransactionId = transactionId;
        }

        public OperationContainer WithResource(IIdentifiable resource)
        {
            ArgumentGuard.NotNull(resource, nameof(resource));

            return new OperationContainer(Kind, resource, TargetedFields, Request);
        }

        public ISet<IIdentifiable> GetSecondaryResources()
        {
            var secondaryResources = new HashSet<IIdentifiable>(IdentifiableComparer.Instance);

            foreach (var relationship in TargetedFields.Relationships)
            {
                var rightValue = relationship.GetValue(Resource);
                foreach (var rightResource in TypeHelper.ExtractResources(rightValue))
                {
                    secondaryResources.Add(rightResource);
                }
            }

            return secondaryResources;
        }
    }
}
