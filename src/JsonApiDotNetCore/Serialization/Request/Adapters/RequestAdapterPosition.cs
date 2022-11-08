using System.Text;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Serialization.Request.Adapters;

/// <summary>
/// Tracks the location within an object tree when validating and converting a request body.
/// </summary>
[PublicAPI]
public sealed class RequestAdapterPosition
{
    private readonly Stack<string> _stack = new();
    private readonly IDisposable _disposable;

    public RequestAdapterPosition()
    {
        _disposable = new PopStackOnDispose(this);
    }

    public IDisposable PushElement(string name)
    {
        ArgumentGuard.NotNullNorEmpty(name);

        _stack.Push($"/{name}");
        return _disposable;
    }

    public IDisposable PushArrayIndex(int index)
    {
        _stack.Push($"[{index}]");
        return _disposable;
    }

    public string? ToSourcePointer()
    {
        if (!_stack.Any())
        {
            return null;
        }

        var builder = new StringBuilder();
        var clone = new Stack<string>(_stack);

        while (clone.Any())
        {
            string element = clone.Pop();
            builder.Append(element);
        }

        return builder.ToString();
    }

    public override string ToString()
    {
        return ToSourcePointer() ?? string.Empty;
    }

    private sealed class PopStackOnDispose : IDisposable
    {
        private readonly RequestAdapterPosition _owner;

        public PopStackOnDispose(RequestAdapterPosition owner)
        {
            _owner = owner;
        }

        public void Dispose()
        {
            _owner._stack.Pop();
        }
    }
}
