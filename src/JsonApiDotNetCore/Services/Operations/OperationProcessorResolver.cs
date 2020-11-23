using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Models.Operations;
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
        /// Locates the correct <see cref="GetOpProcessor{T, TId}"/>
        /// </summary>
        IOpProcessor LocateGetService(Operation operation);

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
        private readonly IGenericProcessorFactory _processorFactory;
        private readonly IContextEntityProvider _provider;

        /// <nodoc />
        public OperationProcessorResolver(
            IGenericProcessorFactory processorFactory,
            IContextEntityProvider provider)
        {
            _processorFactory = processorFactory;
            _provider = provider;
        }

        /// <inheritdoc />
        public IOpProcessor LocateCreateService(Operation operation)
        {
            var resource = operation.GetResourceTypeName();

            var contextEntity = GetResourceMetadata(resource);

            var processor = _processorFactory.GetProcessor<IOpProcessor>(
                typeof(ICreateOpProcessor<,>), contextEntity.EntityType, contextEntity.IdentityType
            );

            return processor;
        }

        /// <inheritdoc />
        public IOpProcessor LocateGetService(Operation operation)
        {
            var resource = operation.GetResourceTypeName();

            var contextEntity = GetResourceMetadata(resource);

            var processor = _processorFactory.GetProcessor<IOpProcessor>(
                typeof(IGetOpProcessor<,>), contextEntity.EntityType, contextEntity.IdentityType
            );

            return processor;
        }

        /// <inheritdoc />
        public IOpProcessor LocateRemoveService(Operation operation)
        {
            var resource = operation.GetResourceTypeName();

            var contextEntity = GetResourceMetadata(resource);

            var processor = _processorFactory.GetProcessor<IOpProcessor>(
                typeof(IRemoveOpProcessor<,>), contextEntity.EntityType, contextEntity.IdentityType
            );

            return processor;
        }

        /// <inheritdoc />
        public IOpProcessor LocateUpdateService(Operation operation)
        {
            var resource = operation.GetResourceTypeName();

            var contextEntity = GetResourceMetadata(resource);

            var processor = _processorFactory.GetProcessor<IOpProcessor>(
                typeof(IUpdateOpProcessor<,>), contextEntity.EntityType, contextEntity.IdentityType
            );

            return processor;
        }

        private ContextEntity GetResourceMetadata(string resourceName)
        {
            var contextEntity = _provider.GetContextEntity(resourceName);
            if(contextEntity == null)
                throw new JsonApiException(400, $"This API does not expose a resource of type '{resourceName}'.");

            return contextEntity;
        }
    }
}
