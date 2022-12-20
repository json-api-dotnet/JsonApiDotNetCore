using System.Linq.Expressions;

namespace JsonApiDotNetCore.OpenApi.Client;

public interface IJsonApiClient
{
    /// <summary>
    /// <para>
    /// Calling this method ensures that attributes containing a default value (<c>null</c> for reference types, <c>0</c> for integers, <c>false</c> for
    /// booleans, etc) are omitted during serialization, except for those explicitly marked for inclusion in
    /// <paramref name="alwaysIncludedAttributeSelectors" />.
    /// </para>
    /// <para>
    /// This is sometimes required to ensure correct serialization of attributes during a POST/PATCH request. In JSON:API, an omitted attribute indicates to
    /// ignore it, while an attribute that is set to "null" means to clear it. This poses a problem because the serializer cannot distinguish between "you
    /// have explicitly set this .NET property to null" vs "you didn't touch it, so it is null by default" when converting an instance to JSON.
    /// </para>
    /// </summary>
    /// <param name="requestDocument">
    /// The request document instance for which default values should be omitted.
    /// </param>
    /// <param name="alwaysIncludedAttributeSelectors">
    /// Optional. A list of expressions to indicate which properties to unconditionally include in the JSON request body. For example:
    /// <code><![CDATA[
    /// video => video.Title, video => video.Summary
    /// ]]></code>
    /// </param>
    /// <typeparam name="TRequestDocument">
    /// The type of the request document.
    /// </typeparam>
    /// <typeparam name="TAttributesObject">
    /// The type of the attributes object inside <typeparamref name="TRequestDocument" />.
    /// </typeparam>
    /// <returns>
    /// An <see cref="IDisposable" /> to clear the current registration. For efficient memory usage, it is recommended to wrap calls to this method in a
    /// <c>using</c> statement, so the registrations are cleaned up after executing the request.
    /// </returns>
    IDisposable OmitDefaultValuesForAttributesInRequestDocument<TRequestDocument, TAttributesObject>(TRequestDocument requestDocument,
        params Expression<Func<TAttributesObject, object?>>[] alwaysIncludedAttributeSelectors)
        where TRequestDocument : class;
}
