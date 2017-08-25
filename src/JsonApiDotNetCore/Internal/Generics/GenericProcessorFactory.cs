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

        public TInterface GetProcessor<TInterface>(Type[] types)
        {
            var processorType = typeof(GenericProcessor<>).MakeGenericType(types);
            return (TInterface)_serviceProvider.GetService(processorType);
        }
    }
}
