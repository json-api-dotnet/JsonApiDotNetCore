using System.Text;
using JetBrains.Annotations;

#pragma warning disable CA1001 // Types that own disposable fields should be disposable

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
        ArgumentNullException.ThrowIfNull(name);

        _stack.Push($"/{EscapeSegment(name)}");
        return _disposable;
    }

    /// <summary>
    /// Escapes a member name for use in a JSON Pointer (RFC 6901): '~' becomes '~0' and '/' becomes '~1'.
    /// </summary>
    internal static string EscapeSegment(string name)
    {
        return name.Replace("~", "~0", StringComparison.Ordinal).Replace("/", "~1", StringComparison.Ordinal);
    }

    public IDisposable PushArrayIndex(int index)
    {
        _stack.Push($"/{index}");
        return _disposable;
    }

    public string? ToSourcePointer()
    {
        if (_stack.Count == 0)
        {
            return null;
        }

        var builder = new StringBuilder();
        var clone = new Stack<string>(_stack);

        while (clone.Count > 0)
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

    private sealed class PopStackOnDispose(RequestAdapterPosition owner) : IDisposable
    {
        private readonly RequestAdapterPosition _owner = owner;

        public void Dispose()
        {
            _owner._stack.Pop();
        }
    }
}
