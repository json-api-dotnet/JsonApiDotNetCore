using System;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCore.Repositories
{
    internal static class EntityFrameworkCoreSupport
    {
        public static Version Version { get; } = typeof(DbContext).Assembly.GetName().Version;
    }
}
