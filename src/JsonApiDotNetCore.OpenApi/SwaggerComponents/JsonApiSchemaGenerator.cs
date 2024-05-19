using System.Reflection;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.OpenApi.JsonApiObjects;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents;

internal sealed class JsonApiSchemaGenerator : ISchemaGenerator
{
    private static readonly OpenApiSchema IdTypeSchema = new()
    {
        Type = "string"
    };

    private readonly ISchemaGenerator _defaultSchemaGenerator;
    private readonly DocumentSchemaGenerator _documentSchemaGenerator;

    public JsonApiSchemaGenerator(SchemaGenerator defaultSchemaGenerator, DocumentSchemaGenerator documentSchemaGenerator)
    {
        ArgumentGuard.NotNull(defaultSchemaGenerator);
        ArgumentGuard.NotNull(documentSchemaGenerator);

        _defaultSchemaGenerator = defaultSchemaGenerator;
        _documentSchemaGenerator = documentSchemaGenerator;
    }

    public OpenApiSchema GenerateSchema(Type modelType, SchemaRepository schemaRepository, MemberInfo? memberInfo = null, ParameterInfo? parameterInfo = null,
        ApiParameterRouteInfo? routeInfo = null)
    {
        ArgumentGuard.NotNull(modelType);
        ArgumentGuard.NotNull(schemaRepository);

        if (parameterInfo is { Name: "id" } && IsJsonApiParameter(parameterInfo))
        {
            return IdTypeSchema;
        }

        if (schemaRepository.TryLookupByType(modelType, out OpenApiSchema jsonApiDocumentSchema))
        {
            // For unknown reasons, Swashbuckle chooses to wrap root request bodies, but not response bodies.
            // See https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/861#issuecomment-1373631712
            return memberInfo != null || parameterInfo != null
                ? _defaultSchemaGenerator.GenerateSchema(modelType, schemaRepository, memberInfo, parameterInfo)
                : jsonApiDocumentSchema;
        }

        if (JsonApiSchemaFacts.RequiresCustomSchemaGenerator(modelType))
        {
            _ = _documentSchemaGenerator.GenerateSchema(modelType, schemaRepository);

            // Schema might depend on other schemas not handled by us, so should not return here.
        }

        return _defaultSchemaGenerator.GenerateSchema(modelType, schemaRepository, memberInfo, parameterInfo, routeInfo);
    }

    private static bool IsJsonApiParameter(ParameterInfo parameter)
    {
        return parameter.Member.DeclaringType != null && parameter.Member.DeclaringType.IsAssignableTo(typeof(CoreJsonApiController));
    }
}
