using System;
using JsonApiDotNetCore.Extensions;

namespace JsonApiDotNetCore.Internal.Query
{
    public class FilterQuery: QueryAttribute
    {
        [Obsolete("You should use constructor with strongly typed OperationType.")]
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
            Value = value;
            OperationType = operationType;
            Operation = operationType.ToString();
        }

        [Obsolete("Key has been replaced by '" + nameof(Attribute) + "'. Members should be located by their public name, not by coercing the provided value to the internal name.")]
        public string Key { get; set; }
        public string Value { get; set; }
        [Obsolete("Operation has been replaced by '" + nameof(OperationType) + "'. OperationType is typed enum value for Operation property. This should be default property for providing operation type, because of unsustainable string (not typed) value.")]
        public string Operation { get; set; }
        public FilterOperations OperationType { get; set; }

    }
}
