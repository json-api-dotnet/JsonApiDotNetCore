using System;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Internal
{
    public class GenericProcessorFactory : IGenericProcessorFactory
    {
        private readonly DbContext _dbContext;
        private readonly IServiceProvider _serviceProvider;

        public GenericProcessorFactory(DbContext dbContext, 
            IServiceProvider serviceProvider)
        {
            _dbContext = dbContext;
            _serviceProvider = serviceProvider;
        }

        public IGenericProcessor GetProcessor(Type type)
        {
            var processorType = typeof(GenericProcessor<>).MakeGenericType(type);
            return (IGenericProcessor)_serviceProvider.GetService(processorType);
        }
    }
}
