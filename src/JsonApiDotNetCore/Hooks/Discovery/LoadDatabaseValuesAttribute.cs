using System;
namespace JsonApiDotNetCore.Hooks
{
    public class LoadDatabaseValues : Attribute
    {
        public readonly bool value;
        public LoadDatabaseValues(bool mode = true)
        {
            value = mode;
        }
    }
}
