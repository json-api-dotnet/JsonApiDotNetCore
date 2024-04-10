using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Response;

/// <summary>
/// Converts the produced model from an ASP.NET controller action into a <see cref="Document" />, ready to be serialized as the response body.
/// </summary>
public interface IResponseModelAdapter
{
    /// <summary>
    /// Validates and converts the specified <paramref name="model" />. Supported model types:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <c><![CDATA[IEnumerable<IIdentifiable>]]></c>
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c><![CDATA[IIdentifiable]]></c>
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c><![CDATA[null]]></c>
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c><![CDATA[IEnumerable<OperationContainer?>]]></c>
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c><![CDATA[IEnumerable<ErrorObject>]]></c>
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c><![CDATA[ErrorObject]]></c>
    /// </description>
    /// </item>
    /// </list>
    /// </summary>
    Document Convert(object? model);
}
