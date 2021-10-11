using System;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal.Parsing
{
    /// <summary>
    /// Used internally when parsing subexpressions in the query string parsers to indicate requirements when resolving a chain of fields. Note these may be
    /// interpreted differently or even discarded completely by the various parser implementations, as they tend to better understand the characteristics of
    /// the entire expression being parsed.
    /// </summary>
    [Flags]
    public enum FieldChainRequirements
    {
        /// <summary>
        /// Indicates a single <see cref="AttrAttribute" />, optionally preceded by a chain of <see cref="RelationshipAttribute" />s.
        /// </summary>
        EndsInAttribute = 1,

        /// <summary>
        /// Indicates a single <see cref="HasOneAttribute" />, optionally preceded by a chain of <see cref="RelationshipAttribute" />s.
        /// </summary>
        EndsInToOne = 2,

        /// <summary>
        /// Indicates a single <see cref="HasManyAttribute" />, optionally preceded by a chain of <see cref="RelationshipAttribute" />s.
        /// </summary>
        EndsInToMany = 4,

        /// <summary>
        /// Indicates one or a chain of <see cref="RelationshipAttribute" />s.
        /// </summary>
        IsRelationship = EndsInToOne | EndsInToMany
    }
}
