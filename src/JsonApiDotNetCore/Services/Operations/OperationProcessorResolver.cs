using System.Collections.Concurrent;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Models.Operations;
using JsonApiDotNetCore.Services.Operations.Processors;

namespace JsonApiDotNetCore.Services.Operations
{
    public interface IOperationProcessorResolver
    {
        IOpProcessor LocateCreateService(Operation operation);
        IOpProcessor LocateGetService(Operation operation);
        IOpProcessor LocateRemoveService(Operation operation);
        IOpProcessor LocateUpdateService(Operation operation);
    }

    public class OperationProcessorResolver : IOperationProcessorResolver
    {
        private readonly IGenericProcessorFactory _processorFactory;
        private readonly IJsonApiContext _context;

        // processor caches -- since there is some associated cost with creating the processors, we store them in memory
        // to reduce the cost of subsequent requests. in the future, this may be moved into setup code run at startup
        private ConcurrentDictionary<string, IOpProcessor> _createOpProcessors = new ConcurrentDictionary<string, IOpProcessor>();
        private ConcurrentDictionary<string, IOpProcessor> _getOpProcessors = new ConcurrentDictionary<string, IOpProcessor>();
        private ConcurrentDictionary<string, IOpProcessor> _removeOpProcessors = new ConcurrentDictionary<string, IOpProcessor>();
        private ConcurrentDictionary<string, IOpProcessor> _updateOpProcessors = new ConcurrentDictionary<string, IOpProcessor>();

        public OperationProcessorResolver(
            IGenericProcessorFactory processorFactory,
            IJsonApiContext context)
        {
            _processorFactory = processorFactory;
            _context = context;
        }

        // TODO: there may be some optimizations here around the cache such as not caching processors
        // if the request only contains a single op
        public IOpProcessor LocateCreateService(Operation operation)
        {
            var resource = operation.GetResourceTypeName();

            if (_createOpProcessors.TryGetValue(resource, out IOpProcessor cachedProcessor))
                return cachedProcessor;

            var contextEntity = _context.ContextGraph.GetContextEntity(resource);
            var processor = _processorFactory.GetProcessor<IOpProcessor>(
                typeof(ICreateOpProcessor<,>), contextEntity.EntityType, contextEntity.IdentityType
            );

            _createOpProcessors[resource] = processor;

            return processor;
        }

        public IOpProcessor LocateGetService(Operation operation)
        {
            var resource = operation.GetResourceTypeName();

            if (_getOpProcessors.TryGetValue(resource, out IOpProcessor cachedProcessor))
                return cachedProcessor;

            var contextEntity = _context.ContextGraph.GetContextEntity(resource);
            var processor = _processorFactory.GetProcessor<IOpProcessor>(
                typeof(IGetOpProcessor<,>), contextEntity.EntityType, contextEntity.IdentityType
            );

            _getOpProcessors[resource] = processor;

            return processor;
        }

        public IOpProcessor LocateRemoveService(Operation operation)
        {
            var resource = operation.GetResourceTypeName();

            if (_removeOpProcessors.TryGetValue(resource, out IOpProcessor cachedProcessor))
                return cachedProcessor;

            var contextEntity = _context.ContextGraph.GetContextEntity(resource);
            var processor = _processorFactory.GetProcessor<IOpProcessor>(
                typeof(IRemoveOpProcessor<,>), contextEntity.EntityType, contextEntity.IdentityType
            );

            _removeOpProcessors[resource] = processor;

            return processor;
        }

        public IOpProcessor LocateUpdateService(Operation operation)
        {
            var resource = operation.GetResourceTypeName();

            if (_updateOpProcessors.TryGetValue(resource, out IOpProcessor cachedProcessor))
                return cachedProcessor;

            var contextEntity = _context.ContextGraph.GetContextEntity(resource);
            var processor = _processorFactory.GetProcessor<IOpProcessor>(
                typeof(IUpdateOpProcessor<,>), contextEntity.EntityType, contextEntity.IdentityType
            );

            _updateOpProcessors[resource] = processor;

            return processor;
        }
    }
}
