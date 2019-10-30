using System;
namespace JsonApiDotNetCore.Hooks
{
    public class LoaDatabaseValues : Attribute
    {
        public readonly bool value;
        public LoaDatabaseValues(bool mode = true)
        {
            value = mode;
        }
    }
}
