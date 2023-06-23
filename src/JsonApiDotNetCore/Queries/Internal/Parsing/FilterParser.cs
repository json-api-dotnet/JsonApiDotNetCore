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

namespace JsonApiDotNetCore.Queries.Internal.Parsing;

/// <summary>
/// Parses the JSON:API 'filter' query string parameter value.
/// </summary>
[PublicAPI]
public class FilterParser : QueryExpressionParser
{
    private const string PlaceholderMessageForAttributeNotString = "JsonApiDotNetCore:Placeholder_for_attribute_not_string";

    private readonly IResourceFactory _resourceFactory;
    private readonly IEnumerable<IFilterValueConverter> _filterValueConverters;
    private ResourceType _resourceTypeInScope = null!;

    public FilterParser(IResourceFactory resourceFactory, IEnumerable<IFilterValueConverter> filterValueConverters)
    {
        ArgumentGuard.NotNull(resourceFactory);
        ArgumentGuard.NotNull(filterValueConverters);

        _resourceFactory = resourceFactory;
        _filterValueConverters = filterValueConverters;
    }

    public FilterExpression Parse(string source, ResourceType resourceType)
    {
        ArgumentGuard.NotNull(resourceType);

        return InScopeOfResourceType(resourceType, () =>
        {
            Tokenize(source);

            FilterExpression expression = ParseFilter();

            AssertTokenStackIsEmpty();

            return expression;
        });
    }

    protected FilterExpression ParseFilter()
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

    protected NotExpression ParseNot()
    {
        EatText(Keywords.Not);
        EatSingleCharacterToken(TokenKind.OpenParen);

        FilterExpression child = ParseFilter();

        EatSingleCharacterToken(TokenKind.CloseParen);

        return new NotExpression(child);
    }

    protected LogicalExpression ParseLogical(string operatorName)
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

    protected ComparisonExpression ParseComparison(string operatorName)
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

    protected MatchTextExpression ParseTextMatch(string matchFunctionName)
    {
        EatText(matchFunctionName);
        EatSingleCharacterToken(TokenKind.OpenParen);

        int fieldChainStartPosition = GetNextTokenPositionOrEnd();

        ResourceFieldChainExpression targetAttributeChain =
            ParseFieldChain(BuiltInPatterns.ToOneChainEndingInAttribute, FieldChainPatternMatchOptions.None, _resourceTypeInScope, null);

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

        var matchKind = Enum.Parse<TextMatchKind>(matchFunctionName.Pascalize());
        return new MatchTextExpression(targetAttributeChain, constant, matchKind);
    }

    protected AnyExpression ParseAny()
    {
        EatText(Keywords.Any);
        EatSingleCharacterToken(TokenKind.OpenParen);

        ResourceFieldChainExpression targetAttributeChain =
            ParseFieldChain(BuiltInPatterns.ToOneChainEndingInAttribute, FieldChainPatternMatchOptions.None, _resourceTypeInScope, null);

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

    protected HasExpression ParseHas()
    {
        EatText(Keywords.Has);
        EatSingleCharacterToken(TokenKind.OpenParen);

        ResourceFieldChainExpression targetCollection =
            ParseFieldChain(BuiltInPatterns.ToOneChainEndingInToMany, FieldChainPatternMatchOptions.None, _resourceTypeInScope, null);

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
        return InScopeOfResourceType(hasManyRelationship.RightType, ParseFilter);
    }

    private IsTypeExpression ParseIsType()
    {
        EatText(Keywords.IsType);
        EatSingleCharacterToken(TokenKind.OpenParen);

        ResourceFieldChainExpression? targetToOneRelationship = TryParseToOneRelationshipChain();

        EatSingleCharacterToken(TokenKind.Comma);

        ResourceType baseType = targetToOneRelationship != null ? ((RelationshipAttribute)targetToOneRelationship.Fields[^1]).RightType : _resourceTypeInScope;
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

        return ParseFieldChain(BuiltInPatterns.ToOneChain, FieldChainPatternMatchOptions.None, _resourceTypeInScope, "Relationship name or , expected.");
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

            filter = InScopeOfResourceType(derivedType, ParseFilter);
        }

        return filter;
    }

    protected QueryExpression ParseCountOrField(FieldChainPattern pattern)
    {
        CountExpression? count = TryParseCount(FieldChainPatternMatchOptions.None, _resourceTypeInScope);

        if (count != null)
        {
            return count;
        }

        return ParseFieldChain(pattern, FieldChainPatternMatchOptions.None, _resourceTypeInScope, "Count function or field name expected.");
    }

    protected QueryExpression ParseCountOrConstantOrField(Func<string, int, object> constantValueConverter)
    {
        CountExpression? count = TryParseCount(FieldChainPatternMatchOptions.None, _resourceTypeInScope);

        if (count != null)
        {
            return count;
        }

        LiteralConstantExpression? constant = TryParseConstant(constantValueConverter);

        if (constant != null)
        {
            return constant;
        }

        return ParseFieldChain(BuiltInPatterns.ToOneChainEndingInAttribute, FieldChainPatternMatchOptions.None, _resourceTypeInScope,
            "Count function, value between quotes or field name expected.");
    }

    protected QueryExpression ParseCountOrConstantOrNullOrField(Func<string, int, object> constantValueConverter)
    {
        CountExpression? count = TryParseCount(FieldChainPatternMatchOptions.None, _resourceTypeInScope);

        if (count != null)
        {
            return count;
        }

        IdentifierExpression? constantOrNull = TryParseConstantOrNull(constantValueConverter);

        if (constantOrNull != null)
        {
            return constantOrNull;
        }

        return ParseFieldChain(BuiltInPatterns.ToOneChainEndingInAttribute, FieldChainPatternMatchOptions.None, _resourceTypeInScope,
            "Count function, value between quotes, null or field name expected.");
    }

    protected LiteralConstantExpression? TryParseConstant(Func<string, int, object> constantValueConverter)
    {
        if (TokenStack.TryPeek(out Token? nextToken) && nextToken.Kind == TokenKind.QuotedText)
        {
            TokenStack.Pop();

            object constantValue = constantValueConverter(nextToken.Value!, nextToken.Position);
            return new LiteralConstantExpression(constantValue, nextToken.Value!);
        }

        return null;
    }

    protected IdentifierExpression? TryParseConstantOrNull(Func<string, int, object> constantValueConverter)
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

    protected LiteralConstantExpression ParseConstant(Func<string, int, object> constantValueConverter)
    {
        LiteralConstantExpression? constant = TryParseConstant(constantValueConverter);

        if (constant == null)
        {
            int position = GetNextTokenPositionOrEnd();
            throw new QueryParseException("Value between quotes expected.", position);
        }

        return constant;
    }

    protected NullConstantExpression ParseNull()
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

    private TResult InScopeOfResourceType<TResult>(ResourceType resourceType, Func<TResult> action)
    {
        ResourceType backupType = _resourceTypeInScope;

        try
        {
            _resourceTypeInScope = resourceType;
            return action();
        }
        finally
        {
            _resourceTypeInScope = backupType;
        }
    }
}
