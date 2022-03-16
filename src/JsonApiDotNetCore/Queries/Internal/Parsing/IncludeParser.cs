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
    public IncludeExpression Parse(string source, ResourceType resourceTypeInScope, int? maximumDepth)
    {
        ArgumentGuard.NotNull(resourceTypeInScope, nameof(resourceTypeInScope));

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
            ISet<RelationshipAttribute> relationships = GetRelationshipsInTypeOrDerived(parent.Relationship.RightType, relationshipName);

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

    private ISet<RelationshipAttribute> GetRelationshipsInTypeOrDerived(ResourceType resourceType, string relationshipName)
    {
        RelationshipAttribute? relationship = resourceType.FindRelationshipByPublicName(relationshipName);

        if (relationship != null)
        {
            return relationship.AsHashSet();
        }

        // Hiding base members using the 'new' keyword instead of 'override' (effectively breaking inheritance) is currently not supported.
        // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/knowing-when-to-use-override-and-new-keywords
        HashSet<RelationshipAttribute> relationshipsInDerivedTypes = new();

        foreach (ResourceType derivedType in resourceType.DirectlyDerivedTypes)
        {
            ISet<RelationshipAttribute> relationshipsInDerivedType = GetRelationshipsInTypeOrDerived(derivedType, relationshipName);
            relationshipsInDerivedTypes.AddRange(relationshipsInDerivedType);
        }

        return relationshipsInDerivedTypes;
    }

    private static void AssertRelationshipsFound(ISet<RelationshipAttribute> relationshipsFound, string relationshipName, ICollection<IncludeTreeNode> parents)
    {
        if (relationshipsFound.Any())
        {
            return;
        }

        var messageBuilder = new StringBuilder();
        messageBuilder.Append($"Relationship '{relationshipName}'");

        string[] parentPaths = parents.Select(parent => parent.Path).Distinct().Where(path => path != string.Empty).ToArray();

        if (parentPaths.Length > 0)
        {
            messageBuilder.Append($" in '{parentPaths[0]}.{relationshipName}'");
        }

        ResourceType[] parentResourceTypes = parents.Select(parent => parent.Relationship.RightType).Distinct().ToArray();

        if (parentResourceTypes.Length == 1)
        {
            messageBuilder.Append($" does not exist on resource type '{parentResourceTypes[0].PublicName}'");
        }
        else
        {
            string typeNames = string.Join(", ", parentResourceTypes.Select(type => $"'{type.PublicName}'"));
            messageBuilder.Append($" does not exist on any of the resource types {typeNames}");
        }

        bool hasDerived = parents.Any(parent => parent.Relationship.RightType.DirectlyDerivedTypes.Count > 0);
        messageBuilder.Append(hasDerived ? " or any of its derived types." : ".");

        throw new QueryParseException(messageBuilder.ToString());
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

                while (parent is { Relationship: not HiddenRootRelationship })
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
            var relationship = new HiddenRootRelationship(resourceType);
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

            if (element.Relationship is HiddenRootRelationship)
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

            IncludeChainConverter includeChainConverter = new();
            IReadOnlyCollection<ResourceFieldChainExpression> chains = includeChainConverter.GetRelationshipChains(include);

            IEnumerable<ResourceFieldChainExpression> chainsInOrder = InDisplayOrder(chains);
            return string.Join(",", chainsInOrder.Select(RelationshipChainToString));
        }

        private static IEnumerable<ResourceFieldChainExpression> InDisplayOrder(IEnumerable<ResourceFieldChainExpression> chains)
        {
            return chains.OrderBy(chain => string.Join('.',
                chain.Fields.OfType<RelationshipAttribute>().Select(relationship => relationship.PublicName + "@" + relationship.LeftType.PublicName)));
        }

        private static string RelationshipChainToString(ResourceFieldChainExpression chain)
        {
            var textBuilder = new StringBuilder();

            ResourceType? parentType = null;

            foreach (RelationshipAttribute relationship in chain.Fields.Cast<RelationshipAttribute>())
            {
                if (textBuilder.Length > 0)
                {
                    textBuilder.Append('.');
                }

                if (parentType == null || !parentType.Equals(relationship.LeftType))
                {
                    // This is not official syntax at this time, just helpful for debugging the parser.
                    textBuilder.Append('[');
                    textBuilder.Append(relationship.LeftType.PublicName);
                    textBuilder.Append(']');
                }

                textBuilder.Append(relationship.PublicName);

                parentType = relationship.RightType;
            }

            return textBuilder.ToString();
        }

        private sealed class HiddenRootRelationship : RelationshipAttribute
        {
            public HiddenRootRelationship(ResourceType rightType)
            {
                ArgumentGuard.NotNull(rightType, nameof(rightType));

                RightType = rightType;
                PublicName = "<<root>>";
            }
        }
    }
}
