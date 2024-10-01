using DapperExample.TranslationToSql.TreeNodes;

namespace DapperExample.TranslationToSql.Generators;

/// <summary>
/// Generates a SQL parameter with a unique name.
/// </summary>
internal sealed class ParameterGenerator
{
    private readonly ParameterNameGenerator _nameGenerator = new();

    public ParameterNode Create(object? value)
    {
        string name = _nameGenerator.GetNext();
        return new ParameterNode(name, value);
    }

    public void Reset()
    {
        _nameGenerator.Reset();
    }

    private sealed class ParameterNameGenerator()
        : UniqueNameGenerator("@p");
}
