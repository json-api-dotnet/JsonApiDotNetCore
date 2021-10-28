using System;
using System.Collections.Immutable;
using System.Reflection;
using Humanizer;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal.Parsing
{
    [PublicAPI]
    public class FilterParser : QueryExpressionParser
    {
        private readonly IResourceFactory _resourceFactory;
        private readonly Action<ResourceFieldAttribute, ResourceType, string> _validateSingleFieldCallback;
        private ResourceType _resourceTypeInScope;

        public FilterParser(IResourceFactory resourceFactory, Action<ResourceFieldAttribute, ResourceType, string> validateSingleFieldCallback = null)
        {
            ArgumentGuard.NotNull(resourceFactory, nameof(resourceFactory));

            _resourceFactory = resourceFactory;
            _validateSingleFieldCallback = validateSingleFieldCallback;
        }

        public FilterExpression Parse(string source, ResourceType resourceTypeInScope)
        {
            ArgumentGuard.NotNull(resourceTypeInScope, nameof(resourceTypeInScope));

            _resourceTypeInScope = resourceTypeInScope;

            Tokenize(source);

            FilterExpression expression = ParseFilter();

            AssertTokenStackIsEmpty();

            return expression;
        }

        protected FilterExpression ParseFilter()
        {
            if (TokenStack.TryPeek(out Token nextToken) && nextToken.Kind == TokenKind.Text)
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

            while (TokenStack.TryPeek(out Token nextToken) && nextToken.Kind == TokenKind.Comma)
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

            // Allow equality comparison of a HasOne relationship with null.
            FieldChainRequirements leftChainRequirements = comparisonOperator == ComparisonOperator.Equals
                ? FieldChainRequirements.EndsInAttribute | FieldChainRequirements.EndsInToOne
                : FieldChainRequirements.EndsInAttribute;

            QueryExpression leftTerm = ParseCountOrField(leftChainRequirements);

            EatSingleCharacterToken(TokenKind.Comma);

            QueryExpression rightTerm = ParseCountOrConstantOrNullOrField(FieldChainRequirements.EndsInAttribute);

            EatSingleCharacterToken(TokenKind.CloseParen);

            if (leftTerm is ResourceFieldChainExpression leftChain)
            {
                if (leftChainRequirements.HasFlag(FieldChainRequirements.EndsInToOne) && rightTerm is not NullConstantExpression)
                {
                    // Run another pass over left chain to have it fail when chain ends in relationship.
                    OnResolveFieldChain(leftChain.ToString(), FieldChainRequirements.EndsInAttribute);
                }

                PropertyInfo leftProperty = leftChain.Fields[^1].Property;

                if (leftProperty.Name == nameof(Identifiable<object>.Id) && rightTerm is LiteralConstantExpression rightConstant)
                {
                    string id = DeObfuscateStringId(leftProperty.ReflectedType, rightConstant.Value);
                    rightTerm = new LiteralConstantExpression(id);
                }
            }

            return new ComparisonExpression(comparisonOperator, leftTerm, rightTerm);
        }

        protected MatchTextExpression ParseTextMatch(string matchFunctionName)
        {
            EatText(matchFunctionName);
            EatSingleCharacterToken(TokenKind.OpenParen);

            ResourceFieldChainExpression targetAttribute = ParseFieldChain(FieldChainRequirements.EndsInAttribute, null);

            EatSingleCharacterToken(TokenKind.Comma);

            LiteralConstantExpression constant = ParseConstant();

            EatSingleCharacterToken(TokenKind.CloseParen);

            var matchKind = Enum.Parse<TextMatchKind>(matchFunctionName.Pascalize());
            return new MatchTextExpression(targetAttribute, constant, matchKind);
        }

        protected AnyExpression ParseAny()
        {
            EatText(Keywords.Any);
            EatSingleCharacterToken(TokenKind.OpenParen);

            ResourceFieldChainExpression targetAttribute = ParseFieldChain(FieldChainRequirements.EndsInAttribute, null);

            EatSingleCharacterToken(TokenKind.Comma);

            ImmutableHashSet<LiteralConstantExpression>.Builder constantsBuilder = ImmutableHashSet.CreateBuilder<LiteralConstantExpression>();

            LiteralConstantExpression constant = ParseConstant();
            constantsBuilder.Add(constant);

            EatSingleCharacterToken(TokenKind.Comma);

            constant = ParseConstant();
            constantsBuilder.Add(constant);

            while (TokenStack.TryPeek(out Token nextToken) && nextToken.Kind == TokenKind.Comma)
            {
                EatSingleCharacterToken(TokenKind.Comma);

                constant = ParseConstant();
                constantsBuilder.Add(constant);
            }

            EatSingleCharacterToken(TokenKind.CloseParen);

            IImmutableSet<LiteralConstantExpression> constantSet = constantsBuilder.ToImmutable();

            PropertyInfo targetAttributeProperty = targetAttribute.Fields[^1].Property;

            if (targetAttributeProperty.Name == nameof(Identifiable<object>.Id))
            {
                constantSet = DeObfuscateIdConstants(constantSet, targetAttributeProperty);
            }

            return new AnyExpression(targetAttribute, constantSet);
        }

        private IImmutableSet<LiteralConstantExpression> DeObfuscateIdConstants(IImmutableSet<LiteralConstantExpression> constantSet,
            PropertyInfo targetAttributeProperty)
        {
            ImmutableHashSet<LiteralConstantExpression>.Builder idConstantsBuilder = ImmutableHashSet.CreateBuilder<LiteralConstantExpression>();

            foreach (LiteralConstantExpression idConstant in constantSet)
            {
                string stringId = idConstant.Value;
                string id = DeObfuscateStringId(targetAttributeProperty.ReflectedType, stringId);

                idConstantsBuilder.Add(new LiteralConstantExpression(id));
            }

            return idConstantsBuilder.ToImmutable();
        }

        protected HasExpression ParseHas()
        {
            EatText(Keywords.Has);
            EatSingleCharacterToken(TokenKind.OpenParen);

            ResourceFieldChainExpression targetCollection = ParseFieldChain(FieldChainRequirements.EndsInToMany, null);
            FilterExpression filter = null;

            if (TokenStack.TryPeek(out Token nextToken) && nextToken.Kind == TokenKind.Comma)
            {
                EatSingleCharacterToken(TokenKind.Comma);

                filter = ParseFilterInHas((HasManyAttribute)targetCollection.Fields[^1]);
            }

            EatSingleCharacterToken(TokenKind.CloseParen);

            return new HasExpression(targetCollection, filter);
        }

        private FilterExpression ParseFilterInHas(HasManyAttribute hasManyRelationship)
        {
            ResourceType outerScopeBackup = _resourceTypeInScope;

            _resourceTypeInScope = hasManyRelationship.RightType;

            FilterExpression filter = ParseFilter();

            _resourceTypeInScope = outerScopeBackup;
            return filter;
        }

        protected QueryExpression ParseCountOrField(FieldChainRequirements chainRequirements)
        {
            CountExpression count = TryParseCount();

            if (count != null)
            {
                return count;
            }

            return ParseFieldChain(chainRequirements, "Count function or field name expected.");
        }

        protected QueryExpression ParseCountOrConstantOrNullOrField(FieldChainRequirements chainRequirements)
        {
            CountExpression count = TryParseCount();

            if (count != null)
            {
                return count;
            }

            IdentifierExpression constantOrNull = TryParseConstantOrNull();

            if (constantOrNull != null)
            {
                return constantOrNull;
            }

            return ParseFieldChain(chainRequirements, "Count function, value between quotes, null or field name expected.");
        }

        protected IdentifierExpression TryParseConstantOrNull()
        {
            if (TokenStack.TryPeek(out Token nextToken))
            {
                if (nextToken.Kind == TokenKind.Text && nextToken.Value == Keywords.Null)
                {
                    TokenStack.Pop();
                    return new NullConstantExpression();
                }

                if (nextToken.Kind == TokenKind.QuotedText)
                {
                    TokenStack.Pop();
                    return new LiteralConstantExpression(nextToken.Value);
                }
            }

            return null;
        }

        protected LiteralConstantExpression ParseConstant()
        {
            if (TokenStack.TryPop(out Token token) && token.Kind == TokenKind.QuotedText)
            {
                return new LiteralConstantExpression(token.Value);
            }

            throw new QueryParseException("Value between quotes expected.");
        }

        private string DeObfuscateStringId(Type resourceClrType, string stringId)
        {
            IIdentifiable tempResource = _resourceFactory.CreateInstance(resourceClrType);
            tempResource.StringId = stringId;
            return tempResource.GetTypedId().ToString();
        }

        protected override IImmutableList<ResourceFieldAttribute> OnResolveFieldChain(string path, FieldChainRequirements chainRequirements)
        {
            if (chainRequirements == FieldChainRequirements.EndsInToMany)
            {
                return ChainResolver.ResolveToOneChainEndingInToMany(_resourceTypeInScope, path, _validateSingleFieldCallback);
            }

            if (chainRequirements == FieldChainRequirements.EndsInAttribute)
            {
                return ChainResolver.ResolveToOneChainEndingInAttribute(_resourceTypeInScope, path, _validateSingleFieldCallback);
            }

            if (chainRequirements.HasFlag(FieldChainRequirements.EndsInAttribute) && chainRequirements.HasFlag(FieldChainRequirements.EndsInToOne))
            {
                return ChainResolver.ResolveToOneChainEndingInAttributeOrToOne(_resourceTypeInScope, path, _validateSingleFieldCallback);
            }

            throw new InvalidOperationException($"Unexpected combination of chain requirement flags '{chainRequirements}'.");
        }
    }
}
