using System;

namespace JsonApiDotNetCore.Hooks.Internal.Discovery
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class LoadDatabaseValuesAttribute : Attribute
    {
        public bool Value { get; }

        public LoadDatabaseValuesAttribute(bool mode = true)
        {
            Value = mode;
        }
    }
}
