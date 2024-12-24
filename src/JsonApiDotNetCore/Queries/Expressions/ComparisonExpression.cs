using Humanizer;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions;

/// <summary>
/// This expression allows to compare two operands using a comparison operator. It represents comparison filter functions, resulting from text such as:
/// <c>
/// equals(name,'Joe')
/// </c>
/// ,
/// <c>
/// equals(owner,null)
/// </c>
/// , or:
/// <c>
/// greaterOrEqual(count(upVotes),count(downVotes),'1')
/// </c>
/// .
/// </summary>
[PublicAPI]
public class ComparisonExpression : FilterExpression
{
    /// <summary>
    /// The operator used to compare <see cref="Left" /> and <see cref="Right" />.
    /// </summary>
    public ComparisonOperator Operator { get; }

    /// <summary>
    /// The left-hand operand, which can be a function or a field chain. Chain format: an optional list of to-one relationships, followed by an attribute.
    /// When comparing equality with null, the chain may also end in a to-one relationship.
    /// </summary>
    public QueryExpression Left { get; }

    /// <summary>
    /// The right-hand operand, which can be a function, a field chain, a constant, or null (if the type of <see cref="Left" /> is nullable). Chain format:
    /// an optional list of to-one relationships, followed by an attribute.
    /// </summary>
    public QueryExpression Right { get; }

    public ComparisonExpression(ComparisonOperator @operator, QueryExpression left, QueryExpression right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        Operator = @operator;
        Left = left;
        Right = right;
    }

    public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
    {
        return visitor.VisitComparison(this, argument);
    }

    public override string ToString()
    {
        return $"{Operator.ToString().Camelize()}({Left},{Right})";
    }

    public override string ToFullString()
    {
        return $"{Operator.ToString().Camelize()}({Left.ToFullString()},{Right.ToFullString()})";
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

        var other = (ComparisonExpression)obj;

        return Operator == other.Operator && Left.Equals(other.Left) && Right.Equals(other.Right);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Operator, Left, Right);
    }
}
