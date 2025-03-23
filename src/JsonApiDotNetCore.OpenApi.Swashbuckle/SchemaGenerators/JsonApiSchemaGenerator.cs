using System.Diagnostics;
using System.Reflection;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Components;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators.Documents;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators;

internal sealed class JsonApiSchemaGenerator : ISchemaGenerator
{
    private readonly ResourceIdSchemaGenerator _resourceIdSchemaGenerator;
    private readonly DocumentSchemaGenerator[] _documentSchemaGenerators;

    public JsonApiSchemaGenerator(ResourceIdSchemaGenerator resourceIdSchemaGenerator, IEnumerable<DocumentSchemaGenerator> documentSchemaGenerators)
    {
        ArgumentNullException.ThrowIfNull(resourceIdSchemaGenerator);
        ArgumentNullException.ThrowIfNull(documentSchemaGenerators);

        _resourceIdSchemaGenerator = resourceIdSchemaGenerator;
        _documentSchemaGenerators = documentSchemaGenerators as DocumentSchemaGenerator[] ?? documentSchemaGenerators.ToArray();
    }

    public OpenApiSchema GenerateSchema(Type modelType, SchemaRepository schemaRepository, MemberInfo? memberInfo = null, ParameterInfo? parameterInfo = null,
        ApiParameterRouteInfo? routeInfo = null)
    {
        ArgumentNullException.ThrowIfNull(modelType);
        ArgumentNullException.ThrowIfNull(schemaRepository);

        if (parameterInfo is { Name: "id" } && IsJsonApiParameter(parameterInfo))
        {
            return _resourceIdSchemaGenerator.GenerateSchema(modelType, schemaRepository);
        }

        DocumentSchemaGenerator schemaGenerator = GetDocumentSchemaGenerator(modelType);
        OpenApiSchema referenceSchema = schemaGenerator.GenerateSchema(modelType, schemaRepository);

        if (memberInfo != null || parameterInfo != null)
        {
            // For unknown reasons, Swashbuckle chooses to wrap request bodies in allOf, but not response bodies.
            // We just replicate that behavior here. See https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/861#issuecomment-1373631712.
            referenceSchema = referenceSchema.WrapInExtendedSchema();
        }

        return referenceSchema;
    }

    private static bool IsJsonApiParameter(ParameterInfo parameter)
    {
        return parameter.Member.DeclaringType != null && parameter.Member.DeclaringType.IsAssignableTo(typeof(CoreJsonApiController));
    }

    private DocumentSchemaGenerator GetDocumentSchemaGenerator(Type modelType)
    {
        foreach (DocumentSchemaGenerator documentSchemaGenerator in _documentSchemaGenerators)
        {
            if (documentSchemaGenerator.CanGenerate(modelType))
            {
                return documentSchemaGenerator;
            }
        }

        throw new UnreachableException();
    }
}
