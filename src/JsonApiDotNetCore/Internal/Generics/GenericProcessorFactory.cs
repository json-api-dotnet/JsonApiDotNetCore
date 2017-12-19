using System;

namespace JsonApiDotNetCore.Internal.Generics
{
    public class GenericProcessorFactory : IGenericProcessorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public GenericProcessorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IGenericProcessor GetProcessor(Type type)
        {
            var processorType = typeof(GenericProcessor<>).MakeGenericType(type);
            return (IGenericProcessor)_serviceProvider.GetService(processorType);
        }
    }
}
