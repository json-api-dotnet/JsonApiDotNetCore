using System;
using System.Threading.Tasks;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Operations;
using JsonApiDotNetCore.Serialization;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Services.Operations.Processors
{
    public interface IReplaceOpProcessor<T> : IOpProcessor
        where T : class, IIdentifiable<int>
    { }

    public interface IReplaceOpProcessor<T, TId> : IOpProcessor
        where T : class, IIdentifiable<TId>
    { }

    public class ReplaceOpProcessor<T> : ReplaceOpProcessor<T, int>
        where T : class, IIdentifiable<int>
    {
        public ReplaceOpProcessor(
            IUpdateService<T, int> service,
            IJsonApiDeSerializer deSerializer,
            IDocumentBuilder documentBuilder,
            IContextGraph contextGraph
        ) : base(service, deSerializer, documentBuilder, contextGraph)
        { }
    }

    public class ReplaceOpProcessor<T, TId> : IReplaceOpProcessor<T, TId>
         where T : class, IIdentifiable<TId>
    {
        private readonly IUpdateService<T, TId> _service;
        private readonly IJsonApiDeSerializer _deSerializer;
        private readonly IDocumentBuilder _documentBuilder;
        private readonly IContextGraph _contextGraph;

        public ReplaceOpProcessor(
            IUpdateService<T, TId> service,
            IJsonApiDeSerializer deSerializer,
            IDocumentBuilder documentBuilder,
            IContextGraph contextGraph)
        {
            _service = service;
            _deSerializer = deSerializer;
            _documentBuilder = documentBuilder;
            _contextGraph = contextGraph;
        }

        public async Task<Operation> ProcessAsync(Operation operation)
        {
            Console.WriteLine(JsonConvert.SerializeObject(operation));
            var model = (T)_deSerializer.DocumentToObject(operation.DataObject);

            if (string.IsNullOrWhiteSpace(operation?.DataObject?.Id?.ToString()))
                throw new JsonApiException(400, "The data.id parameter is required for replace operations");

            var id = TypeHelper.ConvertType<TId>(operation.DataObject.Id);
            var result = await _service.UpdateAsync(id, model);

            var operationResult = new Operation
            {
                Op = OperationCode.replace
            };

            operationResult.Data = _documentBuilder.GetData(
                _contextGraph.GetContextEntity(operation.GetResourceTypeName()),
                result);

            return operationResult;
        }
    }
}
