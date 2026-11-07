namespace JsonApiDotNetCore.QueryStrings.FieldChains;

[Flags]
internal enum FieldTypes
{
    None = 0,
    ToOneRelationship = 1,
    ToManyRelationship = 1 << 1,
    ScalarAttribute = 1 << 2,
    ScalarCollectionAttribute = 1 << 3,
    CompoundAttribute = 1 << 4,
    CompoundCollectionAttribute = 1 << 5,

    Relationship = ToOneRelationship | ToManyRelationship,
    Attribute = ScalarAttribute | ScalarCollectionAttribute | CompoundAttribute | CompoundCollectionAttribute,
    Field = Relationship | Attribute
}
