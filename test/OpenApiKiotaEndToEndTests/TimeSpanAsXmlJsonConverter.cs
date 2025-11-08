using System.Xml;
using OpenApiTests;

namespace OpenApiKiotaEndToEndTests;

// Kiota requires ISO8601 syntax when "duration" format is used in OpenAPI.
internal sealed class TimeSpanAsXmlJsonConverter()
    : ValueTypeJsonConverter<TimeSpan>(XmlConvert.ToTimeSpan, XmlConvert.ToString);
