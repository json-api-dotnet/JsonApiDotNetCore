using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
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
        Token[] tokens = tokenizer.EnumerateTokens().ToArray();
        TokenStack = TokensToStack(tokens);
    }

    private static Stack<Token> TokensToStack(Token[] tokens)
    {
        var stack = new Stack<Token>(tokens.Length);

        for (int index = tokens.Length - 1; index >= 0; index--)
        {
            stack.Push(tokens[index]);
        }

        return stack;
    }

    /// <summary>
    /// Indicates whether the specified token represents the 'null' constant keyword.
    /// </summary>
    protected static bool IsNullConstantToken([NotNullWhen(true)] Token? token)
    {
        return token is { Kind: TokenKind.Text, Value: Keywords.Null };
    }

    /// <summary>
    /// Parses a dot-separated path of field names into a chain of resource fields, while matching it against the specified pattern.
    /// </summary>
    protected ResourceFieldChainExpression ParseFieldChain(FieldChainPattern pattern, IAttributeContainer container,
        FieldChainPatternMatchOptions options = FieldChainPatternMatchOptions.None)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        ArgumentNullException.ThrowIfNull(container);

        int startPosition = GetNextTokenPositionOrEnd();

        string path = EatFieldChain();
        PatternMatchResult result = pattern.Match(path, container, options);

        if (!result.IsSuccess)
        {
            string containerDescription = container is ResourceType resourceType ? $"resource type '{resourceType}'" : $"compound type '{container}'";

            string message = result.IsFieldChainError
                ? result.FailureMessage
                : $"Field chain on {containerDescription} failed to match the pattern: {pattern.GetDescription()}. {result.FailureMessage}";

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

    private string EatFieldChain()
    {
        var pathBuilder = new StringBuilder();

        while (true)
        {
            int position = GetNextTokenPositionOrEnd();

            if (TokenStack.TryPop(out Token? token) && token.Kind == TokenKind.Text && !IsNullConstantToken(token))
            {
                pathBuilder.Append(token.Value);

                if (TryEatSingleCharacterToken(TokenKind.Period))
                {
                    pathBuilder.Append('.');
                }
                else
                {
                    return pathBuilder.ToString();
                }
            }
            else
            {
                throw new QueryParseException(QueryParseChoices.FieldName, position);
            }
        }
    }

    /// <summary>
    /// Consumes a token containing the expected text from the top of <see cref="TokenStack" />. Throws a <see cref="QueryParseException" /> if a different
    /// token kind is at the top, it contains a different text, or if there are no more tokens available.
    /// </summary>
    protected virtual void EatText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (!TokenStack.TryPop(out Token? token) || token.Kind != TokenKind.Text || token.Value != text)
        {
            int position = token?.Position ?? GetNextTokenPositionOrEnd();
            throw new QueryParseException($"{text} expected.", position);
        }
    }

    /// <summary>
    /// Consumes a token containing the expected text from the top of <see cref="TokenStack" /> if present.
    /// </summary>
    protected virtual bool TryEatText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (TokenStack.TryPeek(out Token? nextToken) && nextToken.Kind == TokenKind.Text && nextToken.Value == text)
        {
            TokenStack.Pop();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Consumes the expected single-character token kind from the top of <see cref="TokenStack" />. Throws a <see cref="QueryParseException" /> if a
    /// different token kind is at the top, or if there are no more tokens available.
    /// </summary>
    protected virtual void EatSingleCharacterToken(TokenKind kind)
    {
        if (!TokenStack.TryPop(out Token? token) || token.Kind != kind)
        {
            if (!QueryTokenizer.TokenKindToSingleCharacterLookup.TryGetValue(kind, out char ch))
            {
                throw new InvalidOperationException($"Token kind '{kind}' is not a single-character token.");
            }

            int position = token?.Position ?? GetNextTokenPositionOrEnd();
            throw new QueryParseException($"{ch} expected.", position);
        }
    }

    /// <summary>
    /// Consumes the expected single-character token kind from the top of <see cref="TokenStack" /> if present.
    /// </summary>
    protected virtual bool TryEatSingleCharacterToken(TokenKind kind)
    {
        if (TokenStack.TryPeek(out Token? nextToken) && nextToken.Kind == kind)
        {
            if (!QueryTokenizer.TokenKindToSingleCharacterLookup.TryGetValue(kind, out _))
            {
                throw new InvalidOperationException($"Token kind '{kind}' is not a single-character token.");
            }

            TokenStack.Pop();
            return true;
        }

        return false;
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
            throw new QueryParseException(QueryParseChoices.EndOfExpression, position);
        }
    }

    /// <summary>
    /// Parses a comma-separated sequence of relationship chains, taking relationships on derived types into account.
    /// </summary>
    protected IncludeExpression ParseCommaSeparatedSequenceOfRelationshipChains(ResourceType resourceType)
    {
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

            ParseRelationshipChain(treeRoot, false, true);
        }

        return treeRoot.ToExpression();
    }

    /// <summary>
    /// Parses a relationship chain that ends in a to-many relationship, taking relationships on derived types into account.
    /// </summary>
    protected IncludeExpression ParseRelationshipChainEndingInToMany(ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        var treeRoot = IncludeTreeNode.CreateRoot(resourceType);
        ParseRelationshipChain(treeRoot, true, false);

        return treeRoot.ToExpression();
    }

    private void ParseRelationshipChain(IncludeTreeNode treeRoot, bool requireChainEndsInToMany, bool validateCanInclude)
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

        ReadOnlyCollection<IncludeTreeNode> children = ParseRelationshipName([treeRoot], requireChainEndsInToMany, validateCanInclude);

        while (TryEatSingleCharacterToken(TokenKind.Period))
        {
            children = ParseRelationshipName(children, requireChainEndsInToMany, validateCanInclude);
        }
    }

    private ReadOnlyCollection<IncludeTreeNode> ParseRelationshipName(IReadOnlyCollection<IncludeTreeNode> parents, bool requireChainEndsInToMany,
        bool validateCanInclude)
    {
        int position = GetNextTokenPositionOrEnd();

        if (TokenStack.TryPop(out Token? token) && token.Kind == TokenKind.Text)
        {
            bool isAtEndOfChain = !TokenStack.TryPeek(out Token? nextToken) || nextToken.Kind != TokenKind.Period;
            bool requireToMany = requireChainEndsInToMany && isAtEndOfChain;

            return LookupRelationshipName(token.Value!, parents, requireToMany, validateCanInclude, position);
        }

        QueryParseChoices choices = requireChainEndsInToMany ? QueryParseChoices.ToManyRelationshipName : QueryParseChoices.RelationshipName;
        throw new QueryParseException(choices, position);
    }

    private ReadOnlyCollection<IncludeTreeNode> LookupRelationshipName(string relationshipName, IReadOnlyCollection<IncludeTreeNode> parents,
        bool requireToMany, bool validateCanInclude, int position)
    {
        List<IncludeTreeNode> children = [];
        HashSet<RelationshipAttribute> relationshipsFound = [];

        foreach (IncludeTreeNode parent in parents)
        {
            // Depending on the left side of the include chain, we may match relationships anywhere in the resource type hierarchy.
            // This is compensated for when rendering the response, which substitutes relationships on base types with the derived ones.
            HashSet<RelationshipAttribute> relationships = GetRelationshipsInConcreteTypes(parent.Relationship.RightType, relationshipName, requireToMany);

            if (relationships.Count > 0)
            {
                relationshipsFound.UnionWith(relationships);

                RelationshipAttribute[] relationshipsToAttach = validateCanInclude
                    ? relationships.Where(relationship => !relationship.IsIncludeBlocked()).ToArray()
                    : relationships.ToArray();

                ReadOnlyCollection<IncludeTreeNode> affectedChildren = parent.EnsureChildren(relationshipsToAttach);
                children.AddRange(affectedChildren);
            }
        }

        AssertRelationshipsFound(relationshipsFound, relationshipName, requireToMany, parents, position);

        if (validateCanInclude)
        {
            AssertAtLeastOneCanBeIncluded(relationshipsFound, relationshipName, position);
        }

        return children.AsReadOnly();
    }

    private static HashSet<RelationshipAttribute> GetRelationshipsInConcreteTypes(ResourceType resourceType, string relationshipName, bool requireToMany)
    {
        HashSet<RelationshipAttribute> relationshipsToInclude = [];

        foreach (RelationshipAttribute relationship in resourceType.GetRelationshipsInTypeOrDerived(relationshipName))
        {
            if (!requireToMany || relationship is HasManyAttribute)
            {
                if (!relationship.LeftType.ClrType.IsAbstract)
                {
                    relationshipsToInclude.Add(relationship);
                }
            }

            IncludeRelationshipsFromConcreteDerivedTypes(relationship, requireToMany, relationshipsToInclude);
        }

        return relationshipsToInclude;
    }

    private static void IncludeRelationshipsFromConcreteDerivedTypes(RelationshipAttribute relationship, bool requireToMany,
        HashSet<RelationshipAttribute> relationshipsToInclude)
    {
        foreach (ResourceType derivedType in relationship.LeftType.GetAllConcreteDerivedTypes())
        {
            if (!requireToMany || relationship is HasManyAttribute)
            {
                RelationshipAttribute relationshipInDerived = derivedType.GetRelationshipByPublicName(relationship.PublicName);
                relationshipsToInclude.Add(relationshipInDerived);
            }
        }
    }

    private static void AssertRelationshipsFound(HashSet<RelationshipAttribute> relationshipsFound, string relationshipName, bool requireToMany,
        IReadOnlyCollection<IncludeTreeNode> parents, int position)
    {
        if (relationshipsFound.Count > 0)
        {
            return;
        }

        ResourceType[] parentResourceTypes = parents.Select(parent => parent.Relationship.RightType).Distinct().ToArray();

        bool hasDerivedTypes = parents.Any(parent => parent.Relationship.RightType.DirectlyDerivedTypes.Count > 0);

        string message = GetErrorMessageForNoneFound(relationshipName, requireToMany, parentResourceTypes, hasDerivedTypes);
        throw new QueryParseException(message, position);
    }

    private static string GetErrorMessageForNoneFound(string relationshipName, bool requireToMany, ResourceType[] parentResourceTypes, bool hasDerivedTypes)
    {
        var builder = new StringBuilder($"{(requireToMany ? "To-many relationship" : "Relationship")} '{relationshipName}'");

        if (parentResourceTypes.Length == 1)
        {
            builder.Append($" does not exist on resource type '{parentResourceTypes.First().PublicName}'");
        }
        else
        {
            string typeNames = string.Join(", ", parentResourceTypes.Select(type => $"'{type.PublicName}'"));
            builder.Append($" does not exist on any of the resource types {typeNames}");
        }

        builder.Append(hasDerivedTypes ? parentResourceTypes.Length == 1 ? " or any of its derived types." : " or any of their derived types." : ".");

        return builder.ToString();
    }

    private static void AssertAtLeastOneCanBeIncluded(HashSet<RelationshipAttribute> relationshipsFound, string relationshipName, int position)
    {
        if (relationshipsFound.All(relationship => relationship.IsIncludeBlocked()))
        {
            ResourceType resourceType = relationshipsFound.First().LeftType;
            throw new QueryParseException($"Including relationship '{relationshipName}' on '{resourceType}' is not allowed.", position);
        }
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
