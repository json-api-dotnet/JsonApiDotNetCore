using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Services
{
    public interface IExposedFieldExplorer
    {
        List<IResourceField> GetFields<T>(Expression<Func<T, dynamic>> selector = null) where T : IIdentifiable;
        List<AttrAttribute> GetAttributes<T>(Expression<Func<T, dynamic>> selector = null) where T : IIdentifiable;
        List<RelationshipAttribute> GetRelationships<T>(Expression<Func<T, dynamic>> selector = null) where T : IIdentifiable;
        List<IResourceField> GetFields(Type type);
        List<AttrAttribute> GetAttributes(Type type);
        List<RelationshipAttribute> GetRelationships(Type type);
    }
}
