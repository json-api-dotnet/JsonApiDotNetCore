using System;
using JsonApiDotNetCore.Extensions;

namespace JsonApiDotNetCore.Internal.Query
{
    public class FilterQuery: QueryAttribute
    {
        public FilterQuery(string attribute, string value, string operation)
            :base(attribute)
        {
            Key = attribute.ToProperCase();
            Value = value;
            Operation = operation;
        }
        
        [Obsolete("Key has been replaced by '" + nameof(Attribute) + "'. Members should be located by their public name, not by coercing the provided value to the internal name.")]
        public string Key { get; set; }
        public string Value { get; set; }
        public string Operation { get; set; }

    }
}
