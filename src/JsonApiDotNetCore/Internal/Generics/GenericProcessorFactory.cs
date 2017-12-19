using System;
using JsonApiDotNetCore.Data;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Internal.Generics
{
    public class GenericProcessorFactory : IGenericProcessorFactory
    {
        private readonly DbContext _dbContext;
        private readonly IServiceProvider _serviceProvider;

        public GenericProcessorFactory(
            IDbContextResolver dbContextResolver, 
            IServiceProvider serviceProvider)
        {
            _dbContext = dbContextResolver.GetContext();
            _serviceProvider = serviceProvider;
        }

        public IGenericProcessor GetProcessor(Type type)
        {
            var processorType = typeof(GenericProcessor<>).MakeGenericType(type);
            return (IGenericProcessor)_serviceProvider.GetService(processorType);
        }
    }
}
