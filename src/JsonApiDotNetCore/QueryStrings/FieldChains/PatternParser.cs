namespace JsonApiDotNetCore.QueryStrings.FieldChains;

/// <summary>
/// Parses a field chain pattern from text into a chain of <see cref="FieldChainPattern" /> segments.
/// </summary>
internal sealed class PatternParser
{
    private static readonly Dictionary<char, Token> CharToTokenTable = new()
    {
        ['?'] = Token.QuestionMark,
        ['+'] = Token.Plus,
        ['*'] = Token.Asterisk,
        ['['] = Token.BracketOpen,
        [']'] = Token.BracketClose,
        ['M'] = Token.ToManyRelationship,
        ['O'] = Token.ToOneRelationship,
        ['R'] = Token.Relationship,
        ['A'] = Token.Attribute,
        ['F'] = Token.Field
    };

    private static readonly Dictionary<Token, FieldTypes> TokenToFieldTypeTable = new()
    {
        [Token.ToManyRelationship] = FieldTypes.ToManyRelationship,
        [Token.ToOneRelationship] = FieldTypes.ToOneRelationship,
        [Token.Relationship] = FieldTypes.Relationship,
        [Token.Attribute] = FieldTypes.Attribute,
        [Token.Field] = FieldTypes.Field
    };

    private static readonly HashSet<Token> QuantifierTokens =
    [
        Token.QuestionMark,
        Token.Plus,
        Token.Asterisk
    ];

    private string _source = null!;
    private Queue<Token> _tokenQueue = null!;
    private int _position;

    public FieldChainPattern Parse(string source)
    {
        ArgumentNullException.ThrowIfNull(source);

        _source = source;
        EnqueueTokens();

        _position = 0;
        FieldChainPattern? pattern = TryParsePatternChain();

        if (pattern == null)
        {
            throw new PatternFormatException(_source, _position, "Pattern is empty.");
        }

        return pattern;
    }

    private void EnqueueTokens()
    {
        _tokenQueue = new Queue<Token>();
        _position = 0;

        foreach (char character in _source)
        {
            if (CharToTokenTable.TryGetValue(character, out Token token))
            {
                _tokenQueue.Enqueue(token);
            }
            else
            {
                throw new PatternFormatException(_source, _position, $"Unknown token '{character}'.");
            }

            _position++;
        }
    }

    private FieldChainPattern? TryParsePatternChain()
    {
        if (_tokenQueue.Count == 0)
        {
            return null;
        }

        FieldTypes choices = ParseTypeOrSet();
        (bool atLeastOne, bool atMostOne) = ParseQuantifier();
        FieldChainPattern? next = TryParsePatternChain();

        return new FieldChainPattern(choices, atLeastOne, atMostOne, next);
    }

    private FieldTypes ParseTypeOrSet()
    {
        bool isChoiceSet = TryEatToken(static token => token == Token.BracketOpen) != null;
        FieldTypes choices = EatFieldType(isChoiceSet ? "Field type expected." : "Field type or [ expected.");

        if (isChoiceSet)
        {
            FieldTypes? extraChoice;

            while ((extraChoice = TryEatFieldType()) != null)
            {
                choices |= extraChoice.Value;
            }

            EatToken(static token => token == Token.BracketClose, "Field type or ] expected.");
        }

        return choices;
    }

    private (bool atLeastOne, bool atMostOne) ParseQuantifier()
    {
        Token? quantifier = TryEatToken(static token => QuantifierTokens.Contains(token));

        return quantifier switch
        {
            Token.QuestionMark => (false, true),
            Token.Plus => (true, false),
            Token.Asterisk => (false, false),
            _ => (true, true)
        };
    }

    private FieldTypes EatFieldType(string errorMessage)
    {
        FieldTypes? fieldType = TryEatFieldType();

        if (fieldType != null)
        {
            return fieldType.Value;
        }

        throw new PatternFormatException(_source, _position, errorMessage);
    }

    private FieldTypes? TryEatFieldType()
    {
        Token? token = TryEatToken(static token => TokenToFieldTypeTable.ContainsKey(token));

        if (token != null)
        {
            return TokenToFieldTypeTable[token.Value];
        }

        return null;
    }

    private void EatToken(Predicate<Token> condition, string errorMessage)
    {
        Token? token = TryEatToken(condition);

        if (token == null)
        {
            throw new PatternFormatException(_source, _position, errorMessage);
        }
    }

    private Token? TryEatToken(Predicate<Token> condition)
    {
        if (_tokenQueue.TryPeek(out Token nextToken) && condition(nextToken))
        {
            _tokenQueue.Dequeue();
            _position++;
            return nextToken;
        }

        return null;
    }

    private enum Token
    {
        QuestionMark,
        Plus,
        Asterisk,
        BracketOpen,
        BracketClose,
        ToManyRelationship,
        ToOneRelationship,
        Relationship,
        Attribute,
        Field
    }
}
