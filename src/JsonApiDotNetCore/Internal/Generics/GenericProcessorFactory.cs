using System;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Internal
{
    /// <summary>
    /// Used to generate a generic operations processor when the types
    /// are not know until runtime. The typical use case would be for
    /// accessing relationship data.
    /// </summary>
    public static class GenericProcessorFactory
    {
        public static IGenericProcessor GetProcessor(Type type, DbContext dbContext)
        {
            var repositoryType = typeof(GenericProcessor<>).MakeGenericType(type);
            return (IGenericProcessor)Activator.CreateInstance(repositoryType, dbContext);
        }
    }
}
