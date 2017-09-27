using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Operations;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Services.Operations.Processors
{
    public interface IGetOpProcessor<T> : IOpProcessor
        where T : class, IIdentifiable<int>
    { }

    public interface IGetOpProcessor<T, TId> : IOpProcessor
        where T : class, IIdentifiable<TId>
    { }

    public class GetOpProcessor<T> : GetOpProcessor<T, int>
        where T : class, IIdentifiable<int>
    {
        public GetOpProcessor(
            IGetAllService<T, int> service,
            IJsonApiDeSerializer deSerializer,
            IDocumentBuilder documentBuilder,
            IContextGraph contextGraph,
            IJsonApiContext jsonApiContext
        ) : base(service, deSerializer, documentBuilder, contextGraph, jsonApiContext)
        { }
    }

    public class GetOpProcessor<T, TId> : IGetOpProcessor<T, TId>
         where T : class, IIdentifiable<TId>
    {
        private readonly IGetAllService<T, TId> _service;
        private readonly IJsonApiDeSerializer _deSerializer;
        private readonly IDocumentBuilder _documentBuilder;
        private readonly IContextGraph _contextGraph;
        private readonly IJsonApiContext _jsonApiContext;

        public GetOpProcessor(
            IGetAllService<T, TId> service,
            IJsonApiDeSerializer deSerializer,
            IDocumentBuilder documentBuilder,
            IContextGraph contextGraph,
            IJsonApiContext jsonApiContext)
        {
            _service = service;
            _deSerializer = deSerializer;
            _documentBuilder = documentBuilder;
            _contextGraph = contextGraph;
            _jsonApiContext = jsonApiContext.ApplyContext<T>(this);
        }

        public async Task<Operation> ProcessAsync(Operation operation)
        {
            var result = await _service.GetAsync();

            var operationResult = new Operation
            {
                Op = OperationCode.add
            };

            var operations = new List<DocumentData>();
            foreach (var resource in result)
            {
                var doc = _documentBuilder.GetData(
                _contextGraph.GetContextEntity(operation.GetResourceTypeName()),
                resource);
                operations.Add(doc);
            }

            operationResult.Data = operations;

            return operationResult;
        }
    }
}
