using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
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

        IncludeExpression expression = ParseInclude(resourceType);

        AssertTokenStackIsEmpty();
        ValidateMaximumIncludeDepth(expression, 0);

        return expression;
    }

    protected virtual IncludeExpression ParseInclude(ResourceType resourceType)
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

            ParseRelationshipChain(treeRoot);
        }

        return treeRoot.ToExpression();
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
}
