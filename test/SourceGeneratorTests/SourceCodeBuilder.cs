using System.Text;

namespace SourceGeneratorTests;

internal sealed class SourceCodeBuilder
{
    private readonly HashSet<string> _namespaceImports = [];
    private string? _namespace;
    private string? _code;

    public MarkedSourceCode Build()
    {
        StringBuilder builder = new();

        if (_namespaceImports.Count > 0)
        {
            foreach (string namespaceImport in _namespaceImports)
            {
                builder.AppendLine($"using {namespaceImport};");
            }

            builder.AppendLine();
        }

        if (_namespace != null)
        {
            builder.AppendLine($"namespace {_namespace};");
            builder.AppendLine();
        }

        if (_code != null)
        {
            builder.Append(_code);
        }

        string text = builder.ToString();
        return new MarkedSourceCode(text);
    }

    public SourceCodeBuilder WithNamespaceImportFor(Type type)
    {
        _namespaceImports.Add(type.Namespace!);
        return this;
    }

    public SourceCodeBuilder InNamespace(string @namespace)
    {
        _namespace = @namespace;
        return this;
    }

    public SourceCodeBuilder WithCode(string code)
    {
        _code = code;
        return this;
    }
}
