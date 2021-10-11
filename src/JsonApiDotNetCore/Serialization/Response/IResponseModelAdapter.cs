using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Response
{
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
        /// <code><![CDATA[IEnumerable<IIdentifiable>]]></code>
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <code><![CDATA[IIdentifiable]]></code>
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <code><![CDATA[null]]></code>
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <code><![CDATA[IEnumerable<OperationContainer?>]]></code>
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <code><![CDATA[IEnumerable<ErrorObject>]]></code>
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <code><![CDATA[ErrorObject]]></code>
        /// </description>
        /// </item>
        /// </list>
        /// </summary>
        Document Convert(object? model);
    }
}
