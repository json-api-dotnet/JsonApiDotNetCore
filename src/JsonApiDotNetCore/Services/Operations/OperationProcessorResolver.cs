using System.Collections.Concurrent;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Models.Operations;

namespace JsonApiDotNetCore.Services.Operations
{
    public interface IOperationProcessorResolver
    {
        IOpProcessor LocateCreateService(Operation operation);
    }

    public class OperationProcessorResolver : IOperationProcessorResolver
    {
        private readonly IGenericProcessorFactory _processorFactory;
        private readonly IJsonApiContext _context;
        private ConcurrentDictionary<string, IOpProcessor> _cachedProcessors = new ConcurrentDictionary<string, IOpProcessor>();

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

            if (_cachedProcessors.TryGetValue(resource, out IOpProcessor cachedProcessor))
                return cachedProcessor;

            var contextEntity = _context.ContextGraph.GetContextEntity();
            var processor = _processorFactory.GetProcessor<IOpProcessor>(contextEntity.EntityType, contextEntity.IdentityType);
            
            _cachedProcessors[resource] = processor;
            
            return processor;
        }
    }
}
