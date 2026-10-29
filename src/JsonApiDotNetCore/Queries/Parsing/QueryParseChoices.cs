namespace JsonApiDotNetCore.Queries.Parsing;

/// <summary>
/// Lists the grammatical choices that can be expected at a token position in a query string expression.
/// </summary>
[Flags]
public enum QueryParseChoices
{
    None = 0,
    Minus = 1 << 0,
    OpenParenthesis = 1 << 1,
    FilterFunction = 1 << 2,
    CountFunction = 1 << 3,
    Function = 1 << 4,
    Number = 1 << 5,
    ParameterName = 1 << 6,
    ResourceType = 1 << 7,
    FieldName = 1 << 8,
    ToOneRelationshipName = 1 << 9,
    ToManyRelationshipName = 1 << 10,
    RelationshipName = ToOneRelationshipName | ToManyRelationshipName,
    QuotedText = 1 << 11,
    Underscore = 1 << 12,
    Null = 1 << 13,
    Comma = 1 << 14,
    EndOfExpression = 1 << 15
}
