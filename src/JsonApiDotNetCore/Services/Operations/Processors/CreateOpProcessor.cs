using System.Threading.Tasks;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Operations;
using JsonApiDotNetCore.Serialization;

namespace JsonApiDotNetCore.Services.Operations.Processors
{
    public class CreateOpProcessor<T> : CreateOpProcessor<T, int>
        where T : class, IIdentifiable<int>
    {
        public CreateOpProcessor(
            ICreateService<T, int> service,
            IJsonApiDeSerializer deSerializer,
            IDocumentBuilder documentBuilder,
            IContextGraph contextGraph
        ) : base(service, deSerializer, documentBuilder, contextGraph)
        { }
    }

    public class CreateOpProcessor<T, TId> : IOpProcessor<T, TId>
         where T : class, IIdentifiable<TId>
    {
        private readonly ICreateService<T, TId> _service;
        private readonly IJsonApiDeSerializer _deSerializer;
        private readonly IDocumentBuilder _documentBuilder;
        private readonly IContextGraph _contextGraph;

        public CreateOpProcessor(
            ICreateService<T, TId> service,
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
            var model = (T)_deSerializer.DocumentToObject(operation.DataObject);
            var result = await _service.CreateAsync(model);

            var operationResult = new Operation {
                Op = OperationCode.add
            };
            
            operationResult.Data = _documentBuilder.GetData(
                _contextGraph.GetContextEntity(operation.GetResourceTypeName()), 
                result);

            return operationResult;
        }
    }
}
