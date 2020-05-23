using JsonApiDotNetCore.Models.Fluent;
using System;
using System.Linq.Expressions;

namespace JsonApiDotNetCore.Reflection
{
    public static class ReflectionExtensions
    {
        public static Member ToMember<TMapping, TReturn>(this Expression<Func<TMapping, TReturn>> propertyExpression)
        {
            return ReflectionHelper.GetMember(propertyExpression);
        }

        public static bool TryGetResouceMapping(this Type entityType, out IResourceMapping resourceMapping)
        {
            return ReflectionHelper.TryGetResouceMapping(entityType, out resourceMapping);
        }
    }
}
