using System.Reflection;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Documents;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators;

internal sealed class JsonApiSchemaGenerator : ISchemaGenerator
{
    private readonly SchemaGenerator _defaultSchemaGenerator;
    private readonly ResourceIdSchemaGenerator _resourceIdSchemaGenerator;
    private readonly DocumentSchemaGenerator[] _documentSchemaGenerators;

    public JsonApiSchemaGenerator(SchemaGenerator defaultSchemaGenerator, ResourceIdSchemaGenerator resourceIdSchemaGenerator,
        IEnumerable<DocumentSchemaGenerator> documentSchemaGenerators)
    {
        ArgumentNullException.ThrowIfNull(defaultSchemaGenerator);
        ArgumentNullException.ThrowIfNull(resourceIdSchemaGenerator);
        ArgumentNullException.ThrowIfNull(documentSchemaGenerators);

        _defaultSchemaGenerator = defaultSchemaGenerator;
        _resourceIdSchemaGenerator = resourceIdSchemaGenerator;
        _documentSchemaGenerators = documentSchemaGenerators as DocumentSchemaGenerator[] ?? documentSchemaGenerators.ToArray();
    }

    public IOpenApiSchema GenerateSchema(Type schemaType, SchemaRepository schemaRepository, MemberInfo? memberInfo = null, ParameterInfo? parameterInfo = null,
        ApiParameterRouteInfo? routeInfo = null)
    {
        ArgumentNullException.ThrowIfNull(schemaType);
        ArgumentNullException.ThrowIfNull(schemaRepository);

        if (parameterInfo is { Name: "id" } && IsJsonApiParameter(parameterInfo))
        {
            return _resourceIdSchemaGenerator.GenerateSchema(parameterInfo, schemaRepository);
        }

        DocumentSchemaGenerator? schemaGenerator = GetDocumentSchemaGenerator(schemaType);

        if (schemaGenerator != null)
        {
            IOpenApiSchema documentSchema = schemaGenerator.GenerateSchema(schemaType, schemaRepository);

            if (memberInfo != null || parameterInfo != null)
            {
                // For unknown reasons, Swashbuckle chooses to wrap request bodies in allOf, but not response bodies.
                // We just replicate that behavior here. See https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/861#issuecomment-1373631712.
                documentSchema = documentSchema.WrapInExtendedSchema();
            }

            return documentSchema;
        }

        return _defaultSchemaGenerator.GenerateSchema(schemaType, schemaRepository, memberInfo, parameterInfo, routeInfo);
    }

    private static bool IsJsonApiParameter(ParameterInfo parameter)
    {
        return parameter.Member.DeclaringType != null && parameter.Member.DeclaringType.IsAssignableTo(typeof(CoreJsonApiController));
    }

    private DocumentSchemaGenerator? GetDocumentSchemaGenerator(Type schemaType)
    {
        foreach (DocumentSchemaGenerator documentSchemaGenerator in _documentSchemaGenerators)
        {
            if (documentSchemaGenerator.CanGenerate(schemaType))
            {
                return documentSchemaGenerator;
            }
        }

        return null;
    }
}
