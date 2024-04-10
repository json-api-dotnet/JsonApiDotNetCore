using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Request.Adapters;

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
    /// <c><![CDATA[IList<OperationContainer>]]></c> (operations)
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c><![CDATA[ISet<IIdentifiable>]]></c> (to-many relationship, unknown relationship)
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c><![CDATA[IIdentifiable]]></c> (resource, to-one relationship)
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c><![CDATA[null]]></c> (to-one relationship)
    /// </description>
    /// </item>
    /// </list>
    /// </summary>
    object? Convert(Document document);
}
