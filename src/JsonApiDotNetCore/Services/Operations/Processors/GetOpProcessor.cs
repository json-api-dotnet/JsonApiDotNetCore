using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Operations;
using JsonApiDotNetCore.Serialization;

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
            IGetAllService<T, int> getAll,
            IGetByIdService<T, int> getById,
            IJsonApiDeSerializer deSerializer,
            IDocumentBuilder documentBuilder,
            IContextGraph contextGraph,
            IJsonApiContext jsonApiContext
        ) : base(getAll, getById, deSerializer, documentBuilder, contextGraph, jsonApiContext)
        { }
    }

    public class GetOpProcessor<T, TId> : IGetOpProcessor<T, TId>
         where T : class, IIdentifiable<TId>
    {
        private readonly IGetAllService<T, TId> _getAll;
        private readonly IGetByIdService<T, TId> _getById;
        private readonly IJsonApiDeSerializer _deSerializer;
        private readonly IDocumentBuilder _documentBuilder;
        private readonly IContextGraph _contextGraph;
        private readonly IJsonApiContext _jsonApiContext;

        public GetOpProcessor(
            IGetAllService<T, TId> getAll,
            IGetByIdService<T, TId> getById,
            IJsonApiDeSerializer deSerializer,
            IDocumentBuilder documentBuilder,
            IContextGraph contextGraph,
            IJsonApiContext jsonApiContext)
        {
            _getAll = getAll;
            _getById = getById;
            _deSerializer = deSerializer;
            _documentBuilder = documentBuilder;
            _contextGraph = contextGraph;
            _jsonApiContext = jsonApiContext.ApplyContext<T>(this);
        }

        public async Task<Operation> ProcessAsync(Operation operation)
        {
            var operationResult = new Operation
            {
                Op = OperationCode.get
            };

            operationResult.Data = string.IsNullOrWhiteSpace(operation.Ref.Id?.ToString())
            ? await GetAllAsync(operation)
            : await GetByIdAsync(operation);

            return operationResult;
        }

        private async Task<object> GetAllAsync(Operation operation)
        {
            var result = await _getAll.GetAsync();

            var operations = new List<DocumentData>();
            foreach (var resource in result)
            {
                var doc = _documentBuilder.GetData(
                    _contextGraph.GetContextEntity(operation.GetResourceTypeName()),
                    resource);
                operations.Add(doc);
            }

            return operations;
        }

        private async Task<object> GetByIdAsync(Operation operation)
        {
            var id = TypeHelper.ConvertType<TId>(operation.Ref.Id);
            var result = await _getById.GetAsync(id);

            // this is a bit ugly but we need to bomb the entire transaction if the entity cannot be found
            // in the future it would probably be better to return a result status along with the doc to 
            // avoid throwing exceptions on 4xx errors.
            // consider response type (status, document)
            if (result == null)
                throw new JsonApiException(404, $"Could not find '{operation.Ref.Type}' record with id '{operation.Ref.Id}'");

            var doc = _documentBuilder.GetData(
                _contextGraph.GetContextEntity(operation.GetResourceTypeName()),
                result);

            return doc;
        }
    }
}
