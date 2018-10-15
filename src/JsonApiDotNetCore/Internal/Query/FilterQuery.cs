using System;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Internal.Query
{
    public class FilterQuery : BaseQuery
    {
        [Obsolete("Use constructor with FilterOperations operationType paremeter. Filter operation should be provided " +
            "as enum type, not by string.")]
        public FilterQuery(string attribute, string value, string operation)
            :base(attribute)
        {
            Key = attribute.ToProperCase();         
            Value = value;
            Operation = operation;

            Enum.TryParse(operation, out FilterOperations opertion);
            OperationType = opertion;
        }

        public FilterQuery(string attribute, string value, FilterOperations operationType)
            : base(attribute)
        {
            Key = attribute.ToProperCase();
            Value = value;
            Operation = operationType.ToString();
            OperationType = operationType;
        }

        [Obsolete("Key has been replaced by '" + nameof(Attribute) + "'. Members should be located by their public name, not by coercing the provided value to the internal name.")]
        public string Key { get; set; }
        public string Value { get; set; }
        [Obsolete("Use '" + nameof(OperationType) + "' instead.")]
        public string Operation { get; set; }

        public FilterOperations OperationType { get; set; }

    }
}
