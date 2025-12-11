using Microsoft.CodeAnalysis;

namespace JsonApiDotNetCore.SourceGenerators;

internal readonly record struct TypeInfo(string Namespace, string TypeName)
{
    // Uncomment to verify non-cached outputs are detected in tests.
    //public readonly object Dummy = new();

    // Uncomment to verify banned types are detected in tests.
    //private static readonly SyntaxNode FrozenIdentifier = SyntaxFactory.IdentifierName("some");
    //public readonly SyntaxNode Dummy = FrozenIdentifier;

    public static TypeInfo CreateFromQualified(ITypeSymbol typeSymbol)
    {
        string @namespace = GetNamespace(typeSymbol);
        return new TypeInfo(@namespace, typeSymbol.Name);
    }

    public static TypeInfo? TryCreateFromQualifiedOrPossiblyNullableKeyword(ITypeSymbol typeSymbol)
    {
        ITypeSymbol innerTypeSymbol = UnwrapNullableValueTypeOrSelf(typeSymbol);

        if (innerTypeSymbol.Kind == SymbolKind.ErrorType)
        {
            return null;
        }

        if (innerTypeSymbol.SpecialType != SpecialType.None)
        {
            // Built-in types that don't need a namespace import, such as: int, long?, string, string?
            return new TypeInfo(string.Empty, typeSymbol.ToString());
        }

        string @namespace = GetNamespace(innerTypeSymbol);
        string typeName = !ReferenceEquals(innerTypeSymbol, typeSymbol) ? $"{innerTypeSymbol.Name}?" : innerTypeSymbol.Name;

        // Fully-qualified types, such as: System.Guid, System.Guid?
        return new TypeInfo(@namespace, typeName);
    }

    private static ITypeSymbol UnwrapNullableValueTypeOrSelf(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
        {
            if (namedTypeSymbol.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T && namedTypeSymbol.TypeArguments.Length == 1)
            {
                return namedTypeSymbol.TypeArguments[0];
            }
        }

        return typeSymbol;
    }

    private static string GetNamespace(ITypeSymbol typeSymbol)
    {
        return typeSymbol.ContainingNamespace == null || typeSymbol.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : typeSymbol.ContainingNamespace.ToString();
    }

    public override string ToString()
    {
        return Namespace.Length > 0 ? $"{Namespace}.{TypeName}" : TypeName;
    }
}
