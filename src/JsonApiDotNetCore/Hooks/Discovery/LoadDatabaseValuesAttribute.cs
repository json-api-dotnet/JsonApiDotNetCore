using System;
namespace JsonApiDotNetCore.Hooks
{
    public sealed class LoadDatabaseValues : Attribute
    {
        public readonly bool Value;

        public LoadDatabaseValues(bool mode = true)
        {
            Value = mode;
        }
    }
}
