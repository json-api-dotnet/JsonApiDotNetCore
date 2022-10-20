using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace JsonApiDotNetCore.SourceGenerators;

/// <summary>
/// Collects type declarations in the project that have at least one attribute on them. Because this receiver operates at the syntax level, we cannot
/// check for the expected attribute. This must be done during semantic analysis, because source code may contain any of these:
/// <code><![CDATA[
/// using JsonApiDotNetCore.Resources.Annotations;
/// using AlternateNamespaceName = JsonApiDotNetCore.Resources;
/// using AlternateTypeName = JsonApiDotNetCore.Resources.Annotations.ResourceAttribute;
/// 
/// [Resource]
/// public class ExampleResource1 : Identifiable<int> { }
/// 
/// [ResourceAttribute]
/// public class ExampleResource2 : Identifiable<int> { }
/// 
/// [AlternateNamespaceName.Annotations.Resource]
/// public class ExampleResource3 : Identifiable<int> { }
/// 
/// [AlternateTypeName]
/// public class ExampleResource4 : Identifiable<int> { }
/// ]]></code>
/// </summary>
internal sealed class TypeWithAttributeSyntaxReceiver : ISyntaxReceiver
{
    public readonly ISet<TypeDeclarationSyntax> TypeDeclarations = new HashSet<TypeDeclarationSyntax>();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is TypeDeclarationSyntax typeDeclarationSyntax && typeDeclarationSyntax.AttributeLists.Any())
        {
            TypeDeclarations.Add(typeDeclarationSyntax);
        }
    }
}
