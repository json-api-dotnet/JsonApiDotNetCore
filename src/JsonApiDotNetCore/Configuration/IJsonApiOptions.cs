using System.Data;
using System.Text.Json;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Configuration;

/// <summary>
/// Global options that configure the behavior of JsonApiDotNetCore.
/// </summary>
public interface IJsonApiOptions
{
    /// <summary>
    /// The URL prefix to use for exposed endpoints.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// options.Namespace = "api/shopping";
    /// ]]></code>
    /// </example>
    string? Namespace { get; }

    /// <summary>
    /// Specifies the default set of allowed capabilities on JSON:API attributes. Defaults to <see cref="AttrCapabilities.All" />. This setting can be
    /// overruled per attribute using <see cref="AttrAttribute.Capabilities" />.
    /// </summary>
    AttrCapabilities DefaultAttrCapabilities { get; }

    /// <summary>
    /// Specifies the default set of allowed capabilities on JSON:API to-one relationships. Defaults to <see cref="HasOneCapabilities.All" />. This setting
    /// can be overruled per relationship using <see cref="HasOneAttribute.Capabilities" />.
    /// </summary>
    HasOneCapabilities DefaultHasOneCapabilities { get; }

    /// <summary>
    /// Specifies the default set of allowed capabilities on JSON:API to-many relationships. Defaults to <see cref="HasManyCapabilities.All" />. This setting
    /// can be overruled per relationship using <see cref="HasManyAttribute.Capabilities" />.
    /// </summary>
    HasManyCapabilities DefaultHasManyCapabilities { get; }

    /// <summary>
    /// Whether to include a 'jsonapi' object in responses, which contains the highest JSON:API version supported. <c>false</c> by default.
    /// </summary>
    bool IncludeJsonApiVersion { get; }

    /// <summary>
    /// Whether to include <see cref="Exception" /> stack traces in <see cref="ErrorObject.Meta" /> responses. <c>false</c> by default.
    /// </summary>
    bool IncludeExceptionStackTraceInErrors { get; }

    /// <summary>
    /// Whether to include the request body in <see cref="Document.Meta" /> responses when it is invalid. <c>false</c> by default.
    /// </summary>
    bool IncludeRequestBodyInErrors { get; }

    /// <summary>
    /// Whether to use relative links for all resources. <c>false</c> by default.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// options.UseRelativeLinks = true;
    /// ]]></code>
    /// <code><![CDATA[
    /// {
    ///   "type": "articles",
    ///   "id": "4309",
    ///   "relationships": {
    ///      "author": {
    ///        "links": {
    ///          "self": "/api/shopping/articles/4309/relationships/author",
    ///          "related": "/api/shopping/articles/4309/author"
    ///        }
    ///      }
    ///   }
    /// }
    /// ]]></code>
    /// </example>
    bool UseRelativeLinks { get; }

    /// <summary>
    /// Configures which links to write in the <see cref="Serialization.Objects.TopLevelLinks" /> object. Defaults to <see cref="LinkTypes.All" />. This
    /// setting can be overruled per resource type by adding <see cref="ResourceLinksAttribute" /> on the class definition of a resource.
    /// </summary>
    LinkTypes TopLevelLinks { get; }

    /// <summary>
    /// Configures which links to write in the <see cref="Serialization.Objects.ResourceLinks" /> object. Defaults to <see cref="LinkTypes.All" />. This
    /// setting can be overruled per resource type by adding <see cref="ResourceLinksAttribute" /> on the class definition of a resource.
    /// </summary>
    LinkTypes ResourceLinks { get; }

    /// <summary>
    /// Configures which links to write in the <see cref="Serialization.Objects.RelationshipLinks" /> object. Defaults to <see cref="LinkTypes.All" />. This
    /// setting can be overruled for all relationships per resource type by adding <see cref="ResourceLinksAttribute" /> on the class definition of a
    /// resource. This can be further overruled per relationship by setting <see cref="RelationshipAttribute.Links" />.
    /// </summary>
    LinkTypes RelationshipLinks { get; }

    /// <summary>
    /// Whether to include the total resource count in top-level meta objects. This requires an additional database query. <c>false</c> by default.
    /// </summary>
    bool IncludeTotalResourceCount { get; }

    /// <summary>
    /// The page size (10 by default) that is used when not specified in query string. Set to <c>null</c> to not use pagination by default.
    /// </summary>
    PageSize? DefaultPageSize { get; }

    /// <summary>
    /// The maximum page size that can be used, or <c>null</c> for unconstrained (default).
    /// </summary>
    PageSize? MaximumPageSize { get; }

    /// <summary>
    /// The maximum page number that can be used, or <c>null</c> for unconstrained (default).
    /// </summary>
    PageNumber? MaximumPageNumber { get; }

    /// <summary>
    /// Whether ASP.NET ModelState validation is enabled. <c>true</c> by default.
    /// </summary>
    bool ValidateModelState { get; }

    /// <summary>
    /// Whether clients are allowed or required to provide IDs when creating resources. <see cref="ClientIdGenerationMode.Forbidden" /> by default. This
    /// setting can be overruled per resource type using <see cref="ResourceAttribute.ClientIdGeneration" />.
    /// </summary>
    ClientIdGenerationMode ClientIdGeneration { get; }

    /// <summary>
    /// Whether clients can provide IDs when creating resources. When not allowed, a 403 Forbidden response is returned if a client attempts to create a
    /// resource with a defined ID. <c>false</c> by default.
    /// </summary>
    /// <remarks>
    /// Setting this to <c>true</c> corresponds to <see cref="ClientIdGenerationMode.Allowed" />, while <c>false</c> corresponds to
    /// <see cref="ClientIdGenerationMode.Forbidden" />.
    /// </remarks>
    [PublicAPI]
    [Obsolete("Use ClientIdGeneration instead.")]
    bool AllowClientGeneratedIds { get; }

    /// <summary>
    /// Whether to produce an error on unknown query string parameters. <c>false</c> by default.
    /// </summary>
    bool AllowUnknownQueryStringParameters { get; }

    /// <summary>
    /// Whether to produce an error on unknown attribute and relationship keys in request bodies. <c>false</c> by default.
    /// </summary>
    bool AllowUnknownFieldsInRequestBody { get; }

    /// <summary>
    /// Determines whether legacy filter notation in query strings (such as =eq:, =like:, and =in:) is enabled. <c>false</c> by default.
    /// </summary>
    bool EnableLegacyFilterNotation { get; }

    /// <summary>
    /// Controls how many levels deep includes are allowed to be nested. For example, MaximumIncludeDepth=1 would allow ?include=articles but not
    /// ?include=articles.revisions. <c>null</c> by default, which means unconstrained.
    /// </summary>
    int? MaximumIncludeDepth { get; }

    /// <summary>
    /// Limits the maximum number of operations allowed per atomic:operations request. Defaults to 10. Set to <c>null</c> for unlimited.
    /// </summary>
    int? MaximumOperationsPerRequest { get; }

    /// <summary>
    /// Enables to override the default isolation level for database transactions, enabling to balance between consistency and performance. Defaults to
    /// <c>null</c>, which leaves this up to Entity Framework Core to choose (and then it varies per database provider).
    /// </summary>
    IsolationLevel? TransactionIsolationLevel { get; }

    /// <summary>
    /// Enables to customize the settings that are used by the <see cref="JsonSerializer" />.
    /// </summary>
    /// <example>
    /// The next example sets the naming convention to camel casing.
    /// <code><![CDATA[
    /// options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    /// options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    /// ]]></code>
    /// </example>
    JsonSerializerOptions SerializerOptions { get; }

    /// <summary>
    /// Gets the settings used for deserializing request bodies. This value is based on <see cref="SerializerOptions" /> and is intended for internal use.
    /// </summary>
    JsonSerializerOptions SerializerReadOptions { get; }

    /// <summary>
    /// Gets the settings used for serializing response bodies. This value is based on <see cref="SerializerOptions" /> and is intended for internal use.
    /// </summary>
    JsonSerializerOptions SerializerWriteOptions { get; }
}
