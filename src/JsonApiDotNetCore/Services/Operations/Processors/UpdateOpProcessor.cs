using System.Net;
using System.Threading.Tasks;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Operations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization;
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
            IJsonApiDeSerializer deSerializer,
            IDocumentBuilder documentBuilder,
            IResourceGraph resourceGraph
        ) : base(service, deSerializer, documentBuilder, resourceGraph)
        { }
    }

    public class UpdateOpProcessor<T, TId> : IUpdateOpProcessor<T, TId>
         where T : class, IIdentifiable<TId>
    {
        private readonly IUpdateService<T, TId> _service;
        private readonly IJsonApiDeSerializer _deSerializer;
        private readonly IDocumentBuilder _documentBuilder;
        private readonly IResourceGraph _resourceGraph;

        public UpdateOpProcessor(
            IUpdateService<T, TId> service,
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
            if (string.IsNullOrWhiteSpace(operation?.DataObject?.Id?.ToString()))
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                    {
                        Title = "The data.id parameter is required for replace operations"
                    });

            var model = (T)_deSerializer.DocumentToObject(operation.DataObject);

            var result = await _service.UpdateAsync(model.Id, model);
            if (result == null)
                throw new JsonApiException(new Error(HttpStatusCode.NotFound)
                    {
                        Title = $"Could not find an instance of '{operation.DataObject.Type}' with id {operation.DataObject.Id}"
                    });

            var operationResult = new Operation
            {
                Op = OperationCode.update
            };

            operationResult.Data = _documentBuilder.GetData(_resourceGraph.GetContextEntity(operation.GetResourceTypeName()), result);

            return operationResult;
        }
    }
}
