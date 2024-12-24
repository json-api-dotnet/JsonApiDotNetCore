using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace JsonApiDotNetCore.QueryStrings.FieldChains;

/// <summary>
/// A pattern that can be matched against a dot-separated resource field chain.
/// </summary>
[PublicAPI]
public sealed class FieldChainPattern
{
    /// <summary>
    /// Gets the set of possible resource field types.
    /// </summary>
    internal FieldTypes Choices { get; }

    /// <summary>
    /// Indicates whether this pattern segment must match at least one resource field.
    /// </summary>
    internal bool AtLeastOne { get; }

    /// <summary>
    /// Indicates whether this pattern can match multiple resource fields.
    /// </summary>
    internal bool AtMostOne { get; }

    /// <summary>
    /// Gets the next pattern segment in the chain, or <c>null</c> if at the end.
    /// </summary>
    internal FieldChainPattern? Next { get; }

    internal FieldChainPattern(FieldTypes choices, bool atLeastOne, bool atMostOne, FieldChainPattern? next)
    {
        if (choices == FieldTypes.None)
        {
            throw new ArgumentException("The set of choices cannot be empty.", nameof(choices));
        }

        Choices = choices;
        AtLeastOne = atLeastOne;
        AtMostOne = atMostOne;
        Next = next;
    }

    /// <summary>
    /// Creates a pattern from the specified text that can be matched against.
    /// </summary>
    /// <remarks>
    /// Patterns are similar to regular expressions, but a lot simpler. They consist of a sequence of terms. A term can be a single character or a character
    /// choice. A term is optionally followed by a quantifier.
    /// <p>
    /// The following characters can be used:
    /// <list type="table">
    /// <item>
    /// <term>M</term>
    /// <description>
    /// Matches a to-many relationship.
    /// </description>
    /// </item>
    /// <item>
    /// <term>O</term>
    /// <description>
    /// Matches a to-one relationship.
    /// </description>
    /// </item>
    /// <item>
    /// <term>R</term>
    /// <description>
    /// Matches a relationship.
    /// </description>
    /// </item>
    /// <item>
    /// <term>A</term>
    /// <description>
    /// Matches an attribute.
    /// </description>
    /// </item>
    /// <item>
    /// <term>F</term>
    /// <description>
    /// Matches a field.
    /// </description>
    /// </item>
    /// </list>
    /// </p>
    /// <p>
    /// A character choice contains a set of characters, surrounded by brackets. One of the choices must match. For example, "[MO]" matches a relationship,
    /// but not at attribute.
    /// </p>
    /// A quantifier is used to indicate how many times its term directly to the left can occur.
    /// <list type="table">
    /// <item>
    /// <term>?</term>
    /// <description>
    /// Matches its preceding term zero or one times.
    /// </description>
    /// </item>
    /// <item>
    /// <term>*</term>
    /// <description>
    /// Matches its preceding term zero or more times.
    /// </description>
    /// </item>
    /// <item>
    /// <term>+</term>
    /// <description>
    /// Matches its preceding term one or more times.
    /// </description>
    /// </item>
    /// </list>
    /// <example>
    /// For example, the pattern "M?O*A" matches "children.parent.name", "parent.parent.name" and "name".
    /// </example>
    /// </remarks>
    /// <exception cref="PatternFormatException">
    /// The pattern is invalid.
    /// </exception>
    public static FieldChainPattern Parse(string pattern)
    {
        var parser = new PatternParser();
        return parser.Parse(pattern);
    }

    /// <summary>
    /// Matches the specified resource field chain against this pattern.
    /// </summary>
    /// <param name="fieldChain">
    /// The dot-separated chain of resource field names.
    /// </param>
    /// <param name="resourceType">
    /// The parent resource type to start matching from.
    /// </param>
    /// <param name="options">
    /// Match options, defaults to <see cref="FieldChainPatternMatchOptions.None" />.
    /// </param>
    /// <param name="loggerFactory">
    /// When provided, logs the matching steps at <see cref="LogLevel.Trace" /> level.
    /// </param>
    /// <returns>
    /// The match result.
    /// </returns>
    public PatternMatchResult Match(string fieldChain, ResourceType resourceType, FieldChainPatternMatchOptions options = FieldChainPatternMatchOptions.None,
        ILoggerFactory? loggerFactory = null)
    {
        ArgumentNullException.ThrowIfNull(fieldChain);
        ArgumentNullException.ThrowIfNull(resourceType);

        ILogger<PatternMatcher> logger = loggerFactory == null ? NullLogger<PatternMatcher>.Instance : loggerFactory.CreateLogger<PatternMatcher>();
        var matcher = new PatternMatcher(this, options, logger);
        return matcher.Match(fieldChain, resourceType);
    }

    /// <summary>
    /// Returns only the first segment of this pattern chain. Used for diagnostic messages.
    /// </summary>
    internal FieldChainPattern WithoutNext()
    {
        return Next == null ? this : new FieldChainPattern(Choices, AtLeastOne, AtMostOne, null);
    }

    /// <summary>
    /// Gets the text representation of this pattern.
    /// </summary>
    public override string ToString()
    {
        var formatter = new PatternTextFormatter(this);
        return formatter.Format();
    }

    /// <summary>
    /// Gets a human-readable description of this pattern.
    /// </summary>
    public string GetDescription()
    {
        var formatter = new PatternDescriptionFormatter(this);
        return formatter.Format();
    }
}
