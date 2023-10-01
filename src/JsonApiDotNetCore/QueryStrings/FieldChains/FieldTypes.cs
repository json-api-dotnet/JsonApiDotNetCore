namespace JsonApiDotNetCore.QueryStrings.FieldChains;

[Flags]
internal enum FieldTypes
{
    None = 0,
    Attribute = 1,
    ToOneRelationship = 1 << 1,
    ToManyRelationship = 1 << 2,
    Relationship = ToOneRelationship | ToManyRelationship,
    Field = Attribute | Relationship
}
