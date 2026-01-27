using System.Net;
using System.Numerics;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_always

namespace OpenApiTests.AttributeTypes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class AttributeTypesDbContext(DbContextOptions<AttributeTypesDbContext> options)
    : TestableDbContext(options)
{
    public DbSet<TypeContainer> TypeContainers => Set<TypeContainer>();

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        builder.Properties<Int128>()
            .HaveConversion<Int128Converter>();

        builder.Properties<UInt128>()
            .HaveConversion<UInt128Converter>();

        builder.Properties<Half>()
            .HaveConversion<HalfConverter>();

        builder.Properties<Complex>()
            .HaveConversion<ComplexConverter>();

        builder.Properties<Rune>()
            .HaveConversion<RuneConverter>();

        builder.Properties<IPNetwork>()
            .HaveConversion<IPNetworkConverter>();

        builder.Properties<Version>()
            .HaveConversion<VersionConverter>();
    }

    private sealed class Int128Converter()
        : ValueConverter<Int128, string>(value => TypeConverterRegistry.Int128ToString(value), value => TypeConverterRegistry.Int128FromString(value));

    private sealed class UInt128Converter()
        : ValueConverter<UInt128, string>(value => TypeConverterRegistry.UInt128ToString(value), value => TypeConverterRegistry.UInt128FromString(value));

    private sealed class HalfConverter()
        : ValueConverter<Half, float>(value => TypeConverterRegistry.HalfToFloat(value), value => TypeConverterRegistry.HalfFromFloat(value));

    private sealed class ComplexConverter()
        : ValueConverter<Complex, string>(value => TypeConverterRegistry.ComplexToString(value), value => TypeConverterRegistry.ComplexFromString(value));

    private sealed class RuneConverter()
        : ValueConverter<Rune, string>(value => TypeConverterRegistry.RuneToString(value), value => TypeConverterRegistry.RuneFromString(value));

    private sealed class IPNetworkConverter()
        : ValueConverter<IPNetwork, string>(value => TypeConverterRegistry.IPNetworkToString(value), value => TypeConverterRegistry.IPNetworkFromString(value));

    private sealed class VersionConverter()
        : ValueConverter<Version, string>(value => TypeConverterRegistry.VersionToString(value), value => TypeConverterRegistry.VersionFromString(value));
}
