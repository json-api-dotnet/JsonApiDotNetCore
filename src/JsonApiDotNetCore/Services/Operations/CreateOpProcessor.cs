using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Operations;
using JsonApiDotNetCore.Serialization;

namespace JsonApiDotNetCore.Services.Operations
{
    public class CreateOpProcessor<T, TId> : IOpProcessor<T, TId>
         where T : class, IIdentifiable<TId>
    {
        private readonly ICreateService<T, TId> _service;
        private readonly IJsonApiDeSerializer _deSerializer;
        private readonly IDocumentBuilder _documentBuilder;

        public CreateOpProcessor(
            ICreateService<T, TId> service,
            IJsonApiDeSerializer deSerializer,
            IDocumentBuilder documentBuilder)
        {
            _service = service;
            _deSerializer = deSerializer;
            _documentBuilder = documentBuilder;
        }

        public async Task<Operation> ProcessAsync(Operation operation)
        {
            var model = (T)_deSerializer.DocumentToObject(operation.DataObject);
            var result = await _service.CreateAsync(model);

            var operationResult = new Operation {
                Op = OperationCode.add
            };
            
            operationResult.Data = _documentBuilder.Build(result);

            return operationResult;
        }
    }
}
