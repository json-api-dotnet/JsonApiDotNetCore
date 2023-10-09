using System.Linq.Expressions;

namespace JsonApiDotNetCore.OpenApi.Client;

public interface IJsonApiClient
{
    /// <summary>
    /// Ensures correct serialization of JSON:API attributes in the request body of a POST/PATCH request at a resource endpoint. Properties with default
    /// values are omitted, unless explicitly included using <paramref name="alwaysIncludedAttributeSelectors" />
    /// <para>
    /// In JSON:API, an omitted attribute indicates to ignore it, while an attribute that is set to <c>null</c> means to clear it. This poses a problem,
    /// because the serializer cannot distinguish between "you have explicitly set this .NET property to its default value" vs "you didn't touch it, so it
    /// contains its default value" when converting to JSON.
    /// </para>
    /// </summary>
    /// <param name="requestDocument">
    /// The request document instance for which default values should be omitted.
    /// </param>
    /// <param name="alwaysIncludedAttributeSelectors">
    /// Optional. A list of lambda expressions that indicate which properties to always include in the JSON request body. For example:
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
    /// <c>using</c> statement, so the registrations are cleaned up after executing the request. After disposal, the client can be reused without the
    /// registrations added earlier.
    /// </returns>
    IDisposable WithPartialAttributeSerialization<TRequestDocument, TAttributesObject>(TRequestDocument requestDocument,
        params Expression<Func<TAttributesObject, object?>>[] alwaysIncludedAttributeSelectors)
        where TRequestDocument : class;
}
