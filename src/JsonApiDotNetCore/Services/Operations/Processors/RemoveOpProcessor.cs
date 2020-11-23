using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Models.Operations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Serialization.Building;
using JsonApiDotNetCore.Serialization.Objects;

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
            IJsonApiDeserializer deserializer,
            IResourceObjectBuilder resourceObjectBuilder,
            IResourceGraph resourceGraph
        ) : base(service, deserializer, resourceObjectBuilder, resourceGraph)
        { }
    }

    public class RemoveOpProcessor<T, TId> : IRemoveOpProcessor<T, TId>
         where T : class, IIdentifiable<TId>
    {
        private readonly IDeleteService<T, TId> _service;
        private readonly IJsonApiDeserializer _deserializer;
        private readonly IResourceObjectBuilder _resourceObjectBuilder;
        private readonly IResourceGraph _resourceGraph;

        public RemoveOpProcessor(
            IDeleteService<T, TId> service,
            IJsonApiDeserializer deserializer,
            IResourceObjectBuilder resourceObjectBuilder,
            IResourceGraph resourceGraph)
        {
            _service = service;
            _deserializer = deserializer;
            _resourceObjectBuilder = resourceObjectBuilder;
            _resourceGraph = resourceGraph;
        }

        public async Task<Operation> ProcessAsync(Operation operation, CancellationToken cancellationToken)
        {
            var stringId = operation.Ref?.Id?.ToString();
            if (string.IsNullOrWhiteSpace(stringId))
            {
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                {
                    Title = "The ref.id element is required for remove operations."
                });
            }

            var id = (TId)TypeHelper.ConvertType(stringId, typeof(TId));
            await _service.DeleteAsync(id, cancellationToken);

            return null;
        }
    }
}
