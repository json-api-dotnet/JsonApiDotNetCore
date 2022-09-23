using System.Collections.Immutable;
using System.Text;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal.Parsing;

[PublicAPI]
public class IncludeParser : QueryExpressionParser
{
    private static readonly ResourceFieldChainErrorFormatter ErrorFormatter = new();

    public IncludeExpression Parse(string source, ResourceType resourceTypeInScope, int? maximumDepth)
    {
        ArgumentGuard.NotNull(resourceTypeInScope);

        Tokenize(source);

        IncludeExpression expression = ParseInclude(resourceTypeInScope, maximumDepth);

        AssertTokenStackIsEmpty();
        ValidateMaximumIncludeDepth(maximumDepth, expression);

        return expression;
    }

    protected IncludeExpression ParseInclude(ResourceType resourceTypeInScope, int? maximumDepth)
    {
        var treeRoot = IncludeTreeNode.CreateRoot(resourceTypeInScope);

        ParseRelationshipChain(treeRoot);

        while (TokenStack.Any())
        {
            EatSingleCharacterToken(TokenKind.Comma);

            ParseRelationshipChain(treeRoot);
        }

        return treeRoot.ToExpression();
    }

    private void ParseRelationshipChain(IncludeTreeNode treeRoot)
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

        ICollection<IncludeTreeNode> children = ParseRelationshipName(treeRoot.AsList());

        while (TokenStack.TryPeek(out Token? nextToken) && nextToken.Kind == TokenKind.Period)
        {
            EatSingleCharacterToken(TokenKind.Period);

            children = ParseRelationshipName(children);
        }
    }

    private ICollection<IncludeTreeNode> ParseRelationshipName(ICollection<IncludeTreeNode> parents)
    {
        if (TokenStack.TryPop(out Token? token) && token.Kind == TokenKind.Text)
        {
            return LookupRelationshipName(token.Value!, parents);
        }

        throw new QueryParseException("Relationship name expected.");
    }

    private ICollection<IncludeTreeNode> LookupRelationshipName(string relationshipName, ICollection<IncludeTreeNode> parents)
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
                relationshipsFound.AddRange(relationships);

                RelationshipAttribute[] relationshipsToInclude = relationships.Where(relationship => relationship.CanInclude).ToArray();
                ICollection<IncludeTreeNode> affectedChildren = parent.EnsureChildren(relationshipsToInclude);
                children.AddRange(affectedChildren);
            }
        }

        AssertRelationshipsFound(relationshipsFound, relationshipName, parents);
        AssertAtLeastOneCanBeIncluded(relationshipsFound, relationshipName, parents);

        return children;
    }

    private static void AssertRelationshipsFound(ISet<RelationshipAttribute> relationshipsFound, string relationshipName, ICollection<IncludeTreeNode> parents)
    {
        if (relationshipsFound.Any())
        {
            return;
        }

        string[] parentPaths = parents.Select(parent => parent.Path).Distinct().Where(path => path != string.Empty).ToArray();
        string path = parentPaths.Length > 0 ? $"{parentPaths[0]}.{relationshipName}" : relationshipName;

        ResourceType[] parentResourceTypes = parents.Select(parent => parent.Relationship.RightType).Distinct().ToArray();

        bool hasDerivedTypes = parents.Any(parent => parent.Relationship.RightType.DirectlyDerivedTypes.Count > 0);

        string message = ErrorFormatter.GetForNoneFound(ResourceFieldCategory.Relationship, relationshipName, path, parentResourceTypes, hasDerivedTypes);
        throw new QueryParseException(message);
    }

    private static void AssertAtLeastOneCanBeIncluded(ISet<RelationshipAttribute> relationshipsFound, string relationshipName,
        ICollection<IncludeTreeNode> parents)
    {
        if (relationshipsFound.All(relationship => !relationship.CanInclude))
        {
            string parentPath = parents.First().Path;
            ResourceType resourceType = relationshipsFound.First().LeftType;

            string message = parentPath == string.Empty
                ? $"Including the relationship '{relationshipName}' on '{resourceType}' is not allowed."
                : $"Including the relationship '{relationshipName}' in '{parentPath}.{relationshipName}' on '{resourceType}' is not allowed.";

            throw new InvalidQueryStringParameterException("include", "Including the requested relationship is not allowed.", message);
        }
    }

    private static void ValidateMaximumIncludeDepth(int? maximumDepth, IncludeExpression include)
    {
        if (maximumDepth != null)
        {
            Stack<RelationshipAttribute> parentChain = new();

            foreach (IncludeElementExpression element in include.Elements)
            {
                ThrowIfMaximumDepthExceeded(element, parentChain, maximumDepth.Value);
            }
        }
    }

    private static void ThrowIfMaximumDepthExceeded(IncludeElementExpression includeElement, Stack<RelationshipAttribute> parentChain, int maximumDepth)
    {
        parentChain.Push(includeElement.Relationship);

        if (parentChain.Count > maximumDepth)
        {
            string path = string.Join('.', parentChain.Reverse().Select(relationship => relationship.PublicName));
            throw new QueryParseException($"Including '{path}' exceeds the maximum inclusion depth of {maximumDepth}.");
        }

        foreach (IncludeElementExpression child in includeElement.Children)
        {
            ThrowIfMaximumDepthExceeded(child, parentChain, maximumDepth);
        }

        parentChain.Pop();
    }

    protected override IImmutableList<ResourceFieldAttribute> OnResolveFieldChain(string path, FieldChainRequirements chainRequirements)
    {
        throw new NotSupportedException();
    }

    private sealed class IncludeTreeNode
    {
        private readonly IncludeTreeNode? _parent;
        private readonly IDictionary<RelationshipAttribute, IncludeTreeNode> _children = new Dictionary<RelationshipAttribute, IncludeTreeNode>();

        public RelationshipAttribute Relationship { get; }

        public string Path
        {
            get
            {
                var pathBuilder = new StringBuilder();
                IncludeTreeNode? parent = this;

                while (parent is { Relationship: not HiddenRootRelationshipAttribute })
                {
                    pathBuilder.Insert(0, pathBuilder.Length > 0 ? $"{parent.Relationship.PublicName}." : parent.Relationship.PublicName);
                    parent = parent._parent;
                }

                return pathBuilder.ToString();
            }
        }

        private IncludeTreeNode(RelationshipAttribute relationship, IncludeTreeNode? parent)
        {
            Relationship = relationship;
            _parent = parent;
        }

        public static IncludeTreeNode CreateRoot(ResourceType resourceType)
        {
            var relationship = new HiddenRootRelationshipAttribute(resourceType);
            return new IncludeTreeNode(relationship, null);
        }

        public ICollection<IncludeTreeNode> EnsureChildren(ICollection<RelationshipAttribute> relationships)
        {
            foreach (RelationshipAttribute relationship in relationships)
            {
                if (!_children.ContainsKey(relationship))
                {
                    var newChild = new IncludeTreeNode(relationship, this);
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
                return new IncludeExpression(element.Children);
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
