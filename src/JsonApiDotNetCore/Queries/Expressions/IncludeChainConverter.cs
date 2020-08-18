using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Models.Annotation;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Converts includes between tree and chain formats.
    /// Exists for backwards compatibility, subject to be removed in the future.
    /// </summary>
    internal static class IncludeChainConverter
    {
        /// <summary>
        /// Converts a tree of inclusions into a set of relationship chains.
        /// </summary>
        /// <example>
        /// Input tree:
        /// Article
        /// {
        ///   Blog,
        ///   Revisions
        ///   {
        ///     Author
        ///   }
        /// }
        ///
        /// Output chains:
        /// Article -> Blog,
        /// Article -> Revisions -> Author
        /// </example>
        public static IReadOnlyCollection<ResourceFieldChainExpression> GetRelationshipChains(IncludeExpression include)
        {
            if (include == null)
            {
                throw new ArgumentNullException(nameof(include));
            }

            IncludeToChainsConverter converter = new IncludeToChainsConverter();
            converter.Visit(include, null);

            return converter.Chains;
        }

        /// <summary>
        /// Converts a set of relationship chains into a tree of inclusions.
        /// </summary>
        /// <example>
        /// Input chains:
        /// Article -> Blog,
        /// Article -> Revisions -> Author
        ///
        /// Output tree:
        /// Article
        /// {
        ///   Blog,
        ///   Revisions
        ///   {
        ///     Author
        ///   }
        /// }
        /// </example>
        public static IncludeExpression FromRelationshipChains(IReadOnlyCollection<ResourceFieldChainExpression> chains)
        {
            if (chains == null)
            {
                throw new ArgumentNullException(nameof(chains));
            }

            var elements = ConvertChainsToElements(chains);
            return elements.Any() ? new IncludeExpression(elements) : IncludeExpression.Empty;
        }

        private static IReadOnlyCollection<IncludeElementExpression> ConvertChainsToElements(IReadOnlyCollection<ResourceFieldChainExpression> chains)
        {
            var rootNode = new MutableIncludeNode(null);

            foreach (ResourceFieldChainExpression chain in chains)
            {
                MutableIncludeNode currentNode = rootNode;

                foreach (var relationship in chain.Fields.OfType<RelationshipAttribute>())
                {
                    if (!currentNode.Children.ContainsKey(relationship))
                    {
                        currentNode.Children[relationship] = new MutableIncludeNode(relationship);
                    }

                    currentNode = currentNode.Children[relationship];
                }
            }

            return rootNode.Children.Values.Select(child => child.ToExpression()).ToArray();
        }

        private sealed class IncludeToChainsConverter : QueryExpressionVisitor<object, object>
        {
            private readonly Stack<RelationshipAttribute> _parentRelationshipStack = new Stack<RelationshipAttribute>();

            public List<ResourceFieldChainExpression> Chains { get; } = new List<ResourceFieldChainExpression>();

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
                List<RelationshipAttribute> fieldsInChain = _parentRelationshipStack.Reverse().ToList();
                fieldsInChain.Add(expression.Relationship);

                Chains.Add(new ResourceFieldChainExpression(fieldsInChain));
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
                var elementChildren = Children.Values.Select(child => child.ToExpression()).ToArray();
                return new IncludeElementExpression(_relationship, elementChildren);
            }
        }
    }
}
