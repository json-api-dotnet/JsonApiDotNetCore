using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreTests.IntegrationTests.OptimisticConcurrency;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public abstract class PostgresVersionedIdentifiable<TId> : Identifiable<TId>, IVersionedIdentifiable<TId, uint>
{
    [NotMapped]
    public string? Version
    {
        get => ConcurrencyToken == default ? null : ConcurrencyToken.ToString();
        set => ConcurrencyToken = value == null ? default : uint.Parse(value);
    }

    // https://www.npgsql.org/efcore/modeling/concurrency.html
    [Column("xmin", TypeName = "xid")]
    [ConcurrencyCheck]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public uint ConcurrencyToken { get; set; }

    public Guid ConcurrencyValue { get; set; }
}
