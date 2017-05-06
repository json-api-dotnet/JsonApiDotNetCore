using System;

namespace JsonApiDotNetCore.Internal.Generics
{
    /// <summary>
    /// Used to generate a generic operations processor when the types
    /// are not know until runtime. The typical use case would be for
    /// accessing relationship data.
    /// </summary>
    public interface IGenericProcessorFactory
    {
        IGenericProcessor GetProcessor(Type type);
    }
}
