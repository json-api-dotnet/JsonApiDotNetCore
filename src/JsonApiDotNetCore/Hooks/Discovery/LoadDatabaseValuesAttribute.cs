using System;
namespace JsonApiDotNetCore.Hooks
{
    public class LoadDatabaseValues : Attribute
    {
        public readonly bool Value;

        public LoadDatabaseValues(bool mode = true)
        {
            Value = mode;
        }
    }
}
