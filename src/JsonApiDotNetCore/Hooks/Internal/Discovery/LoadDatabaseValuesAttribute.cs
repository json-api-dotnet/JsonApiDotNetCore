using System;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Hooks.Internal.Discovery
{
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class LoadDatabaseValuesAttribute : Attribute
    {
        public bool Value { get; }

        public LoadDatabaseValuesAttribute(bool value = true)
        {
            Value = value;
        }
    }
}
