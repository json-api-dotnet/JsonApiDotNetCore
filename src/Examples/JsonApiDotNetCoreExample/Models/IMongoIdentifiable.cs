using JsonApiDotNetCore.Resources;
using MongoDB.Bson;

namespace JsonApiDotNetCoreExample.Models;

/// <summary>
/// Marker interface to indicate a resource that is stored in MongoDB.
/// </summary>
public interface IMongoIdentifiable : IIdentifiable<ObjectId>;
