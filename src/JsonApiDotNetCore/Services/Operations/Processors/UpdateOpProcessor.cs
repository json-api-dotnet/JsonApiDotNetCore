using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Models.Operations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Building;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Services.Operations.Processors
{
    public interface IUpdateOpProcessor<T> : IUpdateOpProcessor<T, int>
        where T : class, IIdentifiable<int>
    { }

    public interface IUpdateOpProcessor<T, TId> : IOpProcessor
        where T : class, IIdentifiable<TId>
    { }

    public class UpdateOpProcessor<T> : UpdateOpProcessor<T, int>, IUpdateOpProcessor<T>
        where T : class, IIdentifiable<int>
    {
        public UpdateOpProcessor(
            IUpdateService<T, int> service,
            IJsonApiDeserializer deserializer,
            IResourceObjectBuilder resourceObjectBuilder,
            IResourceGraph resourceGraph
        ) : base(service, deserializer, resourceObjectBuilder, resourceGraph)
        { }
    }

    public class UpdateOpProcessor<T, TId> : IUpdateOpProcessor<T, TId>
         where T : class, IIdentifiable<TId>
    {
        private readonly IUpdateService<T, TId> _service;
        private readonly IJsonApiDeserializer _deserializer;
        private readonly IResourceObjectBuilder _resourceObjectBuilder;
        private readonly IResourceGraph _resourceGraph;

        public UpdateOpProcessor(
            IUpdateService<T, TId> service,
            IJsonApiDeserializer deserializer,
            IResourceObjectBuilder resourceObjectBuilder,
            IResourceGraph resourceGraph)
        {
            _service = service;
            _deserializer = deserializer;
            _resourceObjectBuilder = resourceObjectBuilder;
            _resourceGraph = resourceGraph;
        }

        public async Task<Operation> ProcessAsync(Operation operation, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(operation?.DataObject?.Id))
            {
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                {
                    Title = "The data.id element is required for replace operations."
                });
            }

            var model = (T)_deserializer.CreateResourceFromObject(operation.DataObject);

            var result = await _service.UpdateAsync(model.Id, model, cancellationToken);

            ResourceObject data = null;

            if (result != null)
            {
                ResourceContext resourceContext = _resourceGraph.GetResourceContext(operation.DataObject.Type);
                data = _resourceObjectBuilder.Build(result, resourceContext.Attributes, resourceContext.Relationships);
            }

            return new Operation
            {
                Op = OperationCode.update,
                Data = data
            };
        }
    }
}
