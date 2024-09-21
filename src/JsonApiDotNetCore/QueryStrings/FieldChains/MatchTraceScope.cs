using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.QueryStrings.FieldChains;

/// <summary>
/// Logs the pattern matching steps at <see cref="LogLevel.Trace" /> level.
/// </summary>
internal sealed partial class MatchTraceScope : IDisposable
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
        var scope = new MatchTraceScope(startState.Pattern, isEnabled, logger, 0);

        if (isEnabled)
        {
            string fieldsRemaining = FormatFieldsRemaining(startState);
            scope.LogMatchFirst(startState.Pattern, fieldsRemaining);
        }

        return scope;
    }

    public MatchTraceScope CreateChild(MatchState startState)
    {
        ArgumentGuard.NotNull(startState);

        int indentDepth = _indentDepth + 1;
        FieldChainPattern? patternSegment = startState.Pattern?.WithoutNext();

        if (_isEnabled)
        {
            string indent = GetIndentText();
            string fieldsRemaining = FormatFieldsRemaining(startState);
            LogMatchNext(indent, patternSegment, fieldsRemaining);
        }

        return new MatchTraceScope(patternSegment, _isEnabled, _logger, indentDepth);
    }

    public void LogMatchResult(MatchState resultState)
    {
        ArgumentGuard.NotNull(resultState);

        if (_isEnabled)
        {
            string indent = GetIndentText();

            if (resultState.Error == null)
            {
                string fieldsMatched = FormatFieldsMatched(resultState);
                LogMatchSuccess(indent, _pattern, fieldsMatched);
            }
            else
            {
                List<string> chain = [.. resultState.FieldsMatched.Select(attribute => attribute.PublicName)];

                if (resultState.FieldsRemaining != null)
                {
                    chain.Add(resultState.FieldsRemaining.Value);
                }

                string chainText = string.Join('.', chain);
                LogMatchFailed(indent, _pattern, chainText);
            }
        }
    }

    public void LogBacktrackTo(MatchState backtrackState)
    {
        ArgumentGuard.NotNull(backtrackState);

        if (_isEnabled)
        {
            string indent = GetIndentText();
            string fieldsMatched = FormatFieldsMatched(backtrackState);
            LogBacktrack(indent, fieldsMatched);
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
            string indent = GetIndentText();

            if (_endState.Error == null)
            {
                LogCompletionSuccess(indent);
            }
            else
            {
                LogCompletionFailure(indent);
            }
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

    private string GetIndentText()
    {
        return new string(' ', _indentDepth * 2);
    }

    [LoggerMessage(Level = LogLevel.Trace, SkipEnabledCheck = true, Message = "Start matching pattern '{Pattern}' against the complete chain '{Chain}'.")]
    private partial void LogMatchFirst(FieldChainPattern? pattern, string chain);

    [LoggerMessage(Level = LogLevel.Trace, SkipEnabledCheck = true,
        Message = "{Indent}Start matching pattern '{Pattern}' against the remaining chain '{Chain}'.")]
    private partial void LogMatchNext(string indent, FieldChainPattern? pattern, string chain);

    [LoggerMessage(Level = LogLevel.Trace, SkipEnabledCheck = true, Message = "{Indent}Match pattern '{Pattern}' against the chain '{Chain}': Success.")]
    private partial void LogMatchSuccess(string indent, FieldChainPattern? pattern, string chain);

    [LoggerMessage(Level = LogLevel.Trace, SkipEnabledCheck = true, Message = "{Indent}Match pattern '{Pattern}' against the chain '{Chain}': Failed.")]
    private partial void LogMatchFailed(string indent, FieldChainPattern? pattern, string chain);

    [LoggerMessage(Level = LogLevel.Trace, SkipEnabledCheck = true, Message = "{Indent}Backtracking to successful match against '{Chain}'.")]
    private partial void LogBacktrack(string indent, string chain);

    [LoggerMessage(Level = LogLevel.Trace, SkipEnabledCheck = true, Message = "{Indent}Matching completed with success.")]
    private partial void LogCompletionSuccess(string indent);

    [LoggerMessage(Level = LogLevel.Trace, SkipEnabledCheck = true, Message = "{Indent}Matching completed with failure.")]
    private partial void LogCompletionFailure(string indent);
}
