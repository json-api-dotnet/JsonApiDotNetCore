using System.Threading.Tasks;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Operations;
using JsonApiDotNetCore.Serialization.Deserializer;

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
            IOperationsDeserializer deserializer,
            IBaseDocumentBuilder documentBuilder,
            IResourceGraph resourceGraph
        ) : base(service, deserializer, documentBuilder, resourceGraph)
        { }
    }

    public class UpdateOpProcessor<T, TId> : IUpdateOpProcessor<T, TId>
         where T : class, IIdentifiable<TId>
    {
        private readonly IUpdateService<T, TId> _service;
        private readonly IOperationsDeserializer _deserializer;
        private readonly IBaseDocumentBuilder _documentBuilder;
        private readonly IResourceGraph _resourceGraph;

        public UpdateOpProcessor(
            IUpdateService<T, TId> service,
            IOperationsDeserializer deserializer,
            IBaseDocumentBuilder documentBuilder,
            IResourceGraph resourceGraph)
        {
            _service = service;
            _deserializer = deserializer;
            _documentBuilder = documentBuilder;
            _resourceGraph = resourceGraph;
        }

        public async Task<Operation> ProcessAsync(Operation operation)
        {
            if (string.IsNullOrWhiteSpace(operation?.DataObject?.Id?.ToString()))
                throw new JsonApiException(400, "The data.id parameter is required for replace operations");

            //var model = (T)_deserializer.DocumentToObject(operation.DataObject);
            T model = null; // TODO

            var result = await _service.UpdateAsync(model.Id, model);
            if (result == null)
                throw new JsonApiException(404, $"Could not find an instance of '{operation.DataObject.Type}' with id {operation.DataObject.Id}");

            var operationResult = new Operation
            {
                Op = OperationCode.update
            };

            operationResult.Data = _documentBuilder.GetData(_resourceGraph.GetContextEntity(operation.GetResourceTypeName()), result);

            return operationResult;
        }
    }
}
