// ReSharper disable InconsistentNaming
using System;
using System.Linq;

namespace JsonApiDotNetCore.Internal.Query
{
    public class FilterOperationsHelper
    {
        /// <summary>
        /// Get filter operation enum and value by string value.
        /// Input string can contain:
        /// a) property value only, then FilterOperations.eq, value is returned
        /// b) filter prefix and value e.g. "prefix:value", then FilterOperations.prefix, value is returned
        /// In case of prefix is provided and is not in FilterOperations enum,
        /// the invalid filter prefix exception is thrown.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static (FilterOperations opereation,string value) GetFilterOperationAndValue(string input)
        {
            // value is empty
            if (input.Length == 0)
                return (FilterOperations.eq, input);

            // split value
            var values = input.Split(QueryConstants.COLON);
            // value only
            if(values.Length == 1)
                return (FilterOperations.eq, input);
            // prefix:value
            else if (values.Length == 2)
            {
                var (operation, succeeded) = ParseFilterOperation(values[0]);
                if (succeeded == false)
                    throw new JsonApiException(400, $"Invalid filter prefix '{values[0]}'");

                return (operation, values[1]);
            }
            // some:colon:value OR prefix:some:colon:value (datetime)
            else
            {
                // succeeded = false means no prefix found => some value with colons(datetime)
                // succeeded = true means prefix provide + some value with colons(datetime)
                var (operation, succeeded) = ParseFilterOperation(values[0]);
                var value = "";
                // datetime
                if(succeeded == false)
                    value = string.Join(QueryConstants.COLON_STR, values);
                else
                    value = string.Join(QueryConstants.COLON_STR, values.Skip(1));
                return (operation, value);
            }
        }

        /// <summary>
        /// Returns typed operation result and info about parsing success
        /// </summary>
        /// <param name="operation">String represented operation</param>
        /// <returns></returns>
        public static (FilterOperations operation, bool succeeded) ParseFilterOperation(string operation)
        {
            var success = Enum.TryParse(operation, out FilterOperations opertion);
            return (opertion, success);
        }
    }
}
