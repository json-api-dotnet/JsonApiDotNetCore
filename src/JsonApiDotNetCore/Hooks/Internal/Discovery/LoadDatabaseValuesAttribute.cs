using System;

namespace JsonApiDotNetCore.Hooks.Internal.Discovery
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class LoadDatabaseValuesAttribute : Attribute
    {
        public readonly bool Value;

        public LoadDatabaseValuesAttribute(bool mode = true)
        {
            Value = mode;
        }
    }
}
