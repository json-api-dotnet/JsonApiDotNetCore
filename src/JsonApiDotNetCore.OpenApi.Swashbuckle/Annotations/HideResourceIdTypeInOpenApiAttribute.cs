namespace JsonApiDotNetCore.OpenApi.Swashbuckle.Annotations;

/// <summary>
/// Hides the underlying resource ID type in OpenAPI documents.
/// </summary>
/// <remarks>
/// For example, when used on a resource type that implements <c><![CDATA[IIdentifiable<long>]]></c>, excludes the <c>format</c> property on the ID
/// schema. As a result, the ID type is displayed as <c>string</c> instead of
/// <c>
/// string($int64)
/// </c>
/// in SwaggerUI.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct)]
public sealed class HideResourceIdTypeInOpenApiAttribute : Attribute;
