#nullable disable

using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Request.Adapters
{
    /// <summary>
    /// The entry point for validating and converting the deserialized <see cref="Document" /> from the request body into a model. The produced models are
    /// used in ASP.NET Model Binding.
    /// </summary>
    public interface IDocumentAdapter
    {
        /// <summary>
        /// Validates and converts the specified <paramref name="document" />. Possible return values:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// <code><![CDATA[IList<OperationContainer>]]></code> (operations)
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <code><![CDATA[ISet<IIdentifiable>]]></code> (to-many relationship, unknown relationship)
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <code><![CDATA[IIdentifiable]]></code> (resource, to-one relationship)
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <code><![CDATA[null]]></code> (to-one relationship)
        /// </description>
        /// </item>
        /// </list>
        /// </summary>
        object Convert(Document document);
    }
}
