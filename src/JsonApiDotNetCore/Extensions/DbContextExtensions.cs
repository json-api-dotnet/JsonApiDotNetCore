using Microsoft.EntityFrameworkCore;
using System;

namespace JsonApiDotNetCore.Extensions
{
    public static class DbContextExtensions
    {
        public static DbSet<T> GetDbSet<T>(this DbContext context) where T : class
        {
            var contextProperties = context.GetType().GetProperties();
            foreach(var property in contextProperties)
            {
                if (property.PropertyType == typeof(DbSet<T>))
                    return (DbSet<T>)property.GetValue(context);
            }

            throw new ArgumentException($"DbSet of type {typeof(T).FullName} not found on the DbContext", nameof(T));
        }
    }
}
