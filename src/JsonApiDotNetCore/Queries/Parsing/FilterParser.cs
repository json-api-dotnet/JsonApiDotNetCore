using System.Collections.Immutable;
using Humanizer;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.QueryStrings.FieldChains;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Resources.Internal;

namespace JsonApiDotNetCore.Queries.Parsing;

/// <inheritdoc cref="IFilterParser" />
[PublicAPI]
public class FilterParser : QueryExpressionParser, IFilterParser
{
    private const string PlaceholderMessageForAttributeNotString = "JsonApiDotNetCore:Placeholder_for_attribute_not_string";

    private readonly IResourceFactory _resourceFactory;
    private readonly IEnumerable<IFilterValueConverter> _filterValueConverters;
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

    public FilterParser(IResourceFactory resourceFactory, IEnumerable<IFilterValueConverter> filterValueConverters)
    {
        ArgumentGuard.NotNull(resourceFactory);
        ArgumentGuard.NotNull(filterValueConverters);

        _resourceFactory = resourceFactory;
        _filterValueConverters = filterValueConverters;
    }

    /// <inheritdoc />
    public FilterExpression Parse(string source, ResourceType resourceType)
    {
        ArgumentGuard.NotNull(resourceType);

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

    protected virtual FilterExpression ParseFilter()
    {
        int position = GetNextTokenPositionOrEnd();

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
        var comparisonOperator = Enum.Parse<ComparisonOperator>(operatorName.Pascalize());

        EatText(operatorName);
        EatSingleCharacterToken(TokenKind.OpenParen);

        // Allow equality comparison of a to-one relationship with null.
        FieldChainPattern leftChainPattern = comparisonOperator == ComparisonOperator.Equals
            ? BuiltInPatterns.ToOneChainEndingInAttributeOrToOne
            : BuiltInPatterns.ToOneChainEndingInAttribute;

        QueryExpression leftTerm = ParseCountOrField(leftChainPattern);

        EatSingleCharacterToken(TokenKind.Comma);

        QueryExpression rightTerm;

        if (leftTerm is CountExpression)
        {
            Func<string, int, object> rightConstantValueConverter = GetConstantValueConverterForCount();
            rightTerm = ParseCountOrConstantOrField(rightConstantValueConverter);
        }
        else if (leftTerm is ResourceFieldChainExpression fieldChain && fieldChain.Fields[^1] is AttrAttribute attribute)
        {
            Func<string, int, object> rightConstantValueConverter = GetConstantValueConverterForAttribute(attribute, typeof(ComparisonExpression));
            rightTerm = ParseCountOrConstantOrNullOrField(rightConstantValueConverter);
        }
        else
        {
            rightTerm = ParseNull();
        }

        EatSingleCharacterToken(TokenKind.CloseParen);

        return new ComparisonExpression(comparisonOperator, leftTerm, rightTerm);
    }

    protected virtual MatchTextExpression ParseTextMatch(string operatorName)
    {
        EatText(operatorName);
        EatSingleCharacterToken(TokenKind.OpenParen);

        int fieldChainStartPosition = GetNextTokenPositionOrEnd();

        ResourceFieldChainExpression targetAttributeChain =
            ParseFieldChain(BuiltInPatterns.ToOneChainEndingInAttribute, FieldChainPatternMatchOptions.None, ResourceTypeInScope, null);

        var targetAttribute = (AttrAttribute)targetAttributeChain.Fields[^1];

        EatSingleCharacterToken(TokenKind.Comma);

        Func<string, int, object> constantValueConverter = GetConstantValueConverterForAttribute(targetAttribute, typeof(MatchTextExpression));
        LiteralConstantExpression constant;

        try
        {
            constant = ParseConstant(constantValueConverter);
        }
        catch (QueryParseException exception) when (exception.Message == PlaceholderMessageForAttributeNotString)
        {
            int attributePosition = fieldChainStartPosition + GetRelativePositionOfLastFieldInChain(targetAttributeChain);
            throw new QueryParseException("Attribute of type 'String' expected.", attributePosition);
        }

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

        Func<string, int, object> constantValueConverter = GetConstantValueConverterForAttribute(targetAttribute, typeof(AnyExpression));
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

            filter = ParseFilterInHas((HasManyAttribute)targetCollection.Fields[^1]);
        }

        EatSingleCharacterToken(TokenKind.CloseParen);

        return new HasExpression(targetCollection, filter);
    }

    private FilterExpression ParseFilterInHas(HasManyAttribute hasManyRelationship)
    {
        using (InScopeOfResourceType(hasManyRelationship.RightType))
        {
            return ParseFilter();
        }
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

    private ResourceType ResolveDerivedType(ResourceType baseType, string derivedTypeName, int position)
    {
        ResourceType? derivedType = GetDerivedType(baseType, derivedTypeName);

        if (derivedType == null)
        {
            throw new QueryParseException($"Resource type '{derivedTypeName}' does not exist or does not derive from '{baseType.PublicName}'.", position);
        }

        return derivedType;
    }

    private ResourceType? GetDerivedType(ResourceType baseType, string publicName)
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

    private QueryExpression ParseCountOrField(FieldChainPattern pattern)
    {
        CountExpression? count = TryParseCount(FieldChainPatternMatchOptions.None, ResourceTypeInScope);

        if (count != null)
        {
            return count;
        }

        return ParseFieldChain(pattern, FieldChainPatternMatchOptions.None, ResourceTypeInScope, "Count function or field name expected.");
    }

    private QueryExpression ParseCountOrConstantOrField(Func<string, int, object> constantValueConverter)
    {
        CountExpression? count = TryParseCount(FieldChainPatternMatchOptions.None, ResourceTypeInScope);

        if (count != null)
        {
            return count;
        }

        LiteralConstantExpression? constant = TryParseConstant(constantValueConverter);

        if (constant != null)
        {
            return constant;
        }

        return ParseFieldChain(BuiltInPatterns.ToOneChainEndingInAttribute, FieldChainPatternMatchOptions.None, ResourceTypeInScope,
            "Count function, value between quotes or field name expected.");
    }

    private QueryExpression ParseCountOrConstantOrNullOrField(Func<string, int, object> constantValueConverter)
    {
        CountExpression? count = TryParseCount(FieldChainPatternMatchOptions.None, ResourceTypeInScope);

        if (count != null)
        {
            return count;
        }

        IdentifierExpression? constantOrNull = TryParseConstantOrNull(constantValueConverter);

        if (constantOrNull != null)
        {
            return constantOrNull;
        }

        return ParseFieldChain(BuiltInPatterns.ToOneChainEndingInAttribute, FieldChainPatternMatchOptions.None, ResourceTypeInScope,
            "Count function, value between quotes, null or field name expected.");
    }

    private LiteralConstantExpression? TryParseConstant(Func<string, int, object> constantValueConverter)
    {
        if (TokenStack.TryPeek(out Token? nextToken) && nextToken.Kind == TokenKind.QuotedText)
        {
            TokenStack.Pop();

            object constantValue = constantValueConverter(nextToken.Value!, nextToken.Position);
            return new LiteralConstantExpression(constantValue, nextToken.Value!);
        }

        return null;
    }

    private IdentifierExpression? TryParseConstantOrNull(Func<string, int, object> constantValueConverter)
    {
        if (TokenStack.TryPeek(out Token? nextToken))
        {
            if (nextToken is { Kind: TokenKind.Text, Value: Keywords.Null })
            {
                TokenStack.Pop();
                return NullConstantExpression.Instance;
            }

            if (nextToken.Kind == TokenKind.QuotedText)
            {
                TokenStack.Pop();

                object constantValue = constantValueConverter(nextToken.Value!, nextToken.Position);
                return new LiteralConstantExpression(constantValue, nextToken.Value!);
            }
        }

        return null;
    }

    private LiteralConstantExpression ParseConstant(Func<string, int, object> constantValueConverter)
    {
        LiteralConstantExpression? constant = TryParseConstant(constantValueConverter);

        if (constant == null)
        {
            int position = GetNextTokenPositionOrEnd();
            throw new QueryParseException("Value between quotes expected.", position);
        }

        return constant;
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

    private Func<string, int, object> GetConstantValueConverterForCount()
    {
        return (stringValue, position) => ConvertStringToType(stringValue, position, typeof(int));
    }

    private static object ConvertStringToType(string value, int position, Type type)
    {
        try
        {
            return RuntimeTypeConverter.ConvertType(value, type)!;
        }
        catch (FormatException exception)
        {
            throw new QueryParseException($"Failed to convert '{value}' of type 'String' to type '{type.Name}'.", position, exception);
        }
    }

    private Func<string, int, object> GetConstantValueConverterForAttribute(AttrAttribute attribute, Type outerExpressionType)
    {
        return (stringValue, position) =>
        {
            object? value = TryConvertFromStringUsingFilterValueConverters(attribute, stringValue, position, outerExpressionType);

            if (value != null)
            {
                return value;
            }

            if (outerExpressionType == typeof(MatchTextExpression))
            {
                if (attribute.Property.PropertyType != typeof(string))
                {
                    // Use placeholder message, so we can correct the position to the attribute instead of its value.
                    throw new QueryParseException(PlaceholderMessageForAttributeNotString, -1);
                }
            }
            else
            {
                // Partial text matching on an obfuscated ID usually fails.
                if (attribute.Property.Name == nameof(Identifiable<object>.Id))
                {
                    return DeObfuscateStringId(attribute.Type.ClrType, stringValue);
                }
            }

            return ConvertStringToType(stringValue, position, attribute.Property.PropertyType);
        };
    }

    private object? TryConvertFromStringUsingFilterValueConverters(AttrAttribute attribute, string stringValue, int position, Type outerExpressionType)
    {
        foreach (IFilterValueConverter converter in _filterValueConverters)
        {
            if (converter.CanConvert(attribute))
            {
                object result = converter.Convert(attribute, stringValue, position, outerExpressionType);

                if (result == null)
                {
                    throw new InvalidOperationException(
                        $"Converter '{converter.GetType().Name}' returned null for '{stringValue}' on attribute '{attribute.PublicName}'. Return a sentinel value instead.");
                }

                return result;
            }
        }

        return null;
    }

    private object DeObfuscateStringId(Type resourceClrType, string stringId)
    {
        IIdentifiable tempResource = _resourceFactory.CreateInstance(resourceClrType);
        tempResource.StringId = stringId;
        return tempResource.GetTypedId();
    }

    protected override void ValidateField(ResourceFieldAttribute field, int position)
    {
        if (field.IsFilterBlocked())
        {
            string kind = field is AttrAttribute ? "attribute" : "relationship";
            throw new QueryParseException($"Filtering on {kind} '{field.PublicName}' is not allowed.", position);
        }
    }

    /// <summary>
    /// Changes the resource type currently in scope and restores the original resource type when the return value is disposed.
    /// </summary>
    protected IDisposable InScopeOfResourceType(ResourceType resourceType)
    {
        ArgumentGuard.NotNull(resourceType);

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

    private sealed class PopResourceTypeOnDispose : IDisposable
    {
        private readonly Stack<ResourceType> _resourceTypeStack;

        public PopResourceTypeOnDispose(Stack<ResourceType> resourceTypeStack)
        {
            _resourceTypeStack = resourceTypeStack;
        }

        public void Dispose()
        {
            _resourceTypeStack.Pop();
        }
    }
}
