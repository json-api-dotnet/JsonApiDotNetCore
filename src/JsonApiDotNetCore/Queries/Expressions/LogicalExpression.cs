using System.Collections.Immutable;
using System.Text;
using Humanizer;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions;

/// <summary>
/// This expression allows to test whether one or all of its boolean operands are true. It represents the logical AND/OR filter functions, resulting from
/// text such as:
/// <c>
/// and(equals(title,'Work'),has(articles))
/// </c>
/// , or:
/// <c>
/// or(equals(title,'Work'),has(articles))
/// </c>
/// .
/// </summary>
[PublicAPI]
public class LogicalExpression : FilterExpression
{
    /// <summary>
    /// The operator used to compare <see cref="Terms" />.
    /// </summary>
    public LogicalOperator Operator { get; }

    /// <summary>
    /// The list of one or more boolean operands.
    /// </summary>
    public IImmutableList<FilterExpression> Terms { get; }

    public LogicalExpression(LogicalOperator @operator, params FilterExpression[] terms)
        : this(@operator, terms.ToImmutableArray())
    {
    }

    public LogicalExpression(LogicalOperator @operator, IImmutableList<FilterExpression> terms)
    {
        ArgumentNullException.ThrowIfNull(terms);

        if (terms.Count < 2)
        {
            throw new ArgumentException("At least two terms are required.", nameof(terms));
        }

        Operator = @operator;
        Terms = terms;
    }

    public static FilterExpression? Compose(LogicalOperator @operator, params FilterExpression?[] filters)
    {
        ArgumentNullException.ThrowIfNull(filters);

        // Workaround for https://youtrack.jetbrains.com/issue/RSRP-496512/Invalid-Use-collection-expression-suggestion.
        // ReSharper disable once UseCollectionExpression
        ImmutableArray<FilterExpression> terms = filters.WhereNotNull().ToImmutableArray();

        return terms.Length > 1 ? new LogicalExpression(@operator, terms) : terms.FirstOrDefault();
    }

    public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitLogical(this, argument);
    }

    public override string ToString()
    {
        return InnerToString(false);
    }

    public override string ToFullString()
    {
        return InnerToString(true);
    }

    private string InnerToString(bool toFullString)
    {
        var builder = new StringBuilder();

        builder.Append(Operator.ToString().Camelize());
        builder.Append('(');
        builder.Append(string.Join(",", Terms.Select(term => toFullString ? term.ToFullString() : term.ToString())));
        builder.Append(')');

        return builder.ToString();
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is null || GetType() != obj.GetType())
        {
            return false;
        }

        var other = (LogicalExpression)obj;

        return Operator == other.Operator && Terms.SequenceEqual(other.Terms);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Operator);

        foreach (QueryExpression term in Terms)
        {
            hashCode.Add(term);
        }

        return hashCode.ToHashCode();
    }
}
