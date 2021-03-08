using System;
using Microsoft.EntityFrameworkCore;

#pragma warning disable AV1008 // Class should not be static

namespace JsonApiDotNetCore.Repositories
{
    internal static class EntityFrameworkCoreSupport
    {
        public static Version Version { get; } = typeof(DbContext).Assembly.GetName().Version;
    }
}
