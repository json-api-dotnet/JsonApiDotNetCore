using System;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Internal.Query
{
    public class FilterQuery : BaseAttrQuery
    {
        /// <summary>
        /// Temporary property while constructor based on string values exists
        /// </summary>
        internal bool IsStringBasedInit { get; } = false;

        [Obsolete("You should use constructors with strongly typed FilterOperations and AttrAttribute or/and RelationshipAttribute parameters.")]
        public FilterQuery(string attribute, string value, string operation)
            :base(null, null)
        {
            Attribute = attribute;
            Key = attribute.ToProperCase();         
            Value = value;
            Operation = operation;

            IsStringBasedInit = true;
            Enum.TryParse(operation, out FilterOperations opertion);
            OperationType = opertion;
        }

        public FilterQuery(AttrAttribute attr, string value, FilterOperations operationType)
            :base(null, attr)
        {
            Value = value;
            OperationType = operationType;
            Key = attr.PublicAttributeName.ToProperCase();
            Operation = operationType.ToString();
        }

        public FilterQuery(RelationshipAttribute relationship, AttrAttribute attr, string value, FilterOperations operationType)
            :base(relationship, attr)
        {
            Value = value;
            OperationType = operationType;
            Key = string.Format("{0}.{1}", Relationship.PublicRelationshipName, Attr.PublicAttributeName);
            Operation = operationType.ToString();
        }

        [Obsolete("Key has been replaced by '" + nameof(Attribute) + "'. Members should be located by their public name, not by coercing the provided value to the internal name.")]
        public string Key { get; set; }
        public string Value { get; set; }
        [Obsolete("Operation has been replaced by '" + nameof(OperationType) + "'. OperationType is typed enum value for Operation property. This should be default property for providing operation type, because of unsustainable string (not typed) value.")]
        public string Operation { get; set; }
        [Obsolete("String based Attribute was replaced by '" + nameof(Attr) + "' property ('" + nameof(AttrAttribute) + "' type) ")]
        public string Attribute { get; }

        public FilterOperations OperationType { get; set; }

    }
}
