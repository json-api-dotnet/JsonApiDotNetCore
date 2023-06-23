using System.Collections.Immutable;
using System.Text;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal.Parsing;

/// <summary>
/// Parses the JSON:API 'include' query string parameter value.
/// </summary>
[PublicAPI]
public class IncludeParser : QueryExpressionParser
{
    public IncludeExpression Parse(string source, ResourceType resourceType, int? maximumDepth)
    {
        ArgumentGuard.NotNull(resourceType);

        Tokenize(source);

        IncludeExpression expression = ParseInclude(source, resourceType, maximumDepth);

        AssertTokenStackIsEmpty();
        ValidateMaximumIncludeDepth(maximumDepth, expression, 0);

        return expression;
    }

    protected IncludeExpression ParseInclude(string source, ResourceType resourceType, int? maximumDepth)
    {
        var treeRoot = IncludeTreeNode.CreateRoot(resourceType);
        bool isAtStart = true;

        while (TokenStack.Any())
        {
            if (!isAtStart)
            {
                EatSingleCharacterToken(TokenKind.Comma);
            }
            else
            {
                isAtStart = false;
            }

            ParseRelationshipChain(source, treeRoot);
        }

        return treeRoot.ToExpression();
    }

    private void ParseRelationshipChain(string source, IncludeTreeNode treeRoot)
    {
        // A relationship name usually matches a single relationship, even when overridden in derived types.
        // But in the following case, two relationships are matched on GET /shoppingBaskets?include=items:
        //
        // public abstract class ShoppingBasket : Identifiable<long>
        // {
        // }
        //
        // public sealed class SilverShoppingBasket : ShoppingBasket
        // {
        //     [HasMany]
        //     public ISet<Article> Items { get; get; }
        // }
        //
        // public sealed class PlatinumShoppingBasket : ShoppingBasket
        // {
        //     [HasMany]
        //     public ISet<Product> Items { get; get; }
        // }
        //
        // Now if the include chain has subsequent relationships, we need to scan both Items relationships for matches,
        // which is why ParseRelationshipName returns a collection.
        //
        // The advantage of this unfolding is we don't require callers to upcast in relationship chains. The downside is
        // that there's currently no way to include Products without Articles. We could add such optional upcast syntax
        // in the future, if desired.

        ICollection<IncludeTreeNode> children = ParseRelationshipName(source, treeRoot.AsList());

        while (TokenStack.TryPeek(out Token? nextToken) && nextToken.Kind == TokenKind.Period)
        {
            EatSingleCharacterToken(TokenKind.Period);

            children = ParseRelationshipName(source, children);
        }
    }

    private ICollection<IncludeTreeNode> ParseRelationshipName(string source, ICollection<IncludeTreeNode> parents)
    {
        int position = GetNextTokenPositionOrEnd();

        if (TokenStack.TryPop(out Token? token) && token.Kind == TokenKind.Text)
        {
            return LookupRelationshipName(token.Value!, parents, source, position);
        }

        throw new QueryParseException("Relationship name expected.", position);
    }

    private ICollection<IncludeTreeNode> LookupRelationshipName(string relationshipName, ICollection<IncludeTreeNode> parents, string source, int position)
    {
        List<IncludeTreeNode> children = new();
        HashSet<RelationshipAttribute> relationshipsFound = new();

        foreach (IncludeTreeNode parent in parents)
        {
            // Depending on the left side of the include chain, we may match relationships anywhere in the resource type hierarchy.
            // This is compensated for when rendering the response, which substitutes relationships on base types with the derived ones.
            IReadOnlySet<RelationshipAttribute> relationships = parent.Relationship.RightType.GetRelationshipsInTypeOrDerived(relationshipName);

            if (relationships.Any())
            {
                relationshipsFound.UnionWith(relationships);

                RelationshipAttribute[] relationshipsToInclude = relationships.Where(relationship => !relationship.IsIncludeBlocked()).ToArray();
                ICollection<IncludeTreeNode> affectedChildren = parent.EnsureChildren(relationshipsToInclude);
                children.AddRange(affectedChildren);
            }
        }

        AssertRelationshipsFound(relationshipsFound, relationshipName, parents, position);
        AssertAtLeastOneCanBeIncluded(relationshipsFound, relationshipName, source, position);

        return children;
    }

    private static void AssertRelationshipsFound(ISet<RelationshipAttribute> relationshipsFound, string relationshipName, ICollection<IncludeTreeNode> parents,
        int position)
    {
        if (relationshipsFound.Any())
        {
            return;
        }

        ResourceType[] parentResourceTypes = parents.Select(parent => parent.Relationship.RightType).Distinct().ToArray();

        bool hasDerivedTypes = parents.Any(parent => parent.Relationship.RightType.DirectlyDerivedTypes.Count > 0);

        string message = GetErrorMessageForNoneFound(relationshipName, parentResourceTypes, hasDerivedTypes);
        throw new QueryParseException(message, position);
    }

    private static string GetErrorMessageForNoneFound(string relationshipName, ICollection<ResourceType> parentResourceTypes, bool hasDerivedTypes)
    {
        var builder = new StringBuilder($"Relationship '{relationshipName}'");

        if (parentResourceTypes.Count == 1)
        {
            builder.Append($" does not exist on resource type '{parentResourceTypes.First().PublicName}'");
        }
        else
        {
            string typeNames = string.Join(", ", parentResourceTypes.Select(type => $"'{type.PublicName}'"));
            builder.Append($" does not exist on any of the resource types {typeNames}");
        }

        builder.Append(hasDerivedTypes ? " or any of its derived types." : ".");

        return builder.ToString();
    }

    private static void AssertAtLeastOneCanBeIncluded(ISet<RelationshipAttribute> relationshipsFound, string relationshipName, string source, int position)
    {
        if (relationshipsFound.All(relationship => relationship.IsIncludeBlocked()))
        {
            ResourceType resourceType = relationshipsFound.First().LeftType;
            string message = $"Including the relationship '{relationshipName}' on '{resourceType}' is not allowed.";

            var exception = new QueryParseException(message, position);
            string specificMessage = exception.GetMessageWithPosition(source);

            throw new InvalidQueryStringParameterException("include", "The specified include is invalid.", specificMessage);
        }
    }

    private static void ValidateMaximumIncludeDepth(int? maximumDepth, IncludeExpression include, int position)
    {
        if (maximumDepth != null)
        {
            Stack<RelationshipAttribute> parentChain = new();

            foreach (IncludeElementExpression element in include.Elements)
            {
                ThrowIfMaximumDepthExceeded(element, parentChain, maximumDepth.Value, position);
            }
        }
    }

    private static void ThrowIfMaximumDepthExceeded(IncludeElementExpression includeElement, Stack<RelationshipAttribute> parentChain, int maximumDepth,
        int position)
    {
        parentChain.Push(includeElement.Relationship);

        if (parentChain.Count > maximumDepth)
        {
            string path = string.Join('.', parentChain.Reverse().Select(relationship => relationship.PublicName));
            throw new QueryParseException($"Including '{path}' exceeds the maximum inclusion depth of {maximumDepth}.", position);
        }

        foreach (IncludeElementExpression child in includeElement.Children)
        {
            ThrowIfMaximumDepthExceeded(child, parentChain, maximumDepth, position);
        }

        parentChain.Pop();
    }

    private sealed class IncludeTreeNode
    {
        private readonly IDictionary<RelationshipAttribute, IncludeTreeNode> _children = new Dictionary<RelationshipAttribute, IncludeTreeNode>();

        public RelationshipAttribute Relationship { get; }

        private IncludeTreeNode(RelationshipAttribute relationship)
        {
            Relationship = relationship;
        }

        public static IncludeTreeNode CreateRoot(ResourceType resourceType)
        {
            var relationship = new HiddenRootRelationshipAttribute(resourceType);
            return new IncludeTreeNode(relationship);
        }

        public ICollection<IncludeTreeNode> EnsureChildren(ICollection<RelationshipAttribute> relationships)
        {
            foreach (RelationshipAttribute relationship in relationships)
            {
                if (!_children.ContainsKey(relationship))
                {
                    var newChild = new IncludeTreeNode(relationship);
                    _children.Add(relationship, newChild);
                }
            }

            return _children.Where(pair => relationships.Contains(pair.Key)).Select(pair => pair.Value).ToList();
        }

        public IncludeExpression ToExpression()
        {
            IncludeElementExpression element = ToElementExpression();

            if (element.Relationship is HiddenRootRelationshipAttribute)
            {
                return element.Children.Any() ? new IncludeExpression(element.Children) : IncludeExpression.Empty;
            }

            return new IncludeExpression(ImmutableHashSet.Create(element));
        }

        private IncludeElementExpression ToElementExpression()
        {
            IImmutableSet<IncludeElementExpression> elementChildren = _children.Values.Select(child => child.ToElementExpression()).ToImmutableHashSet();
            return new IncludeElementExpression(Relationship, elementChildren);
        }

        public override string ToString()
        {
            IncludeExpression include = ToExpression();
            return include.ToFullString();
        }

        private sealed class HiddenRootRelationshipAttribute : RelationshipAttribute
        {
            public HiddenRootRelationshipAttribute(ResourceType rightType)
            {
                ArgumentGuard.NotNull(rightType);

                RightType = rightType;
                PublicName = "<<root>>";
            }
        }
    }
}
