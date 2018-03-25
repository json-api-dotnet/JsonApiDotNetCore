using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models.Operations;

namespace JsonApiDotNetCore.Models.Pointers
{
    internal class OperationsPointer : Pointer<Operation>
    {
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="JsonApiException"></exception>
        public override object GetValue(object root)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            if (PointerAddress == null) throw new InvalidOperationException("Cannot get pointer value from null PointerAddress");

            if (root is List<Operation> operations)
                return GetValueFromRoot(operations);

            throw new ArgumentException(nameof(root));
        }

        private object GetValueFromRoot(List<Operation> operations)
        {
            var pathSegments = PointerAddress.ToLower().Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (pathSegments.Length < 4)
                throw BadRequestException("number of segments", pathSegments.Length);

            if (pathSegments[0] != "operations")
                throw BadRequestException("prefix", pathSegments[0]);

            // /operations/{operationIndex} → operations = [...]
            if (int.TryParse(pathSegments[1], out int operationIndex))
                return GetValueFromOperation(operations[operationIndex], pathSegments);
            else
                throw BadRequestException("operation index", operationIndex);
        }

        private object GetValueFromOperation(Operation operation, string[] pathSegments)
        {
            var operationPropertyName = pathSegments[2];

            // /operations/0/ref → ref = {...}
            if (operationPropertyName == "ref")
                return GetValueFromRef(operation.Ref, pathSegments);

            // /operations/0/data → data = {...}
            if (operation.DataIsList == false)
                return GetValueFromData(operation.DataObject, pathSegments, segementStartIndex: 3);

            // /operations/0/data/{dataIndex} → data = [...]
            if (int.TryParse(pathSegments[3], out int dataIndex))
            {
                if (operation.DataList.Count >= dataIndex - 1)
                    return GetValueFromData(operation.DataList[dataIndex], pathSegments, segementStartIndex: 4);
                throw BadRequestException("data index", dataIndex, "Pointer references an index in the data array that cannot be found at the specified position.");
            }
            else
            {
                throw BadRequestException("data index", dataIndex, "Pointer segement should provide array index but could not be parsed to an integer.");
            }
        }

        private object GetValueFromRef(ResourceReference reference, string[] pathSegments)
        {
            const int segementStartIndex = 3;

            // /operations/0/ref/{dataPropertyName}
            if (pathSegments.Length <= segementStartIndex)
                throw BadRequestException("length", pathSegments.Length, "Pointer does not contain enough segments to locate ref property.");

            var dataPropertyName = pathSegments[segementStartIndex];
            switch (dataPropertyName)
            {
                case "id":
                    return reference.Id;
                case "type":
                    return reference.Type;
                default:
                    throw BadRequestException("ref property name", dataPropertyName, "Only 'id' and 'type' pointers are supported.");
            }
        }

        private object GetValueFromData(DocumentData data, string[] pathSegments, int segementStartIndex)
        {
            // /operations/0/data/{dataPropertyName}
            if (pathSegments.Length <= segementStartIndex)
                throw BadRequestException("length", pathSegments.Length, "Pointer does not contain enough segments to locate data property.");

            var dataPropertyName = pathSegments[segementStartIndex];
            switch (dataPropertyName)
            {
                case "id":
                    return data.Id;
                case "type":
                    return data.Type;
                default:
                    throw BadRequestException("data property name", dataPropertyName, "Only 'id' and 'type' pointers are supported.");
            }
        }

        private JsonApiException BadRequestException(string condition, object value, string extraDetail = null)
            => new JsonApiException(400, $"Operations pointer has invalid {condition} '{value}' in pointer '{PointerAddress}'. {extraDetail}");
    }
}
