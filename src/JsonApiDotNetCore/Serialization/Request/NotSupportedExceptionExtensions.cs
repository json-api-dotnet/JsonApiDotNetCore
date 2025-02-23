using System.Text.Json.Serialization;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Request;

/// <summary>
/// A hacky approach to obtain the proper JSON:API source pointer from an exception thrown in a <see cref="JsonConverter" />.
/// </summary>
/// <remarks>
/// <para>
/// This method relies on the behavior at
/// https://github.com/dotnet/runtime/blob/release/8.0/src/libraries/System.Text.Json/src/System/Text/Json/Serialization/JsonConverterOfT.ReadCore.cs#L100,
/// which wraps a thrown <see cref="NotSupportedException" /> and adds the JSON path to the outer exception message, based on internal reader state.
/// </para>
/// <para>
/// To take advantage of this, we expect a custom converter to throw a <see cref="NotSupportedException" /> with a specially-crafted
/// <see cref="Exception.Source" /> and a nested <see cref="JsonApiException" /> containing a relative source pointer and a captured stack trace. Once
/// all of that happens, this class extracts the added JSON path from the outer exception message and converts it to a JSON:API pointer to enrich the
/// nested <see cref="JsonApiException" /> with.
/// </para>
/// </remarks>
internal static class NotSupportedExceptionExtensions
{
    private const string LeadingText = " Path: ";
    private const string TrailingText = " | LineNumber: ";

    public static bool HasJsonApiException(this NotSupportedException exception)
    {
        return exception.InnerException is NotSupportedException { InnerException: JsonApiException };
    }

    public static JsonApiException EnrichSourcePointer(this NotSupportedException exception)
    {
        var jsonApiException = (JsonApiException)exception.InnerException!.InnerException!;
        string? sourcePointer = GetSourcePointerFromMessage(exception.Message);

        if (sourcePointer != null)
        {
            foreach (ErrorObject error in jsonApiException.Errors)
            {
                if (error.Source == null)
                {
                    error.Source = new ErrorSource
                    {
                        Pointer = sourcePointer
                    };
                }
                else
                {
                    error.Source.Pointer = sourcePointer + '/' + error.Source.Pointer;
                }
            }
        }

        return jsonApiException;
    }

    private static string? GetSourcePointerFromMessage(string message)
    {
        string? jsonPath = ExtractJsonPathFromMessage(message);
        return JsonPathToSourcePointer(jsonPath);
    }

    private static string? ExtractJsonPathFromMessage(string message)
    {
        int startIndex = message.IndexOf(LeadingText, StringComparison.Ordinal);

        if (startIndex != -1)
        {
            int stopIndex = message.IndexOf(TrailingText, startIndex, StringComparison.Ordinal);

            if (stopIndex != -1)
            {
                return message.Substring(startIndex + LeadingText.Length, stopIndex - startIndex - LeadingText.Length);
            }
        }

        return null;
    }

    private static string? JsonPathToSourcePointer(string? jsonPath)
    {
        if (jsonPath != null && jsonPath.StartsWith('$'))
        {
            return jsonPath[1..].Replace('.', '/');
        }

        return null;
    }
}
