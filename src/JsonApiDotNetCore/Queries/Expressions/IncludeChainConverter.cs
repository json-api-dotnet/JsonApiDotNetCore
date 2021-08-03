using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions
{
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

        /// <summary>
        /// Converts a set of relationship chains into a tree of inclusions.
        /// </summary>
        /// <example>
        /// Input chains: <code><![CDATA[
        /// Article -> Blog,
        /// Article -> Revisions -> Author
        /// ]]></code> Output tree:
        /// <code><![CDATA[
        /// Article
        /// {
        ///   Blog,
        ///   Revisions
        ///   {
        ///     Author
        ///   }
        /// }
        /// ]]></code>
        /// </example>
        public IncludeExpression FromRelationshipChains(IEnumerable<ResourceFieldChainExpression> chains)
        {
            ArgumentGuard.NotNull(chains, nameof(chains));

            IImmutableList<IncludeElementExpression> elements = ConvertChainsToElements(chains);
            return elements.Any() ? new IncludeExpression(elements) : IncludeExpression.Empty;
        }

        private static IImmutableList<IncludeElementExpression> ConvertChainsToElements(IEnumerable<ResourceFieldChainExpression> chains)
        {
            var rootNode = new MutableIncludeNode(null);

            foreach (ResourceFieldChainExpression chain in chains)
            {
                ConvertChainToElement(chain, rootNode);
            }

            return rootNode.Children.Values.Select(child => child.ToExpression()).ToImmutableArray();
        }

        private static void ConvertChainToElement(ResourceFieldChainExpression chain, MutableIncludeNode rootNode)
        {
            MutableIncludeNode currentNode = rootNode;

            foreach (RelationshipAttribute relationship in chain.Fields.OfType<RelationshipAttribute>())
            {
                if (!currentNode.Children.ContainsKey(relationship))
                {
                    currentNode.Children[relationship] = new MutableIncludeNode(relationship);
                }

                currentNode = currentNode.Children[relationship];
            }
        }

        private sealed class IncludeToChainsConverter : QueryExpressionVisitor<object, object>
        {
            private readonly Stack<RelationshipAttribute> _parentRelationshipStack = new();

            public List<ResourceFieldChainExpression> Chains { get; } = new();

            public override object VisitInclude(IncludeExpression expression, object argument)
            {
                foreach (IncludeElementExpression element in expression.Elements)
                {
                    Visit(element, null);
                }

                return null;
            }

            public override object VisitIncludeElement(IncludeElementExpression expression, object argument)
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

        private sealed class MutableIncludeNode
        {
            private readonly RelationshipAttribute _relationship;

            public IDictionary<RelationshipAttribute, MutableIncludeNode> Children { get; } = new Dictionary<RelationshipAttribute, MutableIncludeNode>();

            public MutableIncludeNode(RelationshipAttribute relationship)
            {
                _relationship = relationship;
            }

            public IncludeElementExpression ToExpression()
            {
                ImmutableArray<IncludeElementExpression> elementChildren = Children.Values.Select(child => child.ToExpression()).ToImmutableArray();
                return new IncludeElementExpression(_relationship, elementChildren);
            }
        }
    }
}
