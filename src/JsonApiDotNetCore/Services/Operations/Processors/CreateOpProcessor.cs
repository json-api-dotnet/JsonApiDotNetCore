using System.Threading.Tasks;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Operations;
using JsonApiDotNetCore.Serialization;

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
            IJsonApiDeSerializer deSerializer,
            IDocumentBuilder documentBuilder,
            IResourceGraph resourceGraph
        ) : base(service, deSerializer, documentBuilder, resourceGraph)
        { }
    }

    public class CreateOpProcessor<T, TId> : ICreateOpProcessor<T, TId>
         where T : class, IIdentifiable<TId>
    {
        private readonly ICreateService<T, TId> _service;
        private readonly IJsonApiDeSerializer _deSerializer;
        private readonly IDocumentBuilder _documentBuilder;
        private readonly IResourceGraph _resourceGraph;

        public CreateOpProcessor(
            ICreateService<T, TId> service,
            IJsonApiDeSerializer deSerializer,
            IDocumentBuilder documentBuilder,
            IResourceGraph resourceGraph)
        {
            _service = service;
            _deSerializer = deSerializer;
            _documentBuilder = documentBuilder;
            _resourceGraph = resourceGraph;
        }

        public async Task<Operation> ProcessAsync(Operation operation)
        {
            var model = (T)_deSerializer.DocumentToObject(operation.DataObject);
            var result = await _service.CreateAsync(model);

            var operationResult = new Operation
            {
                Op = OperationCode.add
            };

            operationResult.Data = _documentBuilder.GetData(
                _resourceGraph.GetContextEntity(operation.GetResourceTypeName()),
                result);

            // we need to persist the original request localId so that subsequent operations
            // can locate the result of this operation by its localId
            operationResult.DataObject.LocalId = operation.DataObject.LocalId;

            return operationResult;
        }
    }
}
