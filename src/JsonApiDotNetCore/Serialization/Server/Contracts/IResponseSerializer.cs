using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Serialization.Server
{
    internal interface IResponseSerializer
    {
        /// <summary>
        /// Sets the designated request relationship in the case of requests of
        /// the form a /articles/1/relationships/author.
        /// </summary>
        RelationshipAttribute RequestRelationship { get; set; }
    }
}