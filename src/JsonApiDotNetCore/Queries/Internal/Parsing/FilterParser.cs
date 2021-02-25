using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly Action<ResourceFieldAttribute, ResourceContext, string> _validateSingleFieldCallback;
        private ResourceContext _resourceContextInScope;

        public FilterParser(IResourceContextProvider resourceContextProvider, IResourceFactory resourceFactory,
            Action<ResourceFieldAttribute, ResourceContext, string> validateSingleFieldCallback = null)
            : base(resourceContextProvider)
        {
            ArgumentGuard.NotNull(resourceFactory, nameof(resourceFactory));

            _resourceFactory = resourceFactory;
            _validateSingleFieldCallback = validateSingleFieldCallback;
        }

        public FilterExpression Parse(string source, ResourceContext resourceContextInScope)
        {
            ArgumentGuard.NotNull(resourceContextInScope, nameof(resourceContextInScope));

            _resourceContextInScope = resourceContextInScope;

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

            var terms = new List<QueryExpression>();

            FilterExpression term = ParseFilter();
            terms.Add(term);

            EatSingleCharacterToken(TokenKind.Comma);

            term = ParseFilter();
            terms.Add(term);

            while (TokenStack.TryPeek(out Token nextToken) && nextToken.Kind == TokenKind.Comma)
            {
                EatSingleCharacterToken(TokenKind.Comma);

                term = ParseFilter();
                terms.Add(term);
            }

            EatSingleCharacterToken(TokenKind.CloseParen);

            var logicalOperator = Enum.Parse<LogicalOperator>(operatorName.Pascalize());
            return new LogicalExpression(logicalOperator, terms);
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
                if (leftChainRequirements.HasFlag(FieldChainRequirements.EndsInToOne) && !(rightTerm is NullConstantExpression))
                {
                    // Run another pass over left chain to have it fail when chain ends in relationship.
                    OnResolveFieldChain(leftChain.ToString(), FieldChainRequirements.EndsInAttribute);
                }

                PropertyInfo leftProperty = leftChain.Fields.Last().Property;

                if (leftProperty.Name == nameof(Identifiable.Id) && rightTerm is LiteralConstantExpression rightConstant)
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

        protected EqualsAnyOfExpression ParseAny()
        {
            EatText(Keywords.Any);
            EatSingleCharacterToken(TokenKind.OpenParen);

            ResourceFieldChainExpression targetAttribute = ParseFieldChain(FieldChainRequirements.EndsInAttribute, null);

            EatSingleCharacterToken(TokenKind.Comma);

            var constants = new List<LiteralConstantExpression>();

            LiteralConstantExpression constant = ParseConstant();
            constants.Add(constant);

            EatSingleCharacterToken(TokenKind.Comma);

            constant = ParseConstant();
            constants.Add(constant);

            while (TokenStack.TryPeek(out Token nextToken) && nextToken.Kind == TokenKind.Comma)
            {
                EatSingleCharacterToken(TokenKind.Comma);

                constant = ParseConstant();
                constants.Add(constant);
            }

            EatSingleCharacterToken(TokenKind.CloseParen);

            PropertyInfo targetAttributeProperty = targetAttribute.Fields.Last().Property;

            if (targetAttributeProperty.Name == nameof(Identifiable.Id))
            {
                for (int index = 0; index < constants.Count; index++)
                {
                    string stringId = constants[index].Value;
                    string id = DeObfuscateStringId(targetAttributeProperty.ReflectedType, stringId);
                    constants[index] = new LiteralConstantExpression(id);
                }
            }

            return new EqualsAnyOfExpression(targetAttribute, constants);
        }

        protected CollectionNotEmptyExpression ParseHas()
        {
            EatText(Keywords.Has);
            EatSingleCharacterToken(TokenKind.OpenParen);

            ResourceFieldChainExpression targetCollection = ParseFieldChain(FieldChainRequirements.EndsInToMany, null);

            EatSingleCharacterToken(TokenKind.CloseParen);

            return new CollectionNotEmptyExpression(targetCollection);
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

        private string DeObfuscateStringId(Type resourceType, string stringId)
        {
            IIdentifiable tempResource = _resourceFactory.CreateInstance(resourceType);
            tempResource.StringId = stringId;
            return tempResource.GetTypedId().ToString();
        }

        protected override IReadOnlyCollection<ResourceFieldAttribute> OnResolveFieldChain(string path, FieldChainRequirements chainRequirements)
        {
            if (chainRequirements == FieldChainRequirements.EndsInToMany)
            {
                return ChainResolver.ResolveToOneChainEndingInToMany(_resourceContextInScope, path, _validateSingleFieldCallback);
            }

            if (chainRequirements == FieldChainRequirements.EndsInAttribute)
            {
                return ChainResolver.ResolveToOneChainEndingInAttribute(_resourceContextInScope, path, _validateSingleFieldCallback);
            }

            if (chainRequirements.HasFlag(FieldChainRequirements.EndsInAttribute) && chainRequirements.HasFlag(FieldChainRequirements.EndsInToOne))
            {
                return ChainResolver.ResolveToOneChainEndingInAttributeOrToOne(_resourceContextInScope, path, _validateSingleFieldCallback);
            }

            throw new InvalidOperationException($"Unexpected combination of chain requirement flags '{chainRequirements}'.");
        }
    }
}
