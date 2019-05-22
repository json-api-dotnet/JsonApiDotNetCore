using System;
namespace JsonApiDotNetCore.Hooks
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
