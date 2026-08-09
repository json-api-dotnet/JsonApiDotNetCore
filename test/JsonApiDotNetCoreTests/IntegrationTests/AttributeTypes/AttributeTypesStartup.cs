using System.Net;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Parsing;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.AttributeTypes;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class AttributeTypesStartup : TestableStartup<AttributeTypesDbContext>
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IFilterParser, TypeAwareFilterParser>();

        base.ConfigureServices(services);
    }

    protected override void ConfigureJsonApiOptions(JsonApiOptions options)
    {
        base.ConfigureJsonApiOptions(options);

        options.SerializerOptions.Converters.Add(new Int128JsonConverter());
        options.SerializerOptions.Converters.Add(new UInt128JsonConverter());
        options.SerializerOptions.Converters.Add(new BigIntegerJsonConverter());
        options.SerializerOptions.Converters.Add(new ComplexJsonConverter());
        options.SerializerOptions.Converters.Add(new RuneJsonConverter());
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter<DayOfWeek>());
        options.SerializerOptions.Converters.Add(new IPAddressJsonConverter());
        options.SerializerOptions.Converters.Add(new IPNetworkJsonConverter());
    }

    private sealed class Int128JsonConverter()
        : ValueTypeJsonConverter<Int128>(TypeConverterRegistry.Int128FromString, TypeConverterRegistry.Int128ToString);

    private sealed class UInt128JsonConverter()
        : ValueTypeJsonConverter<UInt128>(TypeConverterRegistry.UInt128FromString, TypeConverterRegistry.UInt128ToString);

    private sealed class BigIntegerJsonConverter()
        : ValueTypeJsonConverter<BigInteger>(TypeConverterRegistry.BigIntegerFromString, TypeConverterRegistry.BigIntegerToString);

    private sealed class ComplexJsonConverter()
        : ValueTypeJsonConverter<Complex>(TypeConverterRegistry.ComplexFromString, TypeConverterRegistry.ComplexToString);

    private sealed class RuneJsonConverter()
        : ValueTypeJsonConverter<Rune>(TypeConverterRegistry.RuneFromString, TypeConverterRegistry.RuneToString);

    private sealed class IPAddressJsonConverter()
        : ReferenceTypeJsonConverter<IPAddress>(TypeConverterRegistry.IPAddressFromString, TypeConverterRegistry.IPAddressToString);

    private sealed class IPNetworkJsonConverter()
        : ValueTypeJsonConverter<IPNetwork>(TypeConverterRegistry.IPNetworkFromString, TypeConverterRegistry.IPNetworkToString);
}
