using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    /// <summary>
    /// Handles all "<see cref="OperationCode.get"/>" operations
    /// </summary>
    /// <typeparam name="T">The resource type</typeparam>
    public interface IGetOpProcessor<T> : IGetOpProcessor<T, int>
        where T : class, IIdentifiable<int>
    { }

    /// <summary>
    ///  Handles all "<see cref="OperationCode.get"/>" operations
    /// </summary>
    /// <typeparam name="T">The resource type</typeparam>
    /// <typeparam name="TId">The resource identifier type</typeparam>
    public interface IGetOpProcessor<T, TId> : IOpProcessor
        where T : class, IIdentifiable<TId>
    { }

    /// <inheritdoc />
    public class GetOpProcessor<T> : GetOpProcessor<T, int>, IGetOpProcessor<T>
        where T : class, IIdentifiable<int>
    {
        /// <inheritdoc />
        public GetOpProcessor(
            IGetAllService<T, int> getAll,
            IGetByIdService<T, int> getById,
            IGetRelationshipService<T, int> getRelationship,
            IJsonApiDeSerializer deSerializer,
            IDocumentBuilder documentBuilder,
            IResourceGraph resourceGraph,
            IJsonApiContext jsonApiContext
        ) : base(getAll, getById, getRelationship, deSerializer, documentBuilder, resourceGraph, jsonApiContext)
        { }
    }

    /// <inheritdoc />
    public class GetOpProcessor<T, TId> : IGetOpProcessor<T, TId>
         where T : class, IIdentifiable<TId>
    {
        private readonly IGetAllService<T, TId> _getAll;
        private readonly IGetByIdService<T, TId> _getById;
        private readonly IGetRelationshipService<T, TId> _getRelationship;
        private readonly IJsonApiDeSerializer _deSerializer;
        private readonly IDocumentBuilder _documentBuilder;
        private readonly IResourceGraph _resourceGraph;
        private readonly IJsonApiContext _jsonApiContext;

        /// <inheritdoc />
        public GetOpProcessor(
            IGetAllService<T, TId> getAll,
            IGetByIdService<T, TId> getById,
            IGetRelationshipService<T, TId> getRelationship,
            IJsonApiDeSerializer deSerializer,
            IDocumentBuilder documentBuilder,
            IResourceGraph resourceGraph,
            IJsonApiContext jsonApiContext)
        {
            _getAll = getAll;
            _getById = getById;
            _getRelationship = getRelationship;
            _deSerializer = deSerializer;
            _documentBuilder = documentBuilder;
            _resourceGraph = resourceGraph;
            _jsonApiContext = jsonApiContext.ApplyContext<T>(this);
        }

        /// <inheritdoc />
        public async Task<Operation> ProcessAsync(Operation operation)
        {
            var operationResult = new Operation
            {
                Op = OperationCode.get
            };

            operationResult.Data = string.IsNullOrWhiteSpace(operation.Ref.Id)
                ? await GetAllAsync(operation)
                : string.IsNullOrWhiteSpace(operation.Ref.Relationship)
                    ? await GetByIdAsync(operation)
                    : await GetRelationshipAsync(operation);

            return operationResult;
        }

        private async Task<object> GetAllAsync(Operation operation)
        {
            var result = await _getAll.GetAsync();

            var operations = new List<ResourceObject>();
            foreach (var resource in result)
            {
                var doc = _documentBuilder.GetData(
                    _resourceGraph.GetContextEntity(operation.GetResourceTypeName()),
                    resource);
                operations.Add(doc);
            }

            return operations;
        }

        private async Task<object> GetByIdAsync(Operation operation)
        {
            var id = GetReferenceId(operation);
            var result = await _getById.GetAsync(id);

            // this is a bit ugly but we need to bomb the entire transaction if the entity cannot be found
            // in the future it would probably be better to return a result status along with the doc to
            // avoid throwing exceptions on 4xx errors.
            // consider response type (status, document)
            if (result == null)
                throw new JsonApiException(new Error(HttpStatusCode.NotFound)
                    {
                        Title = $"Could not find '{operation.Ref.Type}' record with id '{operation.Ref.Id}'"
                    });

            var doc = _documentBuilder.GetData(
                _resourceGraph.GetContextEntity(operation.GetResourceTypeName()),
                result);

            return doc;
        }

        private async Task<object> GetRelationshipAsync(Operation operation)
        {
            var id = GetReferenceId(operation);
            var result = await _getRelationship.GetRelationshipAsync(id, operation.Ref.Relationship);

            // TODO: need a better way to get the ContextEntity from a relationship name
            // when no generic parameter is available
            var relationshipType = _resourceGraph.GetContextEntity(operation.GetResourceTypeName())
                .Relationships.Single(r => r.Is(operation.Ref.Relationship)).Type;

            var relatedContextEntity = _jsonApiContext.ResourceGraph.GetContextEntity(relationshipType);

            if (result == null)
                return null;

            if (result is IIdentifiable singleResource)
                return GetData(relatedContextEntity, singleResource);

            if (result is IEnumerable multipleResults)
                return GetData(relatedContextEntity, multipleResults);

            throw new JsonApiException(new Error(HttpStatusCode.InternalServerError)
            {
                Title =  $"An unexpected type was returned from '{_getRelationship.GetType()}.{nameof(IGetRelationshipService<T, TId>.GetRelationshipAsync)}'.",
                Detail = $"Type '{result.GetType()} does not implement {nameof(IIdentifiable)} nor {nameof(IEnumerable<IIdentifiable>)}'"
            });
        }

        private ResourceObject GetData(ContextEntity contextEntity, IIdentifiable singleResource)
        {
            return _documentBuilder.GetData(contextEntity, singleResource);
        }

        private List<ResourceObject> GetData(ContextEntity contextEntity, IEnumerable multipleResults)
        {
            var resources = new List<ResourceObject>();
            foreach (var singleResult in multipleResults)
            {
                if (singleResult is IIdentifiable resource)
                    resources.Add(_documentBuilder.GetData(contextEntity, resource));
            }

            return resources;
        }

        private TId GetReferenceId(Operation operation) => TypeHelper.ConvertType<TId>(operation.Ref.Id);
    }
}
