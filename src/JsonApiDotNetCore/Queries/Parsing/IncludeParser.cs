using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Text;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Parsing;

/// <inheritdoc cref="IIncludeParser" />
[PublicAPI]
public class IncludeParser : QueryExpressionParser, IIncludeParser
{
    private readonly IJsonApiOptions _options;

    public IncludeParser(IJsonApiOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
    }

    /// <inheritdoc />
    public IncludeExpression Parse(string source, ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        Tokenize(source);

        IncludeExpression expression = ParseInclude(source, resourceType);

        AssertTokenStackIsEmpty();
        ValidateMaximumIncludeDepth(expression, 0);

        return expression;
    }

    protected virtual IncludeExpression ParseInclude(string source, ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(resourceType);

        var treeRoot = IncludeTreeNode.CreateRoot(resourceType);
        bool isAtStart = true;

        while (TokenStack.Count > 0)
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

        ReadOnlyCollection<IncludeTreeNode> children = ParseRelationshipName(source, [treeRoot]);

        while (TokenStack.TryPeek(out Token? nextToken) && nextToken.Kind == TokenKind.Period)
        {
            EatSingleCharacterToken(TokenKind.Period);

            children = ParseRelationshipName(source, children);
        }
    }

    private ReadOnlyCollection<IncludeTreeNode> ParseRelationshipName(string source, IReadOnlyCollection<IncludeTreeNode> parents)
    {
        int position = GetNextTokenPositionOrEnd();

        if (TokenStack.TryPop(out Token? token) && token.Kind == TokenKind.Text)
        {
            return LookupRelationshipName(token.Value!, parents, source, position);
        }

        throw new QueryParseException("Relationship name expected.", position);
    }

    private static ReadOnlyCollection<IncludeTreeNode> LookupRelationshipName(string relationshipName, IReadOnlyCollection<IncludeTreeNode> parents,
        string source, int position)
    {
        List<IncludeTreeNode> children = [];
        HashSet<RelationshipAttribute> relationshipsFound = [];

        foreach (IncludeTreeNode parent in parents)
        {
            // Depending on the left side of the include chain, we may match relationships anywhere in the resource type hierarchy.
            // This is compensated for when rendering the response, which substitutes relationships on base types with the derived ones.
            HashSet<RelationshipAttribute> relationships = GetRelationshipsInConcreteTypes(parent.Relationship.RightType, relationshipName);

            if (relationships.Count > 0)
            {
                relationshipsFound.UnionWith(relationships);

                RelationshipAttribute[] relationshipsToInclude = relationships.Where(relationship => !relationship.IsIncludeBlocked()).ToArray();
                ReadOnlyCollection<IncludeTreeNode> affectedChildren = parent.EnsureChildren(relationshipsToInclude);
                children.AddRange(affectedChildren);
            }
        }

        AssertRelationshipsFound(relationshipsFound, relationshipName, parents, position);
        AssertAtLeastOneCanBeIncluded(relationshipsFound, relationshipName, source, position);

        return children.AsReadOnly();
    }

    private static HashSet<RelationshipAttribute> GetRelationshipsInConcreteTypes(ResourceType resourceType, string relationshipName)
    {
        HashSet<RelationshipAttribute> relationshipsToInclude = [];

        foreach (RelationshipAttribute relationship in resourceType.GetRelationshipsInTypeOrDerived(relationshipName))
        {
            if (!relationship.LeftType.ClrType.IsAbstract)
            {
                relationshipsToInclude.Add(relationship);
            }

            IncludeRelationshipsFromConcreteDerivedTypes(relationship, relationshipsToInclude);
        }

        return relationshipsToInclude;
    }

    private static void IncludeRelationshipsFromConcreteDerivedTypes(RelationshipAttribute relationship, HashSet<RelationshipAttribute> relationshipsToInclude)
    {
        foreach (ResourceType derivedType in relationship.LeftType.GetAllConcreteDerivedTypes())
        {
            RelationshipAttribute relationshipInDerived = derivedType.GetRelationshipByPublicName(relationship.PublicName);
            relationshipsToInclude.Add(relationshipInDerived);
        }
    }

    private static void AssertRelationshipsFound(HashSet<RelationshipAttribute> relationshipsFound, string relationshipName,
        IReadOnlyCollection<IncludeTreeNode> parents, int position)
    {
        if (relationshipsFound.Count > 0)
        {
            return;
        }

        ResourceType[] parentResourceTypes = parents.Select(parent => parent.Relationship.RightType).Distinct().ToArray();

        bool hasDerivedTypes = parents.Any(parent => parent.Relationship.RightType.DirectlyDerivedTypes.Count > 0);

        string message = GetErrorMessageForNoneFound(relationshipName, parentResourceTypes, hasDerivedTypes);
        throw new QueryParseException(message, position);
    }

    private static string GetErrorMessageForNoneFound(string relationshipName, ResourceType[] parentResourceTypes, bool hasDerivedTypes)
    {
        var builder = new StringBuilder($"Relationship '{relationshipName}'");

        if (parentResourceTypes.Length == 1)
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

    private static void AssertAtLeastOneCanBeIncluded(HashSet<RelationshipAttribute> relationshipsFound, string relationshipName, string source, int position)
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

    private void ValidateMaximumIncludeDepth(IncludeExpression include, int position)
    {
        if (_options.MaximumIncludeDepth != null)
        {
            int maximumDepth = _options.MaximumIncludeDepth.Value;
            Stack<RelationshipAttribute> parentChain = new();

            foreach (IncludeElementExpression element in include.Elements)
            {
                ThrowIfMaximumDepthExceeded(element, parentChain, maximumDepth, position);
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
        private readonly Dictionary<RelationshipAttribute, IncludeTreeNode> _children = [];

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

        public ReadOnlyCollection<IncludeTreeNode> EnsureChildren(RelationshipAttribute[] relationships)
        {
            foreach (RelationshipAttribute relationship in relationships)
            {
                if (!_children.ContainsKey(relationship))
                {
                    var newChild = new IncludeTreeNode(relationship);
                    _children.Add(relationship, newChild);
                }
            }

            return _children.Where(pair => relationships.Contains(pair.Key)).Select(pair => pair.Value).ToArray().AsReadOnly();
        }

        public IncludeExpression ToExpression()
        {
            IncludeElementExpression element = ToElementExpression();

            if (element.Relationship is HiddenRootRelationshipAttribute)
            {
                return element.Children.Count > 0 ? new IncludeExpression(element.Children) : IncludeExpression.Empty;
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
                ArgumentNullException.ThrowIfNull(rightType);

                RightType = rightType;
                PublicName = "<<root>>";
            }
        }
    }
}
