using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.QueryStrings.FieldChains;

/// <summary>
/// Matches a resource field chain against a pattern.
/// </summary>
internal sealed class PatternMatcher
{
    private readonly FieldChainPattern _pattern;
    private readonly ILogger<PatternMatcher> _logger;
    private readonly bool _allowDerivedTypes;

    public PatternMatcher(FieldChainPattern pattern, FieldChainPatternMatchOptions options, ILogger<PatternMatcher> logger)
    {
        ArgumentGuard.NotNull(pattern);
        ArgumentGuard.NotNull(logger);

        _pattern = pattern;
        _logger = logger;
        _allowDerivedTypes = options.HasFlag(FieldChainPatternMatchOptions.AllowDerivedTypes);
    }

    public PatternMatchResult Match(string fieldChain, ResourceType resourceType)
    {
        ArgumentGuard.NotNull(fieldChain);
        ArgumentGuard.NotNull(resourceType);

        var startState = MatchState.Create(_pattern, fieldChain, resourceType);

        if (startState.Error != null)
        {
            return PatternMatchResult.CreateForFailure(startState.Error);
        }

        using var traceScope = MatchTraceScope.CreateRoot(startState, _logger);

        MatchState endState = MatchPattern(startState, traceScope);
        traceScope.SetResult(endState);

        return endState.Error == null
            ? PatternMatchResult.CreateForSuccess(endState.GetAllFieldsMatched())
            : PatternMatchResult.CreateForFailure(endState.Error);
    }

    /// <summary>
    /// Matches the first segment in <see cref="MatchState.Pattern" /> against <see cref="MatchState.FieldsRemaining" />.
    /// </summary>
    private MatchState MatchPattern(MatchState state, MatchTraceScope parentTraceScope)
    {
        AssertIsSuccess(state);

        FieldChainPattern? patternSegment = state.Pattern;
        using MatchTraceScope traceScope = parentTraceScope.CreateChild(state);

        if (patternSegment == null)
        {
            MatchState endState = state.FieldsRemaining == null ? state : state.FailureForTooMuchInput();
            traceScope.LogMatchResult(endState);
            traceScope.SetResult(endState);

            return endState;
        }

        // Build a stack of successful matches against this pattern segment, incrementally trying to match more fields.
        Stack<MatchState> backtrackStack = new();

        if (!patternSegment.AtLeastOne)
        {
            // Also include match against empty chain, which always succeeds.
            traceScope.LogMatchResult(state);
            backtrackStack.Push(state);
        }

        MatchState greedyState = state;

        do
        {
            if (!patternSegment.AtLeastOne && greedyState.FieldsRemaining == null)
            {
                // Already added above.
                continue;
            }

            greedyState = MatchField(greedyState);
            traceScope.LogMatchResult(greedyState);

            if (greedyState.Error == null)
            {
                backtrackStack.Push(greedyState);
            }
        }
        while (!patternSegment.AtMostOne && greedyState is { FieldsRemaining: not null, Error: null });

        // The best error to return is the failure from matching the remaining pattern chain at the most-greedy successful match.
        // If matching against the remaining pattern chains doesn't fail, use the most-greedy failure itself.
        MatchState bestErrorEndState = greedyState;

        // Evaluate the stacked matches (greedy, so longest first) against the remaining pattern chain.
        while (backtrackStack.Count > 0)
        {
            MatchState backtrackState = backtrackStack.Pop();

            if (backtrackState != greedyState)
            {
                // If we're at to most-recent match, and it succeeded, then we're not really backtracking.
                traceScope.LogBacktrackTo(backtrackState);
            }

            // Match the remaining pattern chain against the remaining field chain.
            MatchState endState = MatchPattern(backtrackState.SuccessMoveToNextPattern(), traceScope);

            if (endState.Error == null)
            {
                traceScope.SetResult(endState);
                return endState;
            }

            if (bestErrorEndState == greedyState)
            {
                bestErrorEndState = endState;
            }
        }

        if (greedyState.Error?.IsFieldChainError == true)
        {
            // There was an error in the field chain itself, irrespective of backtracking.
            // It is therefore more relevant to report over any other error.
            bestErrorEndState = greedyState;
        }

        traceScope.SetResult(bestErrorEndState);
        return bestErrorEndState;
    }

    private static void AssertIsSuccess(MatchState state)
    {
        if (state.Error != null)
        {
            throw new InvalidOperationException($"Internal error: Expected successful match, but found error: {state.Error}");
        }
    }

    /// <summary>
    /// Matches the first remaining field against the set of choices in the current pattern segment.
    /// </summary>
    private MatchState MatchField(MatchState state)
    {
        FieldTypes choices = state.Pattern!.Choices;
        ResourceFieldAttribute? chosenField = null;

        if (state.FieldsRemaining != null)
        {
            string publicName = state.FieldsRemaining.Value;

            HashSet<ResourceFieldAttribute> fields = LookupFields(state.ResourceType, publicName);

            if (!fields.Any())
            {
                return state.FailureForUnknownField(publicName, _allowDerivedTypes);
            }

            chosenField = fields.First();

            fields.RemoveWhere(field => !IsTypeMatch(field, choices));

            if (fields.Count == 1)
            {
                return state.SuccessMoveForwardOneField(fields.First());
            }

            if (fields.Count > 1)
            {
                return state.FailureForMultipleDerivedTypes(publicName);
            }
        }

        FieldTypes chosenFieldType = GetFieldType(chosenField);
        return state.FailureForFieldTypeMismatch(choices, chosenFieldType);
    }

    /// <summary>
    /// Lookup the specified field in the resource graph.
    /// </summary>
    private HashSet<ResourceFieldAttribute> LookupFields(ResourceType? resourceType, string publicName)
    {
        HashSet<ResourceFieldAttribute> fields = [];

        if (resourceType != null)
        {
            if (_allowDerivedTypes)
            {
                IReadOnlySet<AttrAttribute> attributes = resourceType.GetAttributesInTypeOrDerived(publicName);
                fields.UnionWith(attributes);

                IReadOnlySet<RelationshipAttribute> relationships = resourceType.GetRelationshipsInTypeOrDerived(publicName);
                fields.UnionWith(relationships);
            }
            else
            {
                AttrAttribute? attribute = resourceType.FindAttributeByPublicName(publicName);

                if (attribute != null)
                {
                    fields.Add(attribute);
                }

                RelationshipAttribute? relationship = resourceType.FindRelationshipByPublicName(publicName);

                if (relationship != null)
                {
                    fields.Add(relationship);
                }
            }
        }

        return fields;
    }

    private static bool IsTypeMatch(ResourceFieldAttribute field, FieldTypes types)
    {
        FieldTypes chosenType = GetFieldType(field);

        return (types & chosenType) != FieldTypes.None;
    }

    private static FieldTypes GetFieldType(ResourceFieldAttribute? field)
    {
        return field switch
        {
            HasManyAttribute => FieldTypes.ToManyRelationship,
            HasOneAttribute => FieldTypes.ToOneRelationship,
            RelationshipAttribute => FieldTypes.Relationship,
            AttrAttribute => FieldTypes.Attribute,
            null => FieldTypes.None,
            _ => FieldTypes.Field
        };
    }
}
