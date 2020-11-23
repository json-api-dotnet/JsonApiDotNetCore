using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Models.Operations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Building;

namespace JsonApiDotNetCore.Services.Operations.Processors
{
    public interface ICreateOpProcessor<T> : ICreateOpProcessor<T, int>
        where T : class, IIdentifiable<int>
    { }

    public interface ICreateOpProcessor<T, TId> : IOpProcessor
        where T : class, IIdentifiable<TId>
    { }

    public class CreateOpProcessor<T>
        : CreateOpProcessor<T, int>, ICreateOpProcessor<T>
        where T : class, IIdentifiable<int>
    {
        public CreateOpProcessor(
            ICreateService<T, int> service,
            IJsonApiDeserializer deserializer,
            IResourceObjectBuilder resourceObjectBuilder,
            IResourceGraph resourceGraph
        ) : base(service, deserializer, resourceObjectBuilder, resourceGraph)
        { }
    }

    public class CreateOpProcessor<T, TId> : ICreateOpProcessor<T, TId>
         where T : class, IIdentifiable<TId>
    {
        private readonly ICreateService<T, TId> _service;
        private readonly IJsonApiDeserializer _deserializer;
        private readonly IResourceObjectBuilder _resourceObjectBuilder;
        private readonly IResourceGraph _resourceGraph;

        public CreateOpProcessor(
            ICreateService<T, TId> service,
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
            var model = (T)_deserializer.CreateResourceFromObject(operation.DataObject);
            var result = await _service.CreateAsync(model, cancellationToken);

            var operationResult = new Operation
            {
                Op = OperationCode.add
            };

            ResourceContext resourceContext = _resourceGraph.GetResourceContext(operation.DataObject.Type);

            operationResult.Data = _resourceObjectBuilder.Build(result, resourceContext.Attributes, resourceContext.Relationships);

            // we need to persist the original request localId so that subsequent operations
            // can locate the result of this operation by its localId
            operationResult.DataObject.LocalId = operation.DataObject.LocalId;

            return operationResult;
        }
    }
}
