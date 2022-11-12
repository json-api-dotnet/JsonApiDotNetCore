using System.Net;
using DatabasePerTenantExample.Models;
using JetBrains.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;

namespace DatabasePerTenantExample.Data;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class AppDbContext : DbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private string? _forcedTenantName;

    public DbSet<Employee> Employees => Set<Employee>();

    public AppDbContext(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public void SetTenantName(string tenantName)
    {
        _forcedTenantName = tenantName;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = GetConnectionString();
        optionsBuilder.UseNpgsql(connectionString);
    }

    private string GetConnectionString()
    {
        string? tenantName = GetTenantName();
        string? connectionString = _configuration[$"Data:{tenantName ?? "Default"}Connection"];

        if (connectionString == null)
        {
            throw GetErrorForInvalidTenant(tenantName);
        }

        string postgresPassword = Environment.GetEnvironmentVariable("PGPASSWORD") ?? "postgres";
        return connectionString.Replace("###", postgresPassword);
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
