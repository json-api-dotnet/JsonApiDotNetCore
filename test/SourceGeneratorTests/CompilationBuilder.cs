using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;

namespace SourceGeneratorTests;

internal sealed class CompilationBuilder
{
    private static readonly CSharpCompilationOptions DefaultOptions = new(OutputKind.DynamicallyLinkedLibrary);

    private readonly HashSet<SyntaxTree> _syntaxTrees = new();
    private readonly HashSet<MetadataReference> _references = new();

    public Compilation Build()
    {
        return CSharpCompilation.Create("compilation", _syntaxTrees, _references, DefaultOptions);
    }

    public CompilationBuilder WithDefaultReferences()
    {
        WithSystemReferences();
        WithLoggerFactoryReference();
        WithJsonApiDotNetCoreReferences();
        return this;
    }

    public CompilationBuilder WithSystemReferences()
    {
        string objectLocation = typeof(object).Assembly.Location;

        PortableExecutableReference systemRuntimeReference =
            MetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(objectLocation)!, "System.Runtime.dll"));

        foreach (PortableExecutableReference reference in new[]
        {
            MetadataReference.CreateFromFile(objectLocation),
            systemRuntimeReference
        })
        {
            _references.Add(reference);
        }

        return this;
    }

    public CompilationBuilder WithLoggerFactoryReference()
    {
        PortableExecutableReference loggerFactoryReference = MetadataReference.CreateFromFile(typeof(ILoggerFactory).Assembly.Location);
        _references.Add(loggerFactoryReference);

        return this;
    }

    public CompilationBuilder WithJsonApiDotNetCoreReferences()
    {
        foreach (PortableExecutableReference reference in new[]
        {
            MetadataReference.CreateFromFile(typeof(ControllerBase).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(AttrAttribute).Assembly.Location)
        })
        {
            _references.Add(reference);
        }

        return this;
    }

    public CompilationBuilder WithSourceCode(string source)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
        _syntaxTrees.Add(syntaxTree);
        return this;
    }
}
