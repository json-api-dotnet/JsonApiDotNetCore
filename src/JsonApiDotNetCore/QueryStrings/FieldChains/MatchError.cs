using System.Text;
using JsonApiDotNetCore.Configuration;

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
        return new MatchError(exception.Message, exception.Position, true);
    }

    public static MatchError CreateForUnknownField(int position, ResourceType? resourceType, string publicName, bool allowDerivedTypes)
    {
        bool hasDerivedTypes = allowDerivedTypes && resourceType != null && resourceType.DirectlyDerivedTypes.Any();

        var builder = new MessageBuilder();

        builder.WriteDoesNotExist(publicName);
        builder.WriteResourceType(resourceType);
        builder.WriteOrDerivedTypes(hasDerivedTypes);
        builder.WriteEnd();

        string message = builder.ToString();
        return new MatchError(message, position, true);
    }

    public static MatchError CreateForMultipleDerivedTypes(int position, ResourceType resourceType, string publicName)
    {
        string message = $"Field '{publicName}' is defined on multiple types that derive from resource type '{resourceType}'.";
        return new MatchError(message, position, true);
    }

    public static MatchError CreateForFieldTypeMismatch(int position, ResourceType? resourceType, FieldTypes choices)
    {
        var builder = new MessageBuilder();

        builder.WriteChoices(choices);
        builder.WriteResourceType(resourceType);
        builder.WriteExpected();
        builder.WriteEnd();

        string message = builder.ToString();
        return new MatchError(message, position, false);
    }

    public static MatchError CreateForTooMuchInput(int position, ResourceType? resourceType, FieldTypes choices)
    {
        var builder = new MessageBuilder();

        builder.WriteEndOfChain();

        if (choices != FieldTypes.None)
        {
            builder.WriteOr();
            builder.WriteChoices(choices);
            builder.WriteResourceType(resourceType);
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

        public void WriteResourceType(ResourceType? resourceType)
        {
            if (resourceType != null)
            {
                _builder.Append($" on resource type '{resourceType}'");
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
