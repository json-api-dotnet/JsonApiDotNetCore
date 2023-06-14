using System.Collections.Immutable;
using Humanizer;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Resources.Internal;

namespace JsonApiDotNetCore.Queries.Internal.Parsing;

[PublicAPI]
public class FilterParser : QueryExpressionParser
{
    private readonly IResourceFactory _resourceFactory;
    private readonly Action<ResourceFieldAttribute, ResourceType, string>? _validateSingleFieldCallback;
    private ResourceType? _resourceTypeInScope;

    public FilterParser(IResourceFactory resourceFactory, Action<ResourceFieldAttribute, ResourceType, string>? validateSingleFieldCallback = null)
    {
        ArgumentGuard.NotNull(resourceFactory);

        _resourceFactory = resourceFactory;
        _validateSingleFieldCallback = validateSingleFieldCallback;
    }

    public FilterExpression Parse(string source, ResourceType resourceTypeInScope)
    {
        ArgumentGuard.NotNull(resourceTypeInScope);

        return InScopeOfResourceType(resourceTypeInScope, () =>
        {
            Tokenize(source);

            FilterExpression expression = ParseFilter();

            AssertTokenStackIsEmpty();

            return expression;
        });
    }

    protected FilterExpression ParseFilter()
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

        throw new QueryParseException("Filter function expected.");
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
        FieldChainRequirements leftChainRequirements = comparisonOperator == ComparisonOperator.Equals
            ? FieldChainRequirements.EndsInAttribute | FieldChainRequirements.EndsInToOne
            : FieldChainRequirements.EndsInAttribute;

        QueryExpression leftTerm = ParseCountOrField(leftChainRequirements);

        EatSingleCharacterToken(TokenKind.Comma);

        QueryExpression rightTerm;

        if (leftTerm is CountExpression)
        {
            Converter<string, object> rightConstantValueConverter = GetConstantValueConverterForCount();
            rightTerm = ParseCountOrConstantOrField(FieldChainRequirements.EndsInAttribute, rightConstantValueConverter);
        }
        else if (leftTerm is ResourceFieldChainExpression fieldChain && fieldChain.Fields[^1] is AttrAttribute attribute)
        {
            Converter<string, object> rightConstantValueConverter = GetConstantValueConverterForAttribute(attribute);
            rightTerm = ParseCountOrConstantOrNullOrField(FieldChainRequirements.EndsInAttribute, rightConstantValueConverter);
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

        ResourceFieldChainExpression targetAttributeChain = ParseFieldChain(FieldChainRequirements.EndsInAttribute, null);
        Type targetAttributeType = ((AttrAttribute)targetAttributeChain.Fields[^1]).Property.PropertyType;

        if (targetAttributeType != typeof(string))
        {
            throw new QueryParseException("Attribute of type 'String' expected.");
        }

        EatSingleCharacterToken(TokenKind.Comma);

        Converter<string, object> constantValueConverter = stringValue => stringValue;
        LiteralConstantExpression constant = ParseConstant(constantValueConverter);

        EatSingleCharacterToken(TokenKind.CloseParen);

        var matchKind = Enum.Parse<TextMatchKind>(matchFunctionName.Pascalize());
        return new MatchTextExpression(targetAttributeChain, constant, matchKind);
    }

    protected AnyExpression ParseAny()
    {
        EatText(Keywords.Any);
        EatSingleCharacterToken(TokenKind.OpenParen);

        ResourceFieldChainExpression targetAttributeChain = ParseFieldChain(FieldChainRequirements.EndsInAttribute, null);
        var targetAttribute = (AttrAttribute)targetAttributeChain.Fields[^1];
        Converter<string, object> constantValueConverter = GetConstantValueConverterForAttribute(targetAttribute);

        EatSingleCharacterToken(TokenKind.Comma);

        ImmutableHashSet<LiteralConstantExpression>.Builder constantsBuilder = ImmutableHashSet.CreateBuilder<LiteralConstantExpression>();

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

        ResourceFieldChainExpression targetCollection = ParseFieldChain(FieldChainRequirements.EndsInToMany, null);
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

        ResourceType baseType = targetToOneRelationship != null ? ((RelationshipAttribute)targetToOneRelationship.Fields[^1]).RightType : _resourceTypeInScope!;
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

        return ParseFieldChain(FieldChainRequirements.EndsInToOne, "Relationship name or , expected.");
    }

    private ResourceType ParseDerivedType(ResourceType baseType)
    {
        if (TokenStack.TryPop(out Token? token) && token.Kind == TokenKind.Text)
        {
            string derivedTypeName = token.Value!;
            return ResolveDerivedType(baseType, derivedTypeName);
        }

        throw new QueryParseException("Resource type expected.");
    }

    private ResourceType ResolveDerivedType(ResourceType baseType, string derivedTypeName)
    {
        ResourceType? derivedType = GetDerivedType(baseType, derivedTypeName);

        if (derivedType == null)
        {
            throw new QueryParseException($"Resource type '{derivedTypeName}' does not exist or does not derive from '{baseType.PublicName}'.");
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

    protected QueryExpression ParseCountOrField(FieldChainRequirements chainRequirements)
    {
        CountExpression? count = TryParseCount();

        if (count != null)
        {
            return count;
        }

        return ParseFieldChain(chainRequirements, "Count function or field name expected.");
    }

    protected QueryExpression ParseCountOrConstantOrField(FieldChainRequirements chainRequirements, Converter<string, object> constantValueConverter)
    {
        CountExpression? count = TryParseCount();

        if (count != null)
        {
            return count;
        }

        LiteralConstantExpression? constant = TryParseConstant(constantValueConverter);

        if (constant != null)
        {
            return constant;
        }

        return ParseFieldChain(chainRequirements, "Count function, value between quotes or field name expected.");
    }

    protected QueryExpression ParseCountOrConstantOrNullOrField(FieldChainRequirements chainRequirements, Converter<string, object> constantValueConverter)
    {
        CountExpression? count = TryParseCount();

        if (count != null)
        {
            return count;
        }

        IdentifierExpression? constantOrNull = TryParseConstantOrNull(constantValueConverter);

        if (constantOrNull != null)
        {
            return constantOrNull;
        }

        return ParseFieldChain(chainRequirements, "Count function, value between quotes, null or field name expected.");
    }

    protected LiteralConstantExpression? TryParseConstant(Converter<string, object> constantValueConverter)
    {
        if (TokenStack.TryPeek(out Token? nextToken) && nextToken.Kind == TokenKind.QuotedText)
        {
            TokenStack.Pop();

            object constantValue = constantValueConverter(nextToken.Value!);
            return new LiteralConstantExpression(constantValue, nextToken.Value!);
        }

        return null;
    }

    protected IdentifierExpression? TryParseConstantOrNull(Converter<string, object> constantValueConverter)
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

                object constantValue = constantValueConverter(nextToken.Value!);
                return new LiteralConstantExpression(constantValue, nextToken.Value!);
            }
        }

        return null;
    }

    protected LiteralConstantExpression ParseConstant(Converter<string, object> constantValueConverter)
    {
        LiteralConstantExpression? constant = TryParseConstant(constantValueConverter);

        if (constant == null)
        {
            throw new QueryParseException("Value between quotes expected.");
        }

        return constant;
    }

    protected NullConstantExpression ParseNull()
    {
        if (TokenStack.TryPop(out Token? token) && token is { Kind: TokenKind.Text, Value: Keywords.Null })
        {
            return NullConstantExpression.Instance;
        }

        throw new QueryParseException("null expected.");
    }

    private Converter<string, object> GetConstantValueConverterForCount()
    {
        return stringValue => ConvertStringToType(stringValue, typeof(int));
    }

    private object ConvertStringToType(string value, Type type)
    {
        try
        {
            return RuntimeTypeConverter.ConvertType(value, type)!;
        }
        catch (FormatException)
        {
            throw new QueryParseException($"Failed to convert '{value}' of type 'String' to type '{type.Name}'.");
        }
    }

    private Converter<string, object> GetConstantValueConverterForAttribute(AttrAttribute attribute)
    {
        return stringValue => attribute.Property.Name == nameof(Identifiable<object>.Id)
            ? DeObfuscateStringId(attribute.Type.ClrType, stringValue)
            : ConvertStringToType(stringValue, attribute.Property.PropertyType);
    }

    private object DeObfuscateStringId(Type resourceClrType, string stringId)
    {
        IIdentifiable tempResource = _resourceFactory.CreateInstance(resourceClrType);
        tempResource.StringId = stringId;
        return tempResource.GetTypedId();
    }

    protected override IImmutableList<ResourceFieldAttribute> OnResolveFieldChain(string path, FieldChainRequirements chainRequirements)
    {
        if (chainRequirements == FieldChainRequirements.EndsInToMany)
        {
            return ChainResolver.ResolveToOneChainEndingInToMany(_resourceTypeInScope!, path, FieldChainInheritanceRequirement.Disabled,
                _validateSingleFieldCallback);
        }

        if (chainRequirements == FieldChainRequirements.EndsInAttribute)
        {
            return ChainResolver.ResolveToOneChainEndingInAttribute(_resourceTypeInScope!, path, FieldChainInheritanceRequirement.Disabled,
                _validateSingleFieldCallback);
        }

        if (chainRequirements == FieldChainRequirements.EndsInToOne)
        {
            return ChainResolver.ResolveToOneChain(_resourceTypeInScope!, path, _validateSingleFieldCallback);
        }

        if (chainRequirements.HasFlag(FieldChainRequirements.EndsInAttribute) && chainRequirements.HasFlag(FieldChainRequirements.EndsInToOne))
        {
            return ChainResolver.ResolveToOneChainEndingInAttributeOrToOne(_resourceTypeInScope!, path, _validateSingleFieldCallback);
        }

        throw new InvalidOperationException($"Unexpected combination of chain requirement flags '{chainRequirements}'.");
    }

    private TResult InScopeOfResourceType<TResult>(ResourceType resourceType, Func<TResult> action)
    {
        ResourceType? backupType = _resourceTypeInScope;

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
