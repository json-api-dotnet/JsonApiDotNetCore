using System.Collections.Immutable;
using System.Text;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.QueryStrings.FieldChains;

/// <summary>
/// Immutable intermediate state, used while matching a resource field chain against a pattern.
/// </summary>
internal sealed class MatchState
{
    /// <summary>
    /// The successful parent match. Chaining together <see cref="FieldsMatched" /> with those of parents produces the full match.
    /// </summary>
    private readonly MatchState? _parentMatch;

    /// <summary>
    /// The remaining chain of pattern segments. The first segment is being matched against.
    /// </summary>
    public FieldChainPattern? Pattern { get; }

    /// <summary>
    /// The resource type to find the next field on.
    /// </summary>
    public ResourceType? ResourceType { get; }

    /// <summary>
    /// The fields matched against this pattern segment.
    /// </summary>
    public IImmutableList<ResourceFieldAttribute> FieldsMatched { get; }

    /// <summary>
    /// The remaining fields to be matched against the remaining pattern chain.
    /// </summary>
    public LinkedListNode<string>? FieldsRemaining { get; }

    /// <summary>
    /// The error in case matching this pattern segment failed.
    /// </summary>
    public MatchError? Error { get; }

    private MatchState(FieldChainPattern? pattern, ResourceType? resourceType, IImmutableList<ResourceFieldAttribute> fieldsMatched,
        LinkedListNode<string>? fieldsRemaining, MatchError? error, MatchState? parentMatch)
    {
        Pattern = pattern;
        ResourceType = resourceType;
        FieldsMatched = fieldsMatched;
        FieldsRemaining = fieldsRemaining;
        Error = error;
        _parentMatch = parentMatch;
    }

    public static MatchState Create(FieldChainPattern pattern, string fieldChainText, ResourceType resourceType)
    {
        ArgumentGuard.NotNull(pattern);
        ArgumentGuard.NotNull(fieldChainText);
        ArgumentGuard.NotNull(resourceType);

        try
        {
            var parser = new FieldChainParser();
            IEnumerable<string> fieldChain = parser.Parse(fieldChainText);

            LinkedListNode<string>? remainingHead = new LinkedList<string>(fieldChain).First;
            return new MatchState(pattern, resourceType, ImmutableArray<ResourceFieldAttribute>.Empty, remainingHead, null, null);
        }
        catch (FieldChainFormatException exception)
        {
            var error = MatchError.CreateForBrokenFieldChain(exception);
            return new MatchState(pattern, resourceType, ImmutableArray<ResourceFieldAttribute>.Empty, null, error, null);
        }
    }

    /// <summary>
    /// Returns a new state for successfully matching the top-level remaining field. Moves one position forward in the resource field chain.
    /// </summary>
    public MatchState SuccessMoveForwardOneField(ResourceFieldAttribute matchedValue)
    {
        ArgumentGuard.NotNull(matchedValue);
        AssertIsSuccess(this);

        IImmutableList<ResourceFieldAttribute> fieldsMatched = FieldsMatched.Add(matchedValue);
        LinkedListNode<string>? fieldsRemaining = FieldsRemaining!.Next;
        ResourceType? resourceType = matchedValue is RelationshipAttribute relationship ? relationship.RightType : null;

        return new MatchState(Pattern, resourceType, fieldsMatched, fieldsRemaining, null, _parentMatch);
    }

    /// <summary>
    /// Returns a new state for matching the next pattern segment.
    /// </summary>
    public MatchState SuccessMoveToNextPattern()
    {
        AssertIsSuccess(this);
        AssertHasPattern();

        return new MatchState(Pattern!.Next, ResourceType, ImmutableArray<ResourceFieldAttribute>.Empty, FieldsRemaining, null, this);
    }

    /// <summary>
    /// Returns a new state for match failure due to an unknown field.
    /// </summary>
    public MatchState FailureForUnknownField(string publicName, bool allowDerivedTypes)
    {
        int position = GetAbsolutePosition(true);
        var error = MatchError.CreateForUnknownField(position, ResourceType, publicName, allowDerivedTypes);

        return Failure(error);
    }

    /// <summary>
    /// Returns a new state for match failure because the field exists on multiple derived types.
    /// </summary>
    public MatchState FailureForMultipleDerivedTypes(string publicName)
    {
        AssertHasResourceType();

        int position = GetAbsolutePosition(true);
        var error = MatchError.CreateForMultipleDerivedTypes(position, ResourceType!, publicName);

        return Failure(error);
    }

    /// <summary>
    /// Returns a new state for match failure because the field type is not one of the pattern choices.
    /// </summary>
    public MatchState FailureForFieldTypeMismatch(FieldTypes choices, FieldTypes chosenFieldType)
    {
        FieldTypes allChoices = IncludeChoicesFromParentMatch(choices);
        int position = GetAbsolutePosition(chosenFieldType != FieldTypes.None);
        var error = MatchError.CreateForFieldTypeMismatch(position, ResourceType, allChoices);

        return Failure(error);
    }

    /// <summary>
    /// Combines the choices of this pattern segment with choices from parent matches, if they can match more.
    /// </summary>
    private FieldTypes IncludeChoicesFromParentMatch(FieldTypes choices)
    {
        if (choices == FieldTypes.Field)
        {
            // We already match everything, there's no point in looking deeper.
            return choices;
        }

        if (_parentMatch is { Pattern: not null })
        {
            // The choices from the parent pattern segment are available when:
            // - The parent pattern can match multiple times.
            // - The parent pattern is optional and matched nothing.
            if (!_parentMatch.Pattern.AtMostOne || (!_parentMatch.Pattern.AtLeastOne && _parentMatch.FieldsMatched.Count == 0))
            {
                FieldTypes mergedChoices = choices | _parentMatch.Pattern.Choices;

                // If the parent pattern didn't match anything, look deeper.
                if (_parentMatch.FieldsMatched.Count == 0)
                {
                    mergedChoices = _parentMatch.IncludeChoicesFromParentMatch(mergedChoices);
                }

                return mergedChoices;
            }
        }

        return choices;
    }

    /// <summary>
    /// Returns a new state for match failure because the resource field chain contains more fields than expected.
    /// </summary>
    public MatchState FailureForTooMuchInput()
    {
        FieldTypes parentChoices = IncludeChoicesFromParentMatch(FieldTypes.None);
        int position = GetAbsolutePosition(true);
        var error = MatchError.CreateForTooMuchInput(position, _parentMatch?.ResourceType, parentChoices);

        return Failure(error);
    }

    private MatchState Failure(MatchError error)
    {
        return new MatchState(Pattern, ResourceType, FieldsMatched, FieldsRemaining, error, _parentMatch);
    }

    private int GetAbsolutePosition(bool hasLeadingDot)
    {
        int length = 0;
        MatchState? currentState = this;

        while (currentState != null)
        {
            length += currentState.FieldsMatched.Sum(field => field.PublicName.Length + 1);
            currentState = currentState._parentMatch;
        }

        length = length > 0 ? length - 1 : 0;

        if (length > 0 && hasLeadingDot)
        {
            length++;
        }

        return length;
    }

    public override string ToString()
    {
        var builder = new StringBuilder();

        if (FieldsMatched.Count == 0 && FieldsRemaining == null && Pattern == null)
        {
            builder.Append("EMPTY");
        }
        else
        {
            builder.Append(Error == null ? "SUCCESS: " : "FAILED: ");
            builder.Append("Matched '");
            builder.Append(string.Join('.', FieldsMatched));
            builder.Append("' against '");
            builder.Append(Pattern?.WithoutNext());
            builder.Append("' with remaining '");
            builder.Append(string.Join('.', FieldsRemaining.ToEnumerable()));
            builder.Append('\'');
        }

        if (_parentMatch != null)
        {
            builder.Append(" -> ");
            builder.Append(_parentMatch);
        }

        return builder.ToString();
    }

    public IReadOnlyList<ResourceFieldAttribute> GetAllFieldsMatched()
    {
        Stack<IImmutableList<ResourceFieldAttribute>> matchStack = new();
        MatchState? current = this;

        while (current != null)
        {
            matchStack.Push(current.FieldsMatched);
            current = current._parentMatch;
        }

        List<ResourceFieldAttribute> fields = [];

        while (matchStack.Count > 0)
        {
            IImmutableList<ResourceFieldAttribute> matches = matchStack.Pop();
            fields.AddRange(matches);
        }

        return fields;
    }

    private static void AssertIsSuccess(MatchState state)
    {
        if (state.Error != null)
        {
            throw new InvalidOperationException($"Internal error: Expected successful match, but found error: {state.Error}");
        }
    }

    private void AssertHasResourceType()
    {
        if (ResourceType == null)
        {
            throw new InvalidOperationException("Internal error: Resource type is unavailable.");
        }
    }

    private void AssertHasPattern()
    {
        if (Pattern == null)
        {
            throw new InvalidOperationException("Internal error: Pattern chain is unavailable.");
        }
    }
}
