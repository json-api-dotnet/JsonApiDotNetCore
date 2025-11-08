using System.Collections.Immutable;
using Humanizer;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.QueryStrings.FieldChains;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Parsing;

/// <inheritdoc cref="IFilterParser" />
[PublicAPI]
public class FilterParser : QueryExpressionParser, IFilterParser
{
    private static readonly HashSet<string> FilterKeywords =
    [
        Keywords.Not,
        Keywords.And,
        Keywords.Or,
        Keywords.Equals,
        Keywords.GreaterThan,
        Keywords.GreaterOrEqual,
        Keywords.LessThan,
        Keywords.LessOrEqual,
        Keywords.Contains,
        Keywords.StartsWith,
        Keywords.EndsWith,
        Keywords.Any,
        Keywords.Count,
        Keywords.Has,
        Keywords.IsType
    ];

    private readonly IResourceFactory _resourceFactory;
    private readonly Stack<ResourceType> _resourceTypeStack = new();

    /// <summary>
    /// Gets the resource type currently in scope. Call <see cref="InScopeOfResourceType" /> to temporarily change the current resource type.
    /// </summary>
    protected ResourceType ResourceTypeInScope
    {
        get
        {
            if (_resourceTypeStack.Count == 0)
            {
                throw new InvalidOperationException("No resource type is currently in scope. Call Parse() first.");
            }

            return _resourceTypeStack.Peek();
        }
    }

    public FilterParser(IResourceFactory resourceFactory)
    {
        ArgumentNullException.ThrowIfNull(resourceFactory);

        _resourceFactory = resourceFactory;
    }

    /// <inheritdoc />
    public FilterExpression Parse(string source, ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        Tokenize(source);

        _resourceTypeStack.Clear();
        FilterExpression expression;

        using (InScopeOfResourceType(resourceType))
        {
            expression = ParseFilter();

            AssertTokenStackIsEmpty();
        }

        AssertResourceTypeStackIsEmpty();

        return expression;
    }

    protected virtual bool IsFunction(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        return name == Keywords.Count || FilterKeywords.Contains(name);
    }

    protected virtual FunctionExpression ParseFunction()
    {
        if (TokenStack.TryPeek(out Token? nextToken) && nextToken.Kind == TokenKind.Text)
        {
            switch (nextToken.Value)
            {
                case Keywords.Count:
                {
                    return ParseCount();
                }
            }
        }

        return ParseFilter();
    }

    private CountExpression ParseCount()
    {
        EatText(Keywords.Count);
        EatSingleCharacterToken(TokenKind.OpenParen);

        ResourceFieldChainExpression targetCollection =
            ParseFieldChain(BuiltInPatterns.ToOneChainEndingInToMany, FieldChainPatternMatchOptions.None, ResourceTypeInScope, null);

        EatSingleCharacterToken(TokenKind.CloseParen);

        return new CountExpression(targetCollection);
    }

    protected virtual FilterExpression ParseFilter()
    {
        if (TokenStack.TryPeek(out Token? nextToken) && nextToken.Kind == TokenKind.Text)
        {
            switch (nextToken.Value)
            {
                case Keywords.Not:
                {
                    return ParseNot();
                }
                case Keywords.And:
                case Keywords.Or:
                {
                    return ParseLogical(nextToken.Value);
                }
                case Keywords.Equals:
                case Keywords.LessThan:
                case Keywords.LessOrEqual:
                case Keywords.GreaterThan:
                case Keywords.GreaterOrEqual:
                {
                    return ParseComparison(nextToken.Value);
                }
                case Keywords.Contains:
                case Keywords.StartsWith:
                case Keywords.EndsWith:
                {
                    return ParseTextMatch(nextToken.Value);
                }
                case Keywords.Any:
                {
                    return ParseAny();
                }
                case Keywords.Has:
                {
                    return ParseHas();
                }
                case Keywords.IsType:
                {
                    return ParseIsType();
                }
            }
        }

        int position = GetNextTokenPositionOrEnd();
        throw new QueryParseException("Filter function expected.", position);
    }

    protected virtual NotExpression ParseNot()
    {
        EatText(Keywords.Not);
        EatSingleCharacterToken(TokenKind.OpenParen);

        FilterExpression child = ParseFilter();

        EatSingleCharacterToken(TokenKind.CloseParen);

        return new NotExpression(child);
    }

    protected virtual LogicalExpression ParseLogical(string operatorName)
    {
        ArgumentException.ThrowIfNullOrEmpty(operatorName);

        EatText(operatorName);
        EatSingleCharacterToken(TokenKind.OpenParen);

        ImmutableArray<FilterExpression>.Builder termsBuilder = ImmutableArray.CreateBuilder<FilterExpression>();

        FilterExpression term = ParseFilter();
        termsBuilder.Add(term);

        EatSingleCharacterToken(TokenKind.Comma);

        term = ParseFilter();
        termsBuilder.Add(term);

        while (TokenStack.TryPeek(out Token? nextToken) && nextToken.Kind == TokenKind.Comma)
        {
            EatSingleCharacterToken(TokenKind.Comma);

            term = ParseFilter();
            termsBuilder.Add(term);
        }

        EatSingleCharacterToken(TokenKind.CloseParen);

        var logicalOperator = Enum.Parse<LogicalOperator>(operatorName.Pascalize());
        return new LogicalExpression(logicalOperator, termsBuilder.ToImmutable());
    }

    protected virtual ComparisonExpression ParseComparison(string operatorName)
    {
        ArgumentException.ThrowIfNullOrEmpty(operatorName);

        var comparisonOperator = Enum.Parse<ComparisonOperator>(operatorName.Pascalize());

        EatText(operatorName);
        EatSingleCharacterToken(TokenKind.OpenParen);

        QueryExpression leftTerm = ParseComparisonLeftTerm(comparisonOperator);

        EatSingleCharacterToken(TokenKind.Comma);

        QueryExpression rightTerm = ParseComparisonRightTerm(leftTerm);

        EatSingleCharacterToken(TokenKind.CloseParen);

        return new ComparisonExpression(comparisonOperator, leftTerm, rightTerm);
    }

    private QueryExpression ParseComparisonLeftTerm(ComparisonOperator comparisonOperator)
    {
        if (TokenStack.TryPeek(out Token? nextToken) && nextToken is { Kind: TokenKind.Text } && IsFunction(nextToken.Value!))
        {
            return ParseFunction();
        }

        // Allow equality comparison of a to-one relationship with null.
        FieldChainPattern pattern = comparisonOperator == ComparisonOperator.Equals
            ? BuiltInPatterns.ToOneChainEndingInAttributeOrToOne
            : BuiltInPatterns.ToOneChainEndingInAttribute;

        return ParseFieldChain(pattern, FieldChainPatternMatchOptions.None, ResourceTypeInScope, "Function or field name expected.");
    }

    private QueryExpression ParseComparisonRightTerm(QueryExpression leftTerm)
    {
        if (leftTerm is ResourceFieldChainExpression leftFieldChain)
        {
            ResourceFieldAttribute leftLastField = leftFieldChain.Fields[^1];

            if (leftLastField is HasOneAttribute)
            {
                return ParseNull();
            }

            var leftAttribute = (AttrAttribute)leftLastField;

            ConstantValueConverter constantValueConverter = GetConstantValueConverterForAttribute(leftAttribute);
            return ParseTypedComparisonRightTerm(leftAttribute.Property.PropertyType, constantValueConverter);
        }

        if (leftTerm is FunctionExpression leftFunction)
        {
            ConstantValueConverter constantValueConverter = GetConstantValueConverterForType(leftFunction.ReturnType);
            return ParseTypedComparisonRightTerm(leftFunction.ReturnType, constantValueConverter);
        }

        throw new InvalidOperationException(
            $"Internal error: Expected left term to be a function or field chain, instead of '{leftTerm.GetType().Name}': '{leftTerm}'.");
    }

    private QueryExpression ParseTypedComparisonRightTerm(Type leftType, ConstantValueConverter constantValueConverter)
    {
        bool allowNull = RuntimeTypeConverter.CanContainNull(leftType);

        string errorMessage =
            allowNull ? "Function, field name, value between quotes or null expected." : "Function, field name or value between quotes expected.";

        if (TokenStack.TryPeek(out Token? nextToken))
        {
            if (nextToken is { Kind: TokenKind.QuotedText })
            {
                TokenStack.Pop();

                object constantValue = constantValueConverter(nextToken.Value!, nextToken.Position);
                return new LiteralConstantExpression(constantValue, nextToken.Value!);
            }

            if (nextToken.Kind == TokenKind.Text)
            {
                if (nextToken.Value == Keywords.Null)
                {
                    if (!allowNull)
                    {
                        throw new QueryParseException(errorMessage, nextToken.Position);
                    }

                    TokenStack.Pop();
                    return NullConstantExpression.Instance;
                }

                if (IsFunction(nextToken.Value!))
                {
                    return ParseFunction();
                }

                return ParseFieldChain(BuiltInPatterns.ToOneChainEndingInAttribute, FieldChainPatternMatchOptions.None, ResourceTypeInScope, errorMessage);
            }
        }

        int position = GetNextTokenPositionOrEnd();
        throw new QueryParseException(errorMessage, position);
    }

    protected virtual MatchTextExpression ParseTextMatch(string operatorName)
    {
        ArgumentException.ThrowIfNullOrEmpty(operatorName);

        EatText(operatorName);
        EatSingleCharacterToken(TokenKind.OpenParen);

        int chainStartPosition = GetNextTokenPositionOrEnd();

        ResourceFieldChainExpression targetAttributeChain =
            ParseFieldChain(BuiltInPatterns.ToOneChainEndingInAttribute, FieldChainPatternMatchOptions.None, ResourceTypeInScope, null);

        var targetAttribute = (AttrAttribute)targetAttributeChain.Fields[^1];

        if (targetAttribute.Property.PropertyType != typeof(string))
        {
            int position = chainStartPosition + GetRelativePositionOfLastFieldInChain(targetAttributeChain);
            throw new QueryParseException("Attribute of type 'String' expected.", position);
        }

        EatSingleCharacterToken(TokenKind.Comma);

        ConstantValueConverter constantValueConverter = GetConstantValueConverterForAttribute(targetAttribute);
        LiteralConstantExpression constant = ParseConstant(constantValueConverter);

        EatSingleCharacterToken(TokenKind.CloseParen);

        var matchKind = Enum.Parse<TextMatchKind>(operatorName.Pascalize());
        return new MatchTextExpression(targetAttributeChain, constant, matchKind);
    }

    protected virtual AnyExpression ParseAny()
    {
        EatText(Keywords.Any);
        EatSingleCharacterToken(TokenKind.OpenParen);

        ResourceFieldChainExpression targetAttributeChain =
            ParseFieldChain(BuiltInPatterns.ToOneChainEndingInAttribute, FieldChainPatternMatchOptions.None, ResourceTypeInScope, null);

        var targetAttribute = (AttrAttribute)targetAttributeChain.Fields[^1];

        EatSingleCharacterToken(TokenKind.Comma);

        ImmutableHashSet<LiteralConstantExpression>.Builder constantsBuilder = ImmutableHashSet.CreateBuilder<LiteralConstantExpression>();

        ConstantValueConverter constantValueConverter = GetConstantValueConverterForAttribute(targetAttribute);
        LiteralConstantExpression constant = ParseConstant(constantValueConverter);
        constantsBuilder.Add(constant);

        while (TokenStack.TryPeek(out Token? nextToken) && nextToken.Kind == TokenKind.Comma)
        {
            EatSingleCharacterToken(TokenKind.Comma);

            constant = ParseConstant(constantValueConverter);
            constantsBuilder.Add(constant);
        }

        EatSingleCharacterToken(TokenKind.CloseParen);

        IImmutableSet<LiteralConstantExpression> constantSet = constantsBuilder.ToImmutable();

        return new AnyExpression(targetAttributeChain, constantSet);
    }

    protected virtual HasExpression ParseHas()
    {
        EatText(Keywords.Has);
        EatSingleCharacterToken(TokenKind.OpenParen);

        ResourceFieldChainExpression targetCollection =
            ParseFieldChain(BuiltInPatterns.ToOneChainEndingInToMany, FieldChainPatternMatchOptions.None, ResourceTypeInScope, null);

        FilterExpression? filter = null;

        if (TokenStack.TryPeek(out Token? nextToken) && nextToken.Kind == TokenKind.Comma)
        {
            EatSingleCharacterToken(TokenKind.Comma);

            var hasManyRelationship = (HasManyAttribute)targetCollection.Fields[^1];

            using (InScopeOfResourceType(hasManyRelationship.RightType))
            {
                filter = ParseFilter();
            }
        }

        EatSingleCharacterToken(TokenKind.CloseParen);

        return new HasExpression(targetCollection, filter);
    }

    protected virtual IsTypeExpression ParseIsType()
    {
        EatText(Keywords.IsType);
        EatSingleCharacterToken(TokenKind.OpenParen);

        ResourceFieldChainExpression? targetToOneRelationship = TryParseToOneRelationshipChain();

        EatSingleCharacterToken(TokenKind.Comma);

        ResourceType baseType = targetToOneRelationship != null ? ((RelationshipAttribute)targetToOneRelationship.Fields[^1]).RightType : ResourceTypeInScope;
        ResourceType derivedType = ParseDerivedType(baseType);

        FilterExpression? child = TryParseFilterInIsType(derivedType);

        EatSingleCharacterToken(TokenKind.CloseParen);

        return new IsTypeExpression(targetToOneRelationship, derivedType, child);
    }

    private ResourceFieldChainExpression? TryParseToOneRelationshipChain()
    {
        if (TokenStack.TryPeek(out Token? nextToken) && nextToken.Kind == TokenKind.Comma)
        {
            return null;
        }

        return ParseFieldChain(BuiltInPatterns.ToOneChain, FieldChainPatternMatchOptions.None, ResourceTypeInScope, "Relationship name or , expected.");
    }

    private ResourceType ParseDerivedType(ResourceType baseType)
    {
        int position = GetNextTokenPositionOrEnd();

        if (TokenStack.TryPop(out Token? token) && token.Kind == TokenKind.Text)
        {
            string derivedTypeName = token.Value!;
            return ResolveDerivedType(baseType, derivedTypeName, token.Position);
        }

        throw new QueryParseException("Resource type expected.", position);
    }

    private static ResourceType ResolveDerivedType(ResourceType baseType, string derivedTypeName, int position)
    {
        ResourceType? derivedType = GetDerivedType(baseType, derivedTypeName);

        if (derivedType == null)
        {
            throw new QueryParseException($"Resource type '{derivedTypeName}' does not exist or does not derive from '{baseType}'.", position);
        }

        return derivedType;
    }

    private static ResourceType? GetDerivedType(ResourceType baseType, string publicName)
    {
        foreach (ResourceType derivedType in baseType.DirectlyDerivedTypes)
        {
            if (derivedType.PublicName == publicName)
            {
                return derivedType;
            }

            ResourceType? nextType = GetDerivedType(derivedType, publicName);

            if (nextType != null)
            {
                return nextType;
            }
        }

        return null;
    }

    private FilterExpression? TryParseFilterInIsType(ResourceType derivedType)
    {
        FilterExpression? filter = null;

        if (TokenStack.TryPeek(out Token? nextToken) && nextToken.Kind == TokenKind.Comma)
        {
            EatSingleCharacterToken(TokenKind.Comma);

            using (InScopeOfResourceType(derivedType))
            {
                filter = ParseFilter();
            }
        }

        return filter;
    }

    private LiteralConstantExpression ParseConstant(ConstantValueConverter constantValueConverter)
    {
        int position = GetNextTokenPositionOrEnd();

        if (TokenStack.TryPop(out Token? token) && token.Kind == TokenKind.QuotedText)
        {
            object constantValue = constantValueConverter(token.Value!, token.Position);
            return new LiteralConstantExpression(constantValue, token.Value!);
        }

        throw new QueryParseException("Value between quotes expected.", position);
    }

    private NullConstantExpression ParseNull()
    {
        int position = GetNextTokenPositionOrEnd();

        if (TokenStack.TryPop(out Token? token) && token is { Kind: TokenKind.Text, Value: Keywords.Null })
        {
            return NullConstantExpression.Instance;
        }

        throw new QueryParseException("null expected.", position);
    }

    protected virtual ConstantValueConverter GetConstantValueConverterForType(Type destinationType)
    {
        ArgumentNullException.ThrowIfNull(destinationType);

        return (stringValue, position) =>
        {
            try
            {
                return RuntimeTypeConverter.ConvertType(stringValue, destinationType)!;
            }
            catch (FormatException exception)
            {
                string destinationTypeName = RuntimeTypeConverter.GetFriendlyTypeName(destinationType);
                throw new QueryParseException($"Failed to convert '{stringValue}' of type 'String' to type '{destinationTypeName}'.", position, exception);
            }
        };
    }

    private ConstantValueConverter GetConstantValueConverterForAttribute(AttrAttribute attribute)
    {
        if (attribute is { Property.Name: nameof(Identifiable<object>.Id) })
        {
            return (stringValue, position) =>
            {
                try
                {
                    return DeObfuscateStringId(attribute.Type, stringValue);
                }
                catch (JsonApiException exception)
                {
                    throw new QueryParseException(exception.Errors[0].Detail!, position);
                }
            };
        }

        return GetConstantValueConverterForType(attribute.Property.PropertyType);
    }

    private object DeObfuscateStringId(ResourceType resourceType, string stringId)
    {
        IIdentifiable tempResource = _resourceFactory.CreateInstance(resourceType.ClrType);
        tempResource.StringId = stringId;
        return tempResource.GetTypedId();
    }

    protected override void ValidateField(ResourceFieldAttribute field, int position)
    {
        ArgumentNullException.ThrowIfNull(field);

        if (field.IsFilterBlocked())
        {
            string kind = field is AttrAttribute ? "attribute" : "relationship";
            throw new QueryParseException($"Filtering on {kind} '{field}' is not allowed.", position);
        }
    }

    /// <summary>
    /// Changes the resource type currently in scope and restores the original resource type when the return value is disposed.
    /// </summary>
    protected IDisposable InScopeOfResourceType(ResourceType resourceType)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        _resourceTypeStack.Push(resourceType);
        return new PopResourceTypeOnDispose(_resourceTypeStack);
    }

    private void AssertResourceTypeStackIsEmpty()
    {
        if (_resourceTypeStack.Count > 0)
        {
            throw new InvalidOperationException("There is still a resource type in scope after parsing has completed. " +
                $"Verify that {nameof(IDisposable.Dispose)}() is called on all return values of {nameof(InScopeOfResourceType)}().");
        }
    }

    private sealed class PopResourceTypeOnDispose(Stack<ResourceType> resourceTypeStack) : IDisposable
    {
        private readonly Stack<ResourceType> _resourceTypeStack = resourceTypeStack;

        public void Dispose()
        {
            _resourceTypeStack.Pop();
        }
    }
}
