using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    public abstract class BaseRelationshipProcessor<TResource, TId>
    {
        private readonly IResourceFactory _resourceFactory;
        protected readonly IJsonApiDeserializer _deserializer;
        private readonly IJsonApiRequest _request;

        protected BaseRelationshipProcessor(IResourceFactory resourceFactory, IJsonApiDeserializer deserializer,
            IJsonApiRequest request)
        {
            _resourceFactory = resourceFactory ?? throw new ArgumentNullException(nameof(resourceFactory));
            _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        protected TId GetPrimaryId(string stringId)
        {
            IIdentifiable primaryResource = _resourceFactory.CreateInstance(_request.PrimaryResource.ResourceType);
            primaryResource.StringId = stringId;

            return (TId) primaryResource.GetTypedId();
        }

        protected HashSet<IIdentifiable> GetSecondaryResourceIds(AtomicOperationObject operation)
        {
            var secondaryResourceIds = new HashSet<IIdentifiable>(IdentifiableComparer.Instance);

            foreach (var resourceObject in operation.ManyData)
            {
                IIdentifiable rightResource = _deserializer.CreateResourceFromObject(resourceObject);
                secondaryResourceIds.Add(rightResource);
            }

            return secondaryResourceIds;
        }
    }
}
