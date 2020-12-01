using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Building;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    /// <summary>
    /// Processes a single operation to update the attributes and/or relationships of an existing resource.
    /// Only the values of sent attributes are replaced. And only the values of sent relationships are replaced.
    /// </summary>
    /// <typeparam name="TResource">The resource type.</typeparam>
    /// <typeparam name="TId">The resource identifier type.</typeparam>
    public class UpdateProcessor<TResource, TId> : BaseAtomicOperationProcessor, IUpdateProcessor<TResource, TId>
        where TResource : class, IIdentifiable<TId>
    {
        private readonly IUpdateService<TResource, TId> _service;
        private readonly IJsonApiDeserializer _deserializer;
        private readonly IResourceObjectBuilder _resourceObjectBuilder;
        private readonly IResourceContextProvider _resourceContextProvider;

        public UpdateProcessor(IUpdateService<TResource, TId> service, IJsonApiOptions options,
            IObjectModelValidator validator, IJsonApiDeserializer deserializer,
            IResourceObjectBuilder resourceObjectBuilder, IResourceContextProvider resourceContextProvider)
            : base(options, validator)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
            _resourceObjectBuilder = resourceObjectBuilder ?? throw new ArgumentNullException(nameof(resourceObjectBuilder));
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
        }

        /// <inheritdoc />
        public async Task<AtomicResultObject> ProcessAsync(AtomicOperationObject operation, CancellationToken cancellationToken)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));

            if (operation.SingleData == null)
            {
                throw new InvalidOperationException("TODO: Expected data element. Can we ever get here?");
            }

            if (operation.SingleData.Id == null)
            {
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                {
                    Title = "The data.id element is required for replace operations."
                });
            }

            var model = (TResource) _deserializer.CreateResourceFromObject(operation.SingleData);
            ValidateModelState(model);

            var result = await _service.UpdateAsync(model.Id, model, cancellationToken);

            ResourceObject data = null;

            if (result != null)
            {
                ResourceContext resourceContext = _resourceContextProvider.GetResourceContext(operation.SingleData.Type);
                data = _resourceObjectBuilder.Build(result, resourceContext.Attributes, resourceContext.Relationships);
            }

            return new AtomicResultObject
            {
                Data = data
            };
        }
    }

    /// <summary>
    /// Processes a single operation to update the attributes and/or relationships of an existing resource.
    /// Only the values of sent attributes are replaced. And only the values of sent relationships are replaced.
    /// </summary>
    /// <typeparam name="TResource">The resource type.</typeparam>
    public class UpdateProcessor<TResource>
        : UpdateProcessor<TResource, int>, IUpdateProcessor<TResource>
        where TResource : class, IIdentifiable<int>
    {
        public UpdateProcessor(IUpdateService<TResource> service, IJsonApiOptions options,
            IObjectModelValidator validator, IJsonApiDeserializer deserializer,
            IResourceObjectBuilder resourceObjectBuilder, IResourceContextProvider resourceContextProvider)
            : base(service, options, validator, deserializer, resourceObjectBuilder, resourceContextProvider)
        {
        }
    }
}
