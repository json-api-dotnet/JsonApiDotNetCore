using Microsoft.CodeAnalysis;

namespace JsonApiDotNetCore.SourceGenerators;

/// <summary>
/// Basic outcome from the code analysis.
/// </summary>
internal readonly record struct CoreControllerInfo(
    TypeInfo ResourceType, TypeInfo IdType, string ControllerNamespace, JsonApiEndpointsCopy Endpoints, bool WriteNullableEnable)
{
    // Using readonly fields, so they can be passed by reference (using 'in' modifier, to avoid making copies) during code generation.
    public readonly TypeInfo ResourceType = ResourceType;
    public readonly TypeInfo IdType = IdType;
    public readonly string ControllerNamespace = ControllerNamespace;
    public readonly JsonApiEndpointsCopy Endpoints = Endpoints;
    public readonly bool WriteNullableEnable = WriteNullableEnable;

    public static CoreControllerInfo? TryCreate(INamedTypeSymbol resourceTypeSymbol, ITypeSymbol idTypeSymbol, JsonApiEndpointsCopy endpoints,
        string controllerNamespace)
    {
        TypeInfo? resourceTypeInfo = TypeInfo.CreateFromQualified(resourceTypeSymbol);
        TypeInfo? idTypeInfo = TypeInfo.TryCreateFromQualifiedOrPossiblyNullableKeyword(idTypeSymbol);

        if (idTypeInfo == null)
        {
            return null;
        }

        bool writeNullableEnable = idTypeSymbol is { IsReferenceType: true, NullableAnnotation: NullableAnnotation.Annotated };

        return new CoreControllerInfo(resourceTypeInfo.Value, idTypeInfo.Value, controllerNamespace, endpoints, writeNullableEnable);
    }
}
