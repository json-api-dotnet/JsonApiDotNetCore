using System.Net;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Models.Operations;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Services.Operations.Processors;

namespace JsonApiDotNetCore.Services.Operations
{
    /// <summary>
    /// Used to resolve <see cref="IOpProcessor"/> at runtime based on the required operation
    /// </summary>
    public interface IOperationProcessorResolver
    {
        /// <summary>
        /// Locates the correct <see cref="CreateOpProcessor{T, TId}"/>
        /// </summary>
        IOpProcessor LocateCreateService(Operation operation);

        /// <summary>
        /// Locates the correct <see cref="RemoveOpProcessor{T, TId}"/>
        /// </summary>
        IOpProcessor LocateRemoveService(Operation operation);

        /// <summary>
        /// Locates the correct <see cref="UpdateOpProcessor{T, TId}"/>
        /// </summary>
        IOpProcessor LocateUpdateService(Operation operation);
    }

    /// <inheritdoc />
    public class OperationProcessorResolver : IOperationProcessorResolver
    {
        private readonly IGenericServiceFactory _genericServiceFactory;
        private readonly IResourceContextProvider _resourceContextProvider;

        /// <nodoc />
        public OperationProcessorResolver(
            IGenericServiceFactory genericServiceFactory,
            IResourceContextProvider resourceContextProvider)
        {
            _genericServiceFactory = genericServiceFactory;
            _resourceContextProvider = resourceContextProvider;
        }

        /// <inheritdoc />
        public IOpProcessor LocateCreateService(Operation operation)
        {
            var resource = operation.GetResourceTypeName();

            var contextEntity = GetResourceMetadata(resource);

            var processor = _genericServiceFactory.Get<IOpProcessor>(
                typeof(ICreateOpProcessor<,>), contextEntity.ResourceType, contextEntity.IdentityType
            );

            return processor;
        }

        /// <inheritdoc />
        public IOpProcessor LocateRemoveService(Operation operation)
        {
            var resource = operation.GetResourceTypeName();

            var contextEntity = GetResourceMetadata(resource);

            var processor = _genericServiceFactory.Get<IOpProcessor>(
                typeof(IRemoveOpProcessor<,>), contextEntity.ResourceType, contextEntity.IdentityType
            );

            return processor;
        }

        /// <inheritdoc />
        public IOpProcessor LocateUpdateService(Operation operation)
        {
            var resource = operation.GetResourceTypeName();

            var contextEntity = GetResourceMetadata(resource);

            var processor = _genericServiceFactory.Get<IOpProcessor>(
                typeof(IUpdateOpProcessor<,>), contextEntity.ResourceType, contextEntity.IdentityType
            );

            return processor;
        }

        private ResourceContext GetResourceMetadata(string resourceName)
        {
            var contextEntity = _resourceContextProvider.GetResourceContext(resourceName);
            if (contextEntity == null)
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                {
                    Title = "Unsupported resource type.",
                    Detail = $"This API does not expose a resource of type '{resourceName}'."
                });

            return contextEntity;
        }
    }
}
