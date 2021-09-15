using System;
using System.Linq.Expressions;

namespace JsonApiDotNetCore.OpenApi.Client
{
    public interface IJsonApiClient
    {
        /// <summary>
        /// Ensures correct serialization of attributes in a POST/PATCH Resource request body. In JSON:API, an omitted attribute indicates to ignore it, while an
        /// attribute that is set to "null" means to clear it. This poses a problem because the serializer cannot distinguish between "you have explicitly set
        /// this .NET property to null" vs "you didn't touch it, so it is null by default" when converting an instance to JSON. Therefore, calling this method
        /// treats all attributes that contain their default value (<c>null</c> for reference types, <c>0</c> for integers, <c>false</c> for booleans, etc) as
        /// omitted unless explicitly listed to include them using <paramref name="alwaysIncludedAttributeSelectors" />.
        /// </summary>
        /// <param name="requestDocument">
        /// The request document instance for which this registration applies.
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
        IDisposable RegisterAttributesForRequestDocument<TRequestDocument, TAttributesObject>(TRequestDocument requestDocument,
            params Expression<Func<TAttributesObject, object>>[] alwaysIncludedAttributeSelectors)
            where TRequestDocument : class;
    }
}
