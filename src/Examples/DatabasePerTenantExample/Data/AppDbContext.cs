using System.Net;
using DatabasePerTenantExample.Models;
using JetBrains.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;

namespace DatabasePerTenantExample.Data;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    : DbContext(options)
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IConfiguration _configuration = configuration;
    private string? _forcedTenantName;

    public DbSet<Employee> Employees => Set<Employee>();

    public void SetTenantName(string tenantName)
    {
        _forcedTenantName = tenantName;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        string connectionString = GetConnectionString();
        builder.UseNpgsql(connectionString);
    }

    private string GetConnectionString()
    {
        string? tenantName = GetTenantName();
        string? connectionString = _configuration.GetConnectionString(tenantName ?? "Default");

        if (connectionString == null)
        {
            throw GetErrorForInvalidTenant(tenantName);
        }

        return connectionString;
    }

    private string? GetTenantName()
    {
        if (_forcedTenantName != null)
        {
            return _forcedTenantName;
        }

        if (_httpContextAccessor.HttpContext != null)
        {
            string? tenantName = (string?)_httpContextAccessor.HttpContext.Request.RouteValues["tenantName"];

            if (tenantName == null)
            {
                throw GetErrorForInvalidTenant(null);
            }

            return tenantName;
        }

        return null;
    }

    private static JsonApiException GetErrorForInvalidTenant(string? tenantName)
    {
        return new JsonApiException(new ErrorObject(HttpStatusCode.BadRequest)
        {
            Title = "Missing or invalid tenant in URL.",
            Detail = $"Tenant '{tenantName}' does not exist."
        });
    }
}
