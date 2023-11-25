using Microsoft.Extensions.Logging;

#pragma warning disable CA2254 // Template should be a static expression

namespace JsonApiDotNetCore.QueryStrings.FieldChains;

/// <summary>
/// Logs the pattern matching steps at <see cref="LogLevel.Trace" /> level.
/// </summary>
internal sealed class MatchTraceScope : IDisposable
{
    private readonly FieldChainPattern? _pattern;
    private readonly bool _isEnabled;
    private readonly ILogger _logger;
    private readonly int _indentDepth;
    private MatchState? _endState;

    private MatchTraceScope(FieldChainPattern? pattern, bool isEnabled, ILogger logger, int indentDepth)
    {
        _pattern = pattern;
        _isEnabled = isEnabled;
        _logger = logger;
        _indentDepth = indentDepth;
    }

    public static MatchTraceScope CreateRoot(MatchState startState, ILogger logger)
    {
        ArgumentGuard.NotNull(startState);
        ArgumentGuard.NotNull(logger);

        bool isEnabled = logger.IsEnabled(LogLevel.Trace);

        if (isEnabled)
        {
            string fieldsRemaining = FormatFieldsRemaining(startState);
            string message = $"Start matching pattern '{startState.Pattern}' against the complete chain '{fieldsRemaining}'";
            logger.LogTrace(message);
        }

        return new MatchTraceScope(startState.Pattern, isEnabled, logger, 0);
    }

    public MatchTraceScope CreateChild(MatchState startState)
    {
        ArgumentGuard.NotNull(startState);

        int indentDepth = _indentDepth + 1;
        FieldChainPattern? patternSegment = startState.Pattern?.WithoutNext();

        if (_isEnabled)
        {
            string fieldsRemaining = FormatFieldsRemaining(startState);
            LogMessage($"Start matching '{patternSegment}' against the remaining chain '{fieldsRemaining}'");
        }

        return new MatchTraceScope(patternSegment, _isEnabled, _logger, indentDepth);
    }

    public void LogMatchResult(MatchState resultState)
    {
        ArgumentGuard.NotNull(resultState);

        if (_isEnabled)
        {
            if (resultState.Error == null)
            {
                string fieldsMatched = FormatFieldsMatched(resultState);
                LogMessage($"Match '{_pattern}' against '{fieldsMatched}': Success");
            }
            else
            {
                List<string> chain = new(resultState.FieldsMatched.Select(attribute => attribute.PublicName));

                if (resultState.FieldsRemaining != null)
                {
                    chain.Add(resultState.FieldsRemaining.Value);
                }

                string chainText = string.Join('.', chain);
                LogMessage($"Match '{_pattern}' against '{chainText}': Failed");
            }
        }
    }

    public void LogBacktrackTo(MatchState backtrackState)
    {
        ArgumentGuard.NotNull(backtrackState);

        if (_isEnabled)
        {
            string fieldsMatched = FormatFieldsMatched(backtrackState);
            LogMessage($"Backtracking to successful match against '{fieldsMatched}'");
        }
    }

    public void SetResult(MatchState endState)
    {
        ArgumentGuard.NotNull(endState);

        _endState = endState;
    }

    public void Dispose()
    {
        if (_endState == null)
        {
            throw new InvalidOperationException("Internal error: End state must be set before leaving trace scope.");
        }

        if (_isEnabled)
        {
            LogMessage(_endState.Error == null ? "Matching completed with success" : "Matching completed with failure");
        }
    }

    private static string FormatFieldsRemaining(MatchState state)
    {
        return string.Join('.', state.FieldsRemaining.ToEnumerable());
    }

    private static string FormatFieldsMatched(MatchState state)
    {
        return string.Join('.', state.FieldsMatched);
    }

    private void LogMessage(string message)
    {
        string indent = new(' ', _indentDepth * 2);
        _logger.LogTrace($"{indent}{message}");
    }
}
