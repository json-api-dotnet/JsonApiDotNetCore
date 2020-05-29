using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Models.Annotation;

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

    public delegate IReadOnlyCollection<ResourceFieldAttribute> ResolveFieldChainCallback(string path, FieldChainRequirements requirements);
}
