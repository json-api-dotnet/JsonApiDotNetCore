namespace JsonApiDotNetCore.QueryStrings.FieldChains;

[Flags]
internal enum FieldTypes
{
    None = 0,
    ScalarAttribute = 1,
    ToOneRelationship = 1 << 1,
    ToManyRelationship = 1 << 2,
    CompoundAttribute = 1 << 3,
    PrimitiveCollection = 1 << 4,
    CompoundCollection = 1 << 5,

    Relationship = ToOneRelationship | ToManyRelationship,
    Field = ScalarAttribute | Relationship | CompoundAttribute | PrimitiveCollection | CompoundCollection
}
