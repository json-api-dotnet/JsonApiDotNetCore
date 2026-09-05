using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JsonApiDotNetCoreExample.Models;

/// <summary>
/// Basic implementation of a JSON:API resource whose Id is stored as a 12-byte hexadecimal ObjectId in MongoDB.
/// </summary>
public abstract class MongoIdentifiable : IMongoIdentifiable
{
    /// <inheritdoc />
    [BsonId]
    public virtual ObjectId Id { get; set; }

    /// <inheritdoc />
    [BsonIgnore]
    public string? StringId
    {
        get => Id.ToString();
        set => Id = ObjectId.Parse(value);
    }

    /// <inheritdoc />
    [BsonIgnore]
    public string? LocalId { get; set; }
}
