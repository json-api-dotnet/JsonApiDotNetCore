using System.Threading.Tasks;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Operations;
using JsonApiDotNetCore.Serialization;

namespace JsonApiDotNetCore.Services.Operations.Processors
{
    public interface IRemoveOpProcessor<T> : IRemoveOpProcessor<T, int>
        where T : class, IIdentifiable<int>
    { }

    public interface IRemoveOpProcessor<T, TId> : IOpProcessor
        where T : class, IIdentifiable<TId>
    { }

    public class RemoveOpProcessor<T> : RemoveOpProcessor<T, int>, IRemoveOpProcessor<T>
        where T : class, IIdentifiable<int>
    {
        public RemoveOpProcessor(
            IDeleteService<T, int> service,
            IJsonApiDeSerializer deSerializer,
            IDocumentBuilder documentBuilder,
            IResourceGraph resourceGraph
        ) : base(service, deSerializer, documentBuilder, resourceGraph)
        { }
    }

    public class RemoveOpProcessor<T, TId> : IRemoveOpProcessor<T, TId>
         where T : class, IIdentifiable<TId>
    {
        private readonly IDeleteService<T, TId> _service;
        private readonly IJsonApiDeSerializer _deSerializer;
        private readonly IDocumentBuilder _documentBuilder;
        private readonly IResourceGraph _resourceGraph;

        public RemoveOpProcessor(
            IDeleteService<T, TId> service,
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
            var stringId = operation.Ref?.Id?.ToString();
            if (string.IsNullOrWhiteSpace(stringId))
                throw new JsonApiException(400, "The ref.id parameter is required for remove operations");

            var id = TypeHelper.ConvertType<TId>(stringId);
            var result = await _service.DeleteAsync(id);

            return null;
        }
    }
}
