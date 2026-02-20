using System.Text;

namespace JsonApiDotNetCore.Queries;

internal sealed class IndentingStringWriter(StringBuilder builder) : IDisposable
{
    private readonly StringBuilder _builder = builder;

    private int _indentDepth;

    public void WriteLine(string? line)
    {
        if (_indentDepth > 0)
        {
            _builder.Append(' ', _indentDepth * 2);
        }

        _builder.AppendLine(line);
    }

    public IndentingStringWriter Indent()
    {
        WriteLine("{");
        _indentDepth++;
        return this;
    }

    public void Dispose()
    {
        if (_indentDepth > 0)
        {
            _indentDepth--;
            WriteLine("}");
        }
    }
}
