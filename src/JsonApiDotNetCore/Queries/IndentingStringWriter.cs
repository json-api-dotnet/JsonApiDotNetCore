using System.Text;

namespace JsonApiDotNetCore.Queries;

internal sealed class IndentingStringWriter : IDisposable
{
    private readonly StringBuilder _builder;

    private int _indentDepth;

    public IndentingStringWriter(StringBuilder builder)
    {
        _builder = builder;
    }

    public void WriteLine(string? line)
    {
        if (_indentDepth > 0)
        {
            _builder.Append(new string(' ', _indentDepth * 2));
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
