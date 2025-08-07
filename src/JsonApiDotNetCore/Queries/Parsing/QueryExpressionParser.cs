using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Text;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.QueryStrings.FieldChains;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Parsing;

/// <summary>
/// The base class for parsing query string parameters, using the Recursive Descent algorithm.
/// </summary>
/// <remarks>
/// A tokenizer populates a stack of tokens from the source text, which is then recursively popped by various parsing routines. A
/// <see cref="QueryParseException" /> is expected to be thrown on invalid input.
/// </remarks>
[PublicAPI]
public abstract class QueryExpressionParser
{
    /// <summary>
    /// Contains the tokens produced from the source text, after <see cref="Tokenize" /> has been called.
    /// </summary>
    /// <remarks>
    /// The various parsing methods typically pop tokens while producing <see cref="QueryExpression" />s.
    /// </remarks>
    protected Stack<Token> TokenStack { get; private set; } = new();

    /// <summary>
    /// Contains the source text that tokens were produced from, after <see cref="Tokenize" /> has been called.
    /// </summary>
    protected string Source { get; private set; } = string.Empty;

    /// <summary>
    /// Enables derived types to throw a <see cref="QueryParseException" /> when usage of a JSON:API field inside a field chain is not permitted.
    /// </summary>
    protected virtual void ValidateField(ResourceFieldAttribute field, int position)
    {
    }

    /// <summary>
    /// Populates <see cref="TokenStack" /> from the source text using <see cref="QueryTokenizer" />.
    /// </summary>
    /// <remarks>
    /// To use a custom tokenizer, override this method and consider overriding <see cref="EatSingleCharacterToken" />.
    /// </remarks>
    protected virtual void Tokenize(string source)
    {
        ArgumentNullException.ThrowIfNull(source);

        Source = source;

        var tokenizer = new QueryTokenizer(source);
        TokenStack = new Stack<Token>(tokenizer.EnumerateTokens().Reverse());
    }

    /// <summary>
    /// Parses a dot-separated path of field names into a chain of resource fields, while matching it against the specified pattern.
    /// </summary>
    protected ResourceFieldChainExpression ParseFieldChain(FieldChainPattern pattern, FieldChainPatternMatchOptions options, ResourceType resourceType,
        string? alternativeErrorMessage)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        ArgumentNullException.ThrowIfNull(resourceType);

        int startPosition = GetNextTokenPositionOrEnd();

        string path = EatFieldChain(alternativeErrorMessage);
        PatternMatchResult result = pattern.Match(path, resourceType, options);

        if (!result.IsSuccess)
        {
            string message = result.IsFieldChainError
                ? result.FailureMessage
                : $"Field chain on resource type '{resourceType}' failed to match the pattern: {pattern.GetDescription()}. {result.FailureMessage}";

            throw new QueryParseException(message, startPosition + result.FailurePosition);
        }

        int chainPosition = 0;

        foreach (ResourceFieldAttribute field in result.FieldChain)
        {
            ValidateField(field, startPosition + chainPosition);

            chainPosition += field.PublicName.Length + 1;
        }

        return new ResourceFieldChainExpression(result.FieldChain.ToImmutableArray());
    }

    private string EatFieldChain(string? alternativeErrorMessage)
    {
        var pathBuilder = new StringBuilder();

        while (true)
        {
            int position = GetNextTokenPositionOrEnd();

            if (TokenStack.TryPop(out Token? token) && token.Kind == TokenKind.Text && token.Value != Keywords.Null)
            {
                pathBuilder.Append(token.Value);

                if (TokenStack.TryPeek(out Token? nextToken) && nextToken.Kind == TokenKind.Period)
                {
                    EatSingleCharacterToken(TokenKind.Period);
                    pathBuilder.Append('.');
                }
                else
                {
                    return pathBuilder.ToString();
                }
            }
            else
            {
                throw new QueryParseException(alternativeErrorMessage ?? "Field name expected.", position);
            }
        }
    }

    /// <summary>
    /// Consumes a token containing the expected text from the top of <see cref="TokenStack" />. Throws a <see cref="QueryParseException" /> if a different
    /// token kind is at the top, it contains a different text, or if there are no more tokens available.
    /// </summary>
    protected void EatText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (!TokenStack.TryPop(out Token? token) || token.Kind != TokenKind.Text || token.Value != text)
        {
            int position = token?.Position ?? GetNextTokenPositionOrEnd();
            throw new QueryParseException($"{text} expected.", position);
        }
    }

    /// <summary>
    /// Consumes the expected token kind from the top of <see cref="TokenStack" />. Throws a <see cref="QueryParseException" /> if a different token kind is
    /// at the top, or if there are no more tokens available.
    /// </summary>
    protected virtual void EatSingleCharacterToken(TokenKind kind)
    {
        if (!TokenStack.TryPop(out Token? token) || token.Kind != kind)
        {
            char ch = QueryTokenizer.SingleCharacterToTokenKinds.Single(pair => pair.Value == kind).Key;
            int position = token?.Position ?? GetNextTokenPositionOrEnd();
            throw new QueryParseException($"{ch} expected.", position);
        }
    }

    /// <summary>
    /// Gets the zero-based position of the token at the top of <see cref="TokenStack" />, or the position at the end of the source text if there are no more
    /// tokens available.
    /// </summary>
    protected int GetNextTokenPositionOrEnd()
    {
        if (TokenStack.TryPeek(out Token? nextToken))
        {
            return nextToken.Position;
        }

        return Source.Length;
    }

    /// <summary>
    /// Gets the zero-based position of the last field in the specified resource field chain.
    /// </summary>
    protected int GetRelativePositionOfLastFieldInChain(ResourceFieldChainExpression fieldChain)
    {
        ArgumentNullException.ThrowIfNull(fieldChain);

        int position = 0;

        for (int index = 0; index < fieldChain.Fields.Count - 1; index++)
        {
            position += fieldChain.Fields[index].PublicName.Length + 1;
        }

        return position;
    }

    /// <summary>
    /// Throws a <see cref="QueryParseException" /> when <see cref="TokenStack" /> is not empty. Derived types should call this when parsing has completed,
    /// to ensure all input has been processed.
    /// </summary>
    protected void AssertTokenStackIsEmpty()
    {
        if (TokenStack.Count > 0)
        {
            int position = GetNextTokenPositionOrEnd();
            throw new QueryParseException("End of expression expected.", position);
        }
    }

    private protected void ParseRelationshipChain(IncludeTreeNode treeRoot)
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

        ReadOnlyCollection<IncludeTreeNode> children = ParseRelationshipName([treeRoot]);

        while (TokenStack.TryPeek(out Token? nextToken) && nextToken.Kind == TokenKind.Period)
        {
            EatSingleCharacterToken(TokenKind.Period);

            children = ParseRelationshipName(children);
        }
    }

    private ReadOnlyCollection<IncludeTreeNode> ParseRelationshipName(IReadOnlyCollection<IncludeTreeNode> parents)
    {
        int position = GetNextTokenPositionOrEnd();

        if (TokenStack.TryPop(out Token? token) && token.Kind == TokenKind.Text)
        {
            return LookupRelationshipName(token.Value!, parents, position);
        }

        throw new QueryParseException("Relationship name expected.", position);
    }

    private ReadOnlyCollection<IncludeTreeNode> LookupRelationshipName(string relationshipName, IReadOnlyCollection<IncludeTreeNode> parents, int position)
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
                IReadOnlyCollection<IncludeTreeNode> affectedChildren = parent.EnsureChildren(relationshipsToInclude);
                children.AddRange(affectedChildren);
            }
        }

        AssertRelationshipsFound(relationshipsFound, relationshipName, parents, position);
        AssertAtLeastOneCanBeIncluded(relationshipsFound, relationshipName, position);

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

    private void AssertAtLeastOneCanBeIncluded(HashSet<RelationshipAttribute> relationshipsFound, string relationshipName, int position)
    {
        if (relationshipsFound.All(relationship => relationship.IsIncludeBlocked()))
        {
            ResourceType resourceType = relationshipsFound.First().LeftType;
            string message = $"Including the relationship '{relationshipName}' on '{resourceType}' is not allowed.";

            var exception = new QueryParseException(message, position);
            string specificMessage = exception.GetMessageWithPosition(Source);

            throw new InvalidQueryStringParameterException("include", "The specified include is invalid.", specificMessage);
        }
    }

    internal sealed class IncludeTreeNode
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

        public IReadOnlyCollection<IncludeTreeNode> EnsureChildren(RelationshipAttribute[] relationships)
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
