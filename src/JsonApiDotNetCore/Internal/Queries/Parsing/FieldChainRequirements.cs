using System;

namespace JsonApiDotNetCore.Internal.Queries.Parsing
{
    [Flags]
    public enum FieldChainRequirements
    {
        EndsInAttribute = 1,
        EndsInToOne = 2,
        EndsInToMany = 4,

        IsRelationship = EndsInToOne | EndsInToMany
    }
}
