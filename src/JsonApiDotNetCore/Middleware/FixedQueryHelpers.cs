using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

#pragma warning disable AV1008 // Class should not be static
#pragma warning disable AV1708 // Type name contains term that should be avoided
#pragma warning disable AV1130 // Return type in method signature should be a collection interface instead of a concrete type
#pragma warning disable AV1532 // Loop statement contains nested loop

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Replacement implementation for the ASP.NET built-in <see cref="QueryHelpers" />, to workaround bug https://github.com/dotnet/aspnetcore/issues/33394.
    /// This is identical to the built-in version, except it properly un-escapes query string keys without a value.
    /// </summary>
    internal static class FixedQueryHelpers
    {
        /// <summary>
        /// Parse a query string into its component key and value parts.
        /// </summary>
        /// <param name="queryString">
        /// The raw query string value, with or without the leading '?'.
        /// </param>
        /// <returns>
        /// A collection of parsed keys and values, null if there are no entries.
        /// </returns>
        public static Dictionary<string, StringValues>? ParseNullableQuery(string queryString)
        {
            var accumulator = new KeyValueAccumulator();

            if (string.IsNullOrEmpty(queryString) || queryString == "?")
            {
                return null;
            }

            int scanIndex = 0;

            if (queryString[0] == '?')
            {
                scanIndex = 1;
            }

            int textLength = queryString.Length;
            int equalIndex = queryString.IndexOf('=');

            if (equalIndex == -1)
            {
                equalIndex = textLength;
            }

            while (scanIndex < textLength)
            {
                int delimiterIndex = queryString.IndexOf('&', scanIndex);

                if (delimiterIndex == -1)
                {
                    delimiterIndex = textLength;
                }

                if (equalIndex < delimiterIndex)
                {
                    while (scanIndex != equalIndex && char.IsWhiteSpace(queryString[scanIndex]))
                    {
                        ++scanIndex;
                    }

                    string name = queryString.Substring(scanIndex, equalIndex - scanIndex);
                    string value = queryString.Substring(equalIndex + 1, delimiterIndex - equalIndex - 1);
                    accumulator.Append(Uri.UnescapeDataString(name.Replace('+', ' ')), Uri.UnescapeDataString(value.Replace('+', ' ')));
                    equalIndex = queryString.IndexOf('=', delimiterIndex);

                    if (equalIndex == -1)
                    {
                        equalIndex = textLength;
                    }
                }
                else
                {
                    if (delimiterIndex > scanIndex)
                    {
                        // original code:
                        // accumulator.Append(queryString.Substring(scanIndex, delimiterIndex - scanIndex), string.Empty);

                        // replacement:
                        string name = queryString.Substring(scanIndex, delimiterIndex - scanIndex);
                        accumulator.Append(Uri.UnescapeDataString(name.Replace('+', ' ')), string.Empty);
                    }
                }

                scanIndex = delimiterIndex + 1;
            }

            if (!accumulator.HasValues)
            {
                return null;
            }

            return accumulator.GetResults();
        }
    }
}
