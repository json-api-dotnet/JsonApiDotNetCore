using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings.CustomFunctions.Decrypt;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class DecryptDbContext(DbContextOptions<DecryptDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<Blog> Blogs => Set<Blog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        QueryStringDbContext.ConfigureModel(builder);

        builder.HasDbFunction(DatabaseFunctionStub.DecryptMethod)
            .HasName("decrypt_column_value");

        base.OnModelCreating(builder);
    }

    internal async Task DeclareDecryptFunctionAsync()
    {
        // Just for demo purposes, decryption is defined as: base64-decode the incoming value.
        await Database.ExecuteSqlRawAsync("""
            CREATE OR REPLACE FUNCTION decrypt_column_value(value text)
              RETURNS text
            RETURN encode(decode(value, 'base64'), 'escape');
            """);
    }
}
