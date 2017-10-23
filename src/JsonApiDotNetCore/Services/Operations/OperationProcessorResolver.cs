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
        IOpProcessor LocateReplaceService(Operation operation);
    }

    public class OperationProcessorResolver : IOperationProcessorResolver
    {
        private readonly IGenericProcessorFactory _processorFactory;
        private readonly IJsonApiContext _context;
        private ConcurrentDictionary<string, IOpProcessor> _createOpProcessors = new ConcurrentDictionary<string, IOpProcessor>();
        private ConcurrentDictionary<string, IOpProcessor> _getOpProcessors = new ConcurrentDictionary<string, IOpProcessor>();
        private ConcurrentDictionary<string, IOpProcessor> _replaceOpProcessors = new ConcurrentDictionary<string, IOpProcessor>();

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

        public IOpProcessor LocateReplaceService(Operation operation)
        {
            var resource = operation.GetResourceTypeName();

            if (_replaceOpProcessors.TryGetValue(resource, out IOpProcessor cachedProcessor))
                return cachedProcessor;

            var contextEntity = _context.ContextGraph.GetContextEntity(resource);
            var processor = _processorFactory.GetProcessor<IOpProcessor>(
                typeof(IReplaceOpProcessor<,>), contextEntity.EntityType, contextEntity.IdentityType
            );

            _replaceOpProcessors[resource] = processor;

            return processor;
        }
    }
}
