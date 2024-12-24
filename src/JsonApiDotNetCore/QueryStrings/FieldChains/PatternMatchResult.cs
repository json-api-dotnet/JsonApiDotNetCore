using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.QueryStrings.FieldChains;

/// <summary>
/// Represents the result of matching a dot-separated resource field chain against a pattern.
/// </summary>
[PublicAPI]
public sealed class PatternMatchResult
{
    /// <summary>
    /// Indicates whether the match succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// The resolved field chain, when <see cref="IsSuccess" /> is <c>true</c>.
    /// </summary>
    /// <remarks>
    /// The chain may be empty, if the pattern allows for that.
    /// </remarks>
    public IReadOnlyList<ResourceFieldAttribute> FieldChain { get; }

    /// <summary>
    /// Gets the match failure message, when <see cref="IsSuccess" /> is <c>false</c>.
    /// </summary>
    public string FailureMessage { get; }

    /// <summary>
    /// Gets the zero-based position in the resource field chain, or at its end, where the match failure occurred.
    /// </summary>
    public int FailurePosition { get; }

    /// <summary>
    /// Indicates whether the match failed due to an invalid field chain, irrespective of greedy matching.
    /// </summary>
    public bool IsFieldChainError { get; }

    private PatternMatchResult(bool isSuccess, IReadOnlyList<ResourceFieldAttribute> fieldChain, string failureMessage, int failurePosition,
        bool isFieldChainError)
    {
        IsSuccess = isSuccess;
        FieldChain = fieldChain;
        FailureMessage = failureMessage;
        FailurePosition = failurePosition;
        IsFieldChainError = isFieldChainError;
    }

    internal static PatternMatchResult CreateForSuccess(IReadOnlyList<ResourceFieldAttribute> fieldChain)
    {
        ArgumentNullException.ThrowIfNull(fieldChain);

        return new PatternMatchResult(true, fieldChain, string.Empty, -1, false);
    }

    internal static PatternMatchResult CreateForFailure(MatchError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new PatternMatchResult(false, Array.Empty<ResourceFieldAttribute>(), error.Message, error.Position, error.IsFieldChainError);
    }
}
