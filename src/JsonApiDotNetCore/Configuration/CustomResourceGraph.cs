using System.Linq.Expressions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Configuration;

[PublicAPI]
public sealed class CustomResourceGraph : IResourceGraph
{
    public IReadOnlySet<ResourceType> GetResourceTypes()
    {
        throw new NotImplementedException();
    }

    public ResourceType GetResourceType(string publicName)
    {
        throw new NotImplementedException();
    }

    public ResourceType GetResourceType(Type resourceClrType)
    {
        throw new NotImplementedException();
    }

    public ResourceType GetResourceType<TResource>()
        where TResource : class, IIdentifiable
    {
        throw new NotImplementedException();
    }

    public ResourceType FindResourceType(string publicName)
    {
        throw new NotImplementedException();
    }

    public ResourceType FindResourceType(Type resourceClrType)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyCollection<ResourceFieldAttribute> GetFields<TResource>(Expression<Func<TResource, object?>> selector)
        where TResource : class, IIdentifiable
    {
        throw new NotImplementedException();
    }

    public IReadOnlyCollection<AttrAttribute> GetAttributes<TResource>(Expression<Func<TResource, object?>> selector)
        where TResource : class, IIdentifiable
    {
        throw new NotImplementedException();
    }

    public IReadOnlyCollection<RelationshipAttribute> GetRelationships<TResource>(Expression<Func<TResource, object?>> selector)
        where TResource : class, IIdentifiable
    {
        throw new NotImplementedException();
    }
}
