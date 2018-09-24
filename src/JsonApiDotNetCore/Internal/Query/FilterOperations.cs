// ReSharper disable InconsistentNaming
using System;

namespace JsonApiDotNetCore.Internal.Query
{
    public enum FilterOperationsEnum
    {
        eq = 0,
        lt = 1,
        gt = 2,
        le = 3,
        ge = 4,
        like = 5,
        ne = 6,
        @in = 7, // prefix with @ to use keyword
        nin = 8,
        isnull = 9,
        isnotnull = 10
    }

    public class FilterOperations
    {
        public static FilterOperationsEnum GetFilterOperation(string prefix)
        {
            if (prefix.Length == 0) return FilterOperationsEnum.eq;

            if (Enum.TryParse(prefix, out FilterOperationsEnum opertion) == false)
                throw new JsonApiException(400, $"Invalid filter prefix '{prefix}'");

            return opertion;
        }

        public static string GetFilterOperationFromQuery(string query)
        {
            var values = query.Split(QueryConstants.COLON);

            if (values.Length == 1)
                return string.Empty;

            var operation = values[0];
            // remove prefix from value
            if (Enum.TryParse(operation, out FilterOperationsEnum op) == false)
                return string.Empty;

            return operation;
        }

    }
}
