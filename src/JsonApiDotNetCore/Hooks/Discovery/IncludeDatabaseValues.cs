using System;
namespace JsonApiDotNetCore.Hooks.Discovery
{
    public class IncludeDatabaseValues : Attribute
    {
        public readonly bool value;
        public IncludeDatabaseValues(bool mode = true)
        {
            value = mode;
        }
    }
}
