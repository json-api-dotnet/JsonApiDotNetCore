using System.Collections.Immutable;
using System.Diagnostics;
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
    private readonly Stack<FilterScope> _scopeStack = new();

    protected QueryParseChoices FieldOrElementChoice =>
        CurrentScope switch
        {
            ScalarCollectionElementFilterScope => QueryParseChoices.Underscore,
            AttributeContainerFilterScope => QueryParseChoices.FieldName,
            _ => throw new UnreachableException()
        };

    /// <summary>
    /// Gets the current filter scope. Call <see cref="InScopeOfContainer" /> or <see cref="InScopeOfScalarCollection" /> to temporarily change the current
    /// scope.
    /// </summary>
    protected FilterScope CurrentScope
    {
        get
        {
            if (_scopeStack.Count == 0)
            {
                throw new InvalidOperationException($"No filter scope is currently active. Call {nameof(Parse)}() first.");
            }

            return _scopeStack.Peek();
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

        _scopeStack.Clear();

        using (InScopeOfContainer(resourceType))
        {
            FilterExpression expression = ParseFilter();

            AssertTokenStackIsEmpty();
            AssertFilterScopeStackHasOnlyRoot();

            return expression;
        }
    }

    protected virtual bool IsFunction(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        return name == Keywords.Count || FilterKeywords.Contains(name);
    }

    protected virtual FunctionExpression ParseFunction()
    {
        if (TokenStack.TryPeek(out Token? nextToken) && nextToken is { Kind: TokenKind.Text, Value: Keywords.Count })
        {
            return ParseCount();
        }

        return ParseFilter();
    }

    protected virtual CountExpression ParseCount()
    {
        if (CurrentScope is not AttributeContainerFilterScope containerScope)
        {
            int position = GetNextTokenPositionOrEnd();
            throw new QueryParseException("Function 'count' cannot be used on scalar collection elements.", position);
        }

        EatText(Keywords.Count);
        EatSingleCharacterToken(TokenKind.OpenParen);

        ResourceFieldChainExpression targetCollection = ParseFieldChain(BuiltInPatterns.ToOneOrCompoundChainEndingInCollection, containerScope.Container);

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
        throw new QueryParseException(QueryParseChoices.FilterFunction, position);
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

        while (TryEatSingleCharacterToken(TokenKind.Comma))
        {
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
        bool isEqualityComparison = comparisonOperator == ComparisonOperator.Equals;

        EatText(operatorName);
        EatSingleCharacterToken(TokenKind.OpenParen);

        QueryExpression leftTerm = ParseComparisonLeftTerm(isEqualityComparison);

        EatSingleCharacterToken(TokenKind.Comma);

        QueryExpression rightTerm = ParseComparisonRightTerm(leftTerm, isEqualityComparison);

        EatSingleCharacterToken(TokenKind.CloseParen);

        return new ComparisonExpression(comparisonOperator, leftTerm, rightTerm);
    }

    private QueryExpression ParseComparisonLeftTerm(bool isEqualityComparison)
    {
        if (TokenStack.TryPeek(out Token? nextToken) && nextToken is { Kind: TokenKind.Text } && IsFunction(nextToken.Value!))
        {
            return ParseFunction();
        }

        if (CurrentScope is ScalarCollectionElementFilterScope collectionScope)
        {
            ScalarCollectionElementValueExpression? elementValue = TryParseScalarCollectionElement(collectionScope);

            if (elementValue != null)
            {
                return elementValue;
            }
        }
        else if (CurrentScope is AttributeContainerFilterScope containerScope)
        {
            if (TokenStack.TryPeek(out Token? token) && token.Kind == TokenKind.Text && !IsNullConstantToken(token))
            {
                // Allow equality comparison of a to-one relationship or attribute with null.
                FieldChainPattern pattern = isEqualityComparison
                    ? BuiltInPatterns.ToOneOrCompoundChainEndingInToOneOrAttribute
                    : BuiltInPatterns.ToOneOrCompoundChainEndingInScalarAttribute;

                return ParseFieldChain(pattern, containerScope.Container);
            }
        }

        int position = GetNextTokenPositionOrEnd();
        throw new QueryParseException(QueryParseChoices.Function | FieldOrElementChoice, position);
    }

    /// <summary>
    /// Attempts to parse the underscore (_), which represents the current scalar collection element.
    /// </summary>
    protected ScalarCollectionElementValueExpression? TryParseScalarCollectionElement(ScalarCollectionElementFilterScope collectionScope)
    {
        ArgumentNullException.ThrowIfNull(collectionScope);

        if (TryEatSingleCharacterToken(TokenKind.Underscore))
        {
            return new ScalarCollectionElementValueExpression(collectionScope.ElementType);
        }

        return null;
    }

    private QueryExpression ParseComparisonRightTerm(QueryExpression leftTerm, bool isEqualityComparison)
    {
        if (leftTerm is ResourceFieldChainExpression leftFieldChain)
        {
            ResourceFieldAttribute leftLastField = leftFieldChain.Fields[^1];

            if (leftLastField is HasOneAttribute)
            {
                return ParseToOneRelationshipComparisonRightTerm();
            }

            if (leftLastField is AttrAttribute { IsCompound: true } or AttrAttribute { IsCollection: true })
            {
                return ParseNull();
            }

            var leftAttribute = (AttrAttribute)leftLastField;

            ConstantValueConverter constantValueConverter = GetConstantValueConverterForAttribute(leftAttribute);
            return ParseTypedComparisonRightTerm(leftAttribute.Property.PropertyType, constantValueConverter, isEqualityComparison);
        }

        if (leftTerm is FunctionExpression leftFunction)
        {
            ConstantValueConverter constantValueConverter = GetConstantValueConverterForType(leftFunction.ReturnType);
            return ParseTypedComparisonRightTerm(leftFunction.ReturnType, constantValueConverter, isEqualityComparison);
        }

        if (leftTerm is ScalarCollectionElementValueExpression collectionElement)
        {
            ConstantValueConverter constantValueConverter = GetConstantValueConverterForType(collectionElement.ElementType);
            return ParseTypedComparisonRightTerm(collectionElement.ElementType, constantValueConverter, isEqualityComparison);
        }

        throw new UnreachableException();
    }

    private QueryExpression ParseToOneRelationshipComparisonRightTerm()
    {
        if (CurrentScope is not AttributeContainerFilterScope containerScope)
        {
            throw new UnreachableException();
        }

        if (TokenStack.TryPeek(out Token? nextToken))
        {
            if (IsNullConstantToken(nextToken))
            {
                TokenStack.Pop();
                return NullConstantExpression.Instance;
            }

            if (nextToken.Kind == TokenKind.Text && !IsFunction(nextToken.Value!))
            {
                return ParseFieldChain(BuiltInPatterns.ToOneChain, containerScope.Container);
            }
        }

        int position = GetNextTokenPositionOrEnd();
        throw new QueryParseException(QueryParseChoices.ToOneRelationshipName | QueryParseChoices.Null, position);
    }

    private QueryExpression ParseTypedComparisonRightTerm(Type leftType, ConstantValueConverter constantValueConverter, bool isEqualityComparison)
    {
        // Always allowing to compare equality with null makes the JSON:API contract resilient to internal implementation details.
        // This unifies the behavior for all attributes types (scalar/compound/collection) across nullability of reference/value types.
        // Comparing a non-nullable value type to null is a harmless predicate that will simply never match any database rows.
        bool allowNull = isEqualityComparison || RuntimeTypeConverter.CanContainNull(leftType);

        QueryParseChoices rightTermChoices = QueryParseChoices.Function | FieldOrElementChoice | QueryParseChoices.QuotedText;

        if (allowNull)
        {
            rightTermChoices |= QueryParseChoices.Null;
        }

        if (TokenStack.TryPeek(out Token? nextToken))
        {
            if (nextToken.Kind == TokenKind.QuotedText)
            {
                TokenStack.Pop();

                object constantValue = constantValueConverter(nextToken.Value!, nextToken.Position);
                return new LiteralConstantExpression(constantValue, nextToken.Value!);
            }

            if (CurrentScope is ScalarCollectionElementFilterScope collectionScope)
            {
                ScalarCollectionElementValueExpression? elementValue = TryParseScalarCollectionElement(collectionScope);

                if (elementValue != null)
                {
                    return elementValue;
                }
            }

            if (IsNullConstantToken(nextToken))
            {
                if (!allowNull)
                {
                    throw new QueryParseException(rightTermChoices, nextToken.Position);
                }

                TokenStack.Pop();
                return NullConstantExpression.Instance;
            }

            if (nextToken.Kind == TokenKind.Text)
            {
                if (IsFunction(nextToken.Value!))
                {
                    return ParseFunction();
                }

                if (CurrentScope is AttributeContainerFilterScope containerScope)
                {
                    return ParseFieldChain(BuiltInPatterns.ToOneOrCompoundChainEndingInScalarAttribute, containerScope.Container);
                }
            }
        }

        int position = GetNextTokenPositionOrEnd();
        throw new QueryParseException(rightTermChoices, position);
    }

    protected virtual MatchTextExpression ParseTextMatch(string operatorName)
    {
        ArgumentException.ThrowIfNullOrEmpty(operatorName);

        EatText(operatorName);
        EatSingleCharacterToken(TokenKind.OpenParen);

        QueryExpression matchTarget = ParseTextMatchLeftTerm();

        EatSingleCharacterToken(TokenKind.Comma);

        ConstantValueConverter constantValueConverter = GetConstantValueConverterForType(typeof(string));
        LiteralConstantExpression constant = ParseConstant(constantValueConverter);

        EatSingleCharacterToken(TokenKind.CloseParen);

        var matchKind = Enum.Parse<TextMatchKind>(operatorName.Pascalize());
        return new MatchTextExpression(matchTarget, constant, matchKind);
    }

    private QueryExpression ParseTextMatchLeftTerm()
    {
        if (TokenStack.TryPeek(out Token? nextToken) && nextToken is { Kind: TokenKind.Text } && IsFunction(nextToken.Value!))
        {
            FunctionExpression targetFunction = ParseFunction();

            if (targetFunction.ReturnType != typeof(string))
            {
                string actualTypeName = RuntimeTypeConverter.GetFriendlyTypeName(targetFunction.ReturnType);
                throw new QueryParseException($"Function returns type '{actualTypeName}' while type 'String' was expected.", nextToken.Position);
            }

            return targetFunction;
        }

        if (CurrentScope is ScalarCollectionElementFilterScope collectionScope)
        {
            int elementPosition = GetNextTokenPositionOrEnd();
            ScalarCollectionElementValueExpression? elementValue = TryParseScalarCollectionElement(collectionScope);

            if (elementValue != null)
            {
                if (elementValue.ElementType != typeof(string))
                {
                    string actualTypeName = RuntimeTypeConverter.GetFriendlyTypeName(elementValue.ElementType);
                    throw new QueryParseException($"Element is of type '{actualTypeName}' while type 'String' was expected.", elementPosition);
                }

                return elementValue;
            }

            throw new QueryParseException(QueryParseChoices.Function | FieldOrElementChoice, elementPosition);
        }

        if (CurrentScope is AttributeContainerFilterScope containerScope)
        {
            if (TokenStack.TryPeek(out Token? token) && token.Kind == TokenKind.Text && !IsNullConstantToken(token))
            {
                int chainStartPosition = GetNextTokenPositionOrEnd();

                ResourceFieldChainExpression targetAttributeChain = ParseFieldChain(BuiltInPatterns.ToOneOrCompoundChainEndingInScalarAttribute,
                    containerScope.Container);

                var targetAttribute = (AttrAttribute)targetAttributeChain.Fields[^1];

                if (targetAttribute.Property.PropertyType != typeof(string))
                {
                    int attrPosition = chainStartPosition + GetRelativePositionOfLastFieldInChain(targetAttributeChain);
                    string actualTypeName = RuntimeTypeConverter.GetFriendlyTypeName(targetAttribute.Property.PropertyType);

                    throw new QueryParseException($"Attribute '{targetAttribute.PublicName}' is of type '{actualTypeName}' while type 'String' was expected.",
                        attrPosition);
                }

                return targetAttributeChain;
            }
        }

        int position = GetNextTokenPositionOrEnd();
        throw new QueryParseException(QueryParseChoices.Function | FieldOrElementChoice, position);
    }

    protected virtual AnyExpression ParseAny()
    {
        EatText(Keywords.Any);
        EatSingleCharacterToken(TokenKind.OpenParen);

        (QueryExpression matchTarget, Func<ConstantValueConverter> constantValueConverterFactory) = ParseAnyLeftTerm();

        EatSingleCharacterToken(TokenKind.Comma);

        ImmutableHashSet<LiteralConstantExpression>.Builder constantsBuilder = ImmutableHashSet.CreateBuilder<LiteralConstantExpression>();

        ConstantValueConverter constantValueConverter = constantValueConverterFactory();
        LiteralConstantExpression constant = ParseConstant(constantValueConverter);
        constantsBuilder.Add(constant);

        while (TryEatSingleCharacterToken(TokenKind.Comma))
        {
            constant = ParseConstant(constantValueConverter);
            constantsBuilder.Add(constant);
        }

        EatSingleCharacterToken(TokenKind.CloseParen);

        IImmutableSet<LiteralConstantExpression> constantSet = constantsBuilder.ToImmutable();

        return new AnyExpression(matchTarget, constantSet);
    }

    private (QueryExpression matchTarget, Func<ConstantValueConverter> constantValueConverterFactory) ParseAnyLeftTerm()
    {
        if (TokenStack.TryPeek(out Token? nextToken) && nextToken is { Kind: TokenKind.Text } && IsFunction(nextToken.Value!))
        {
            FunctionExpression targetFunction = ParseFunction();

            Func<ConstantValueConverter> converterFactory = () => GetConstantValueConverterForType(targetFunction.ReturnType);
            return (targetFunction, converterFactory);
        }

        if (CurrentScope is ScalarCollectionElementFilterScope collectionScope)
        {
            ScalarCollectionElementValueExpression? elementValue = TryParseScalarCollectionElement(collectionScope);

            if (elementValue != null)
            {
                Func<ConstantValueConverter> converterFactory = () => GetConstantValueConverterForType(elementValue.ElementType);
                return (elementValue, converterFactory);
            }
        }
        else if (CurrentScope is AttributeContainerFilterScope containerScope)
        {
            if (TokenStack.TryPeek(out Token? token) && token.Kind == TokenKind.Text && !IsNullConstantToken(token))
            {
                ResourceFieldChainExpression targetAttributeChain = ParseFieldChain(BuiltInPatterns.ToOneOrCompoundChainEndingInScalarAttribute,
                    containerScope.Container);

                var targetAttribute = (AttrAttribute)targetAttributeChain.Fields[^1];

                Func<ConstantValueConverter> converterFactory = () => GetConstantValueConverterForAttribute(targetAttribute);
                return (targetAttributeChain, converterFactory);
            }
        }

        int position = GetNextTokenPositionOrEnd();
        throw new QueryParseException(QueryParseChoices.Function | FieldOrElementChoice, position);
    }

    protected virtual HasExpression ParseHas()
    {
        if (CurrentScope is not AttributeContainerFilterScope containerScope)
        {
            int position = GetNextTokenPositionOrEnd();
            throw new QueryParseException("Function 'has' cannot be used on scalar collection elements.", position);
        }

        EatText(Keywords.Has);
        EatSingleCharacterToken(TokenKind.OpenParen);

        ResourceFieldChainExpression targetCollection = ParseFieldChain(BuiltInPatterns.ToOneOrCompoundChainEndingInCollection, containerScope.Container);

        FilterExpression? filter = null;

        if (TryEatSingleCharacterToken(TokenKind.Comma))
        {
            ResourceFieldAttribute lastField = targetCollection.Fields[^1];

            if (lastField is HasManyAttribute hasManyRelationship)
            {
                using (InScopeOfContainer(hasManyRelationship.RightType))
                {
                    filter = ParseFilter();
                }
            }
            else if (lastField is AttrAttribute { IsCompound: true, IsCollection: true } compoundCollectionAttr)
            {
                using (InScopeOfContainer(compoundCollectionAttr.CompoundType!))
                {
                    filter = ParseFilter();
                }
            }
            else if (lastField is AttrAttribute { IsCompound: false, IsCollection: true } scalarCollectionAttr)
            {
                Type elementType = CollectionConverter.Instance.FindCollectionElementType(scalarCollectionAttr.Property.PropertyType)!;

                using (InScopeOfScalarCollection(elementType))
                {
                    filter = ParseFilter();
                }
            }
        }

        EatSingleCharacterToken(TokenKind.CloseParen);

        return new HasExpression(targetCollection, filter);
    }

    protected virtual IsTypeExpression ParseIsType()
    {
        if (CurrentScope is not AttributeContainerFilterScope { Container: ResourceType resourceTypeInScope })
        {
            int position = GetNextTokenPositionOrEnd();
            throw new QueryParseException("Function 'isType' can only be used on resource types.", position);
        }

        EatText(Keywords.IsType);
        EatSingleCharacterToken(TokenKind.OpenParen);

        ResourceFieldChainExpression? targetToOneRelationship = TryParseCommaSeparatedSequenceOfToOneRelationshipChains(resourceTypeInScope);

        EatSingleCharacterToken(TokenKind.Comma);

        ResourceType baseType = targetToOneRelationship != null ? ((RelationshipAttribute)targetToOneRelationship.Fields[^1]).RightType : resourceTypeInScope;
        ResourceType derivedType = ParseDerivedType(baseType);

        FilterExpression? child = TryParseFilterInIsType(derivedType);

        EatSingleCharacterToken(TokenKind.CloseParen);

        return new IsTypeExpression(targetToOneRelationship, derivedType, child);
    }

    private ResourceFieldChainExpression? TryParseCommaSeparatedSequenceOfToOneRelationshipChains(ResourceType resourceTypeInScope)
    {
        if (TokenStack.TryPeek(out Token? nextToken))
        {
            if (nextToken.Kind == TokenKind.Comma)
            {
                return null;
            }

            if (nextToken.Kind == TokenKind.Text && !IsNullConstantToken(nextToken))
            {
                return ParseFieldChain(BuiltInPatterns.ToOneChain, resourceTypeInScope);
            }
        }

        int position = GetNextTokenPositionOrEnd();
        throw new QueryParseException(QueryParseChoices.ToOneRelationshipName | QueryParseChoices.Comma, position);
    }

    private ResourceType ParseDerivedType(ResourceType baseType)
    {
        int position = GetNextTokenPositionOrEnd();

        if (TokenStack.TryPop(out Token? token) && token.Kind == TokenKind.Text)
        {
            string derivedTypeName = token.Value!;
            return ResolveDerivedType(baseType, derivedTypeName, token.Position);
        }

        throw new QueryParseException(QueryParseChoices.ResourceType, position);
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

        if (TryEatSingleCharacterToken(TokenKind.Comma))
        {
            using (InScopeOfContainer(derivedType))
            {
                filter = ParseFilter();
            }
        }

        return filter;
    }

    protected virtual LiteralConstantExpression ParseConstant(ConstantValueConverter constantValueConverter)
    {
        ArgumentNullException.ThrowIfNull(constantValueConverter);

        int position = GetNextTokenPositionOrEnd();

        if (TokenStack.TryPop(out Token? token) && token.Kind == TokenKind.QuotedText)
        {
            object constantValue = constantValueConverter(token.Value!, token.Position);
            return new LiteralConstantExpression(constantValue, token.Value!);
        }

        throw new QueryParseException(QueryParseChoices.QuotedText, position);
    }

    protected virtual NullConstantExpression ParseNull()
    {
        int position = GetNextTokenPositionOrEnd();

        if (TokenStack.TryPop(out Token? token) && IsNullConstantToken(token))
        {
            return NullConstantExpression.Instance;
        }

        throw new QueryParseException(QueryParseChoices.Null, position);
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
                throw new QueryParseException($"Failed to convert '{stringValue}' to type '{destinationTypeName}'.", position, exception);
            }
        };
    }

    private ConstantValueConverter GetConstantValueConverterForAttribute(AttrAttribute attribute)
    {
        if (attribute is { Property.Name: nameof(Identifiable<>.Id), ContainingType: ResourceType resourceType })
        {
            return (stringValue, position) =>
            {
                try
                {
                    return DeObfuscateStringId(resourceType, stringValue);
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
    /// Changes the current filter scope to the specified attribute container (resource type or compound type) and restores the original scope when the
    /// return value is disposed.
    /// </summary>
    protected IDisposable InScopeOfContainer(IAttributeContainer container)
    {
        ArgumentNullException.ThrowIfNull(container);

        var scope = new AttributeContainerFilterScope(container);
        return PushFilterScope(scope);
    }

    /// <summary>
    /// Changes the current filter scope to the specified scalar attribute collection type and restores the original scope when the return value is disposed.
    /// </summary>
    protected IDisposable InScopeOfScalarCollection(Type collectionElementType)
    {
        ArgumentNullException.ThrowIfNull(collectionElementType);

        var scope = new ScalarCollectionElementFilterScope(collectionElementType);
        return PushFilterScope(scope);
    }

    private PopFilterScopeOnDispose PushFilterScope(FilterScope scope)
    {
        _scopeStack.Push(scope);
        return new PopFilterScopeOnDispose(_scopeStack);
    }

    private void AssertFilterScopeStackHasOnlyRoot()
    {
        if (_scopeStack.Count > 1)
        {
            FilterScope scopeAtTopOfStack = _scopeStack.Peek();

            if (scopeAtTopOfStack is AttributeContainerFilterScope containerScope)
            {
                string containerType = containerScope.Container is ResourceType ? "resource type" : "compound type";

                throw new InvalidOperationException($"There is still a {containerType} in scope after parsing has completed. " +
                    $"Verify that {nameof(IDisposable.Dispose)}() is called on all return values of {nameof(InScopeOfContainer)}().");
            }

            throw new InvalidOperationException("There is still a scalar collection element type in scope after parsing has completed. " +
                $"Verify that {nameof(IDisposable.Dispose)}() is called on all return values of {nameof(InScopeOfScalarCollection)}().");
        }
    }

    private sealed class PopFilterScopeOnDispose(Stack<FilterScope> scopeStack) : IDisposable
    {
        private readonly Stack<FilterScope> _scopeStack = scopeStack;

        public void Dispose()
        {
            _scopeStack.Pop();
        }
    }
}
