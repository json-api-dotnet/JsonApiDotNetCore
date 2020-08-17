namespace JsonApiDotNetCore.Internal.Queries.Parsing
{
    public sealed class Token
    {
        public TokenKind Kind { get; }
        public string Value { get; }

        public Token(TokenKind kind, string value = null)
        {
            Kind = kind;
            Value = value;
        }

        public override string ToString()
        {
            return Value == null ? Kind.ToString() : $"{Kind}: {Value}";
        }
    }
}
