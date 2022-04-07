using System.Collections.Immutable;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions;

/// <summary>
/// Converts includes between tree and chain formats. Exists for backwards compatibility, subject to be removed in the future.
/// </summary>
internal sealed class IncludeChainConverter
{
    /// <summary>
    /// Converts a tree of inclusions into a set of relationship chains.
    /// </summary>
    /// <example>
    /// Input tree: <code><![CDATA[
    /// Article
    /// {
    ///   Blog,
    ///   Revisions
    ///   {
    ///     Author
    ///   }
    /// }
    /// ]]></code> Output chains:
    /// <code><![CDATA[
    /// Article -> Blog,
    /// Article -> Revisions -> Author
    /// ]]></code>
    /// </example>
    public IReadOnlyCollection<ResourceFieldChainExpression> GetRelationshipChains(IncludeExpression include)
    {
        ArgumentGuard.NotNull(include, nameof(include));

        if (!include.Elements.Any())
        {
            return Array.Empty<ResourceFieldChainExpression>();
        }

        var converter = new IncludeToChainsConverter();
        converter.Visit(include, null);

        return converter.Chains;
    }

    private sealed class IncludeToChainsConverter : QueryExpressionVisitor<object?, object?>
    {
        private readonly Stack<RelationshipAttribute> _parentRelationshipStack = new();

        public List<ResourceFieldChainExpression> Chains { get; } = new();

        public override object? VisitInclude(IncludeExpression expression, object? argument)
        {
            foreach (IncludeElementExpression element in expression.Elements)
            {
                Visit(element, null);
            }

            return null;
        }

        public override object? VisitIncludeElement(IncludeElementExpression expression, object? argument)
        {
            if (!expression.Children.Any())
            {
                FlushChain(expression);
            }
            else
            {
                _parentRelationshipStack.Push(expression.Relationship);

                foreach (IncludeElementExpression child in expression.Children)
                {
                    Visit(child, null);
                }

                _parentRelationshipStack.Pop();
            }

            return null;
        }

        private void FlushChain(IncludeElementExpression expression)
        {
            ImmutableArray<ResourceFieldAttribute>.Builder chainBuilder =
                ImmutableArray.CreateBuilder<ResourceFieldAttribute>(_parentRelationshipStack.Count + 1);

            chainBuilder.AddRange(_parentRelationshipStack.Reverse());
            chainBuilder.Add(expression.Relationship);

            Chains.Add(new ResourceFieldChainExpression(chainBuilder.ToImmutable()));
        }
    }
}
