using System.Text;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.QueryStrings.FieldChains;

/// <summary>
/// Indicates a failure to match a pattern against a resource field chain.
/// </summary>
internal sealed class MatchError
{
    /// <summary>
    /// Gets the match failure message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the zero-based position in the resource field chain, or at its end, where the failure occurred.
    /// </summary>
    public int Position { get; }

    /// <summary>
    /// Indicates whether this error occurred due to an invalid field chain, irrespective of greedy matching.
    /// </summary>
    public bool IsFieldChainError { get; }

    private MatchError(string message, int position, bool isFieldChainError)
    {
        Message = message;
        Position = position;
        IsFieldChainError = isFieldChainError;
    }

    public static MatchError CreateForBrokenFieldChain(FieldChainFormatException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return new MatchError(exception.Message, exception.Position, true);
    }

    public static MatchError CreateForUnknownField(int position, FieldContainer container, string publicName, bool allowDerivedTypes)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(publicName);

        bool hasDerivedTypes = allowDerivedTypes && container.Type is { DirectlyDerivedTypes.Count: > 0 };

        var builder = new MessageBuilder();

        builder.WriteDoesNotExist(publicName);
        builder.WriteContainer(container);
        builder.WriteOrDerivedTypes(hasDerivedTypes);
        builder.WriteEnd();

        string message = builder.ToString();
        return new MatchError(message, position, true);
    }

    public static MatchError CreateForMultipleDerivedTypes(int position, FieldContainer container, string publicName)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(publicName);

        string message = $"Field '{publicName}' is defined on multiple types that derive from {container}.";
        return new MatchError(message, position, true);
    }

    public static MatchError CreateForFieldTypeMismatch(int position, FieldContainer container, FieldTypes choices)
    {
        ArgumentNullException.ThrowIfNull(container);

        var builder = new MessageBuilder();

        builder.WriteChoices(choices);
        builder.WriteContainer(container);
        builder.WriteExpected();
        builder.WriteEnd();

        string message = builder.ToString();
        return new MatchError(message, position, false);
    }

    public static MatchError CreateForTooMuchInput(int position, FieldContainer? container, FieldTypes choices)
    {
        var builder = new MessageBuilder();

        builder.WriteEndOfChain();

        if (choices != FieldTypes.None)
        {
            builder.WriteOr();
            builder.WriteChoices(choices);
            builder.WriteContainer(container);
        }

        builder.WriteExpected();
        builder.WriteEnd();

        string message = builder.ToString();
        return new MatchError(message, position, false);
    }

    public override string ToString()
    {
        return Message;
    }

    private sealed class MessageBuilder
    {
        private readonly StringBuilder _builder = new();

        public void WriteDoesNotExist(string publicName)
        {
            _builder.Append($"Field '{publicName}' does not exist");
        }

        public void WriteOrDerivedTypes(bool hasDerivedTypes)
        {
            if (hasDerivedTypes)
            {
                _builder.Append(" or any of its derived types");
            }
        }

        public void WriteEndOfChain()
        {
            _builder.Append("End of field chain");
        }

        public void WriteOr()
        {
            _builder.Append(" or ");
        }

        public void WriteChoices(FieldTypes choices)
        {
            bool firstCharToUpper = _builder.Length == 0;
            choices.WriteTo(_builder, false, false);

            if (firstCharToUpper && _builder.Length > 0)
            {
                _builder[0] = char.ToUpperInvariant(_builder[0]);
            }
        }

        public void WriteContainer(FieldContainer? container)
        {
            if (container != null)
            {
                if (container.Attribute == null || container.Attribute.Kind == AttrKind.Compound || container.Attribute.Kind == AttrKind.CollectionOfCompound)
                {
                    _builder.Append($" on {container}");
                }
            }
        }

        public void WriteExpected()
        {
            _builder.Append(" expected");
        }

        public void WriteEnd()
        {
            _builder.Append('.');
        }

        public override string ToString()
        {
            return _builder.ToString();
        }
    }
}
