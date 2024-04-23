using System.Net;
using GettingStarted.Data;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

namespace GettingStarted;

public sealed class QueryStringDbContextResolver(IHttpContextAccessor httpContextAccessor, SqliteSampleDbContext startupDbContext) : IDbContextResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly SqliteSampleDbContext _startupDbContext = startupDbContext;

    public DbContext GetContext()
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            // DbContext is used to scan the model at app startup. Pick any, since their entities are identical.
            return _startupDbContext;
        }

        StringValues dbType = httpContext.Request.Query["dbType"];

        if (!Enum.TryParse(dbType, true, out DatabaseType databaseType))
        {
            throw new JsonApiException(new ErrorObject(HttpStatusCode.BadRequest)
            {
                Title = "The 'dbType' query string parameter is missing or invalid."
            });
        }

        return databaseType switch
        {
            DatabaseType.Sqlite => httpContext.RequestServices.GetRequiredService<SqliteSampleDbContext>(),
            DatabaseType.PostgreSql => httpContext.RequestServices.GetRequiredService<PostgreSqlSampleDbContext>(),
            _ => throw new NotSupportedException("Unknown database type.")
        };
    }
}
