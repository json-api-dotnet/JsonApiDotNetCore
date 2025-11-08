using System.Buffers.Binary;
using System.Globalization;
using System.Net;
using System.Numerics;
using System.Text;
using Bogus;
using JetBrains.Annotations;
using TestBuildingBlocks;

// @formatter:wrap_chained_method_calls chop_if_long
// @formatter:wrap_before_first_method_call true

namespace OpenApiTests.AttributeTypes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class AttributeTypesFakers
{
    private readonly Lazy<Faker<TypeContainer>> _lazyTypeContainerFaker = new(() => new Faker<TypeContainer>()
        .MakeDeterministic()
        .RuleFor(container => container.TestBoolean, faker => faker.Random.Bool())
        .RuleFor(container => container.TestNullableBoolean, faker => faker.Random.Bool())
        .RuleFor(container => container.TestByte, faker => faker.Random.Byte())
        .RuleFor(container => container.TestNullableByte, faker => faker.Random.Byte())
        .RuleFor(container => container.TestSignedByte, faker => faker.Random.SByte())
        .RuleFor(container => container.TestNullableSignedByte, faker => faker.Random.SByte())
        .RuleFor(container => container.TestInt16, faker => faker.Random.Short())
        .RuleFor(container => container.TestNullableInt16, faker => faker.Random.Short())
        .RuleFor(container => container.TestUnsignedInt16, faker => faker.Random.UShort())
        .RuleFor(container => container.TestNullableUnsignedInt16, faker => faker.Random.UShort())
        .RuleFor(container => container.TestInt32, faker => faker.Random.Int())
        .RuleFor(container => container.TestNullableInt32, faker => faker.Random.Int())
        .RuleFor(container => container.TestUnsignedInt32, faker => faker.Random.UInt(0, int.MaxValue))
        .RuleFor(container => container.TestNullableUnsignedInt32, faker => faker.Random.UInt(0, int.MaxValue))
        .RuleFor(container => container.TestInt64, faker => faker.Random.Long())
        .RuleFor(container => container.TestNullableInt64, faker => faker.Random.Long())
        .RuleFor(container => container.TestUnsignedInt64, faker => faker.Random.ULong(0, long.MaxValue))
        .RuleFor(container => container.TestNullableUnsignedInt64, faker => faker.Random.ULong(0, long.MaxValue))
        .RuleFor(container => container.TestInt128, faker => faker.Random.ULong())
        .RuleFor(container => container.TestNullableInt128, faker => faker.Random.ULong())
        .RuleFor(container => container.TestUnsignedInt128, faker => faker.Random.ULong())
        .RuleFor(container => container.TestNullableUnsignedInt128, faker => faker.Random.ULong())
        .RuleFor(container => container.TestBigInteger, faker => faker.Random.ULong())
        .RuleFor(container => container.TestNullableBigInteger, faker => faker.Random.ULong())
        .RuleFor(container => container.TestHalf, faker => (Half)faker.Random.Float())
        .RuleFor(container => container.TestNullableHalf, faker => (Half)faker.Random.Float())
        .RuleFor(container => container.TestFloat, faker => faker.Random.Float())
        .RuleFor(container => container.TestNullableFloat, faker => faker.Random.Float())
        .RuleFor(container => container.TestDouble, faker => faker.Random.Double())
        .RuleFor(container => container.TestNullableDouble, faker => faker.Random.Double())
        .RuleFor(container => container.TestDecimal, faker => faker.Random.Decimal())
        .RuleFor(container => container.TestNullableDecimal, faker => faker.Random.Decimal())
        .RuleFor(container => container.TestComplex, GetRandomComplex)
        .RuleFor(container => container.TestNullableComplex, faker => GetRandomComplex(faker))
        .RuleFor(container => container.TestChar, faker => faker.Lorem.Letter()[0])
        .RuleFor(container => container.TestNullableChar, faker => faker.Lorem.Letter()[0])
        .RuleFor(container => container.TestString, faker => faker.Random.String2(faker.Random.Int(5, 50)))
        .RuleFor(container => container.TestNullableString, faker => faker.Random.String2(faker.Random.Int(5, 50)))
        .RuleFor(container => container.TestRune, faker => new Rune(faker.Random.Utf16String(1, 1)[0]))
        .RuleFor(container => container.TestNullableRune, faker => new Rune(faker.Random.Utf16String(1, 1)[0]))
        .RuleFor(container => container.TestDateTimeOffset, faker => faker.Date.PastOffset().ToUniversalTime().TruncateToWholeMilliseconds())
        .RuleFor(container => container.TestNullableDateTimeOffset, faker => faker.Date.PastOffset().ToUniversalTime().TruncateToWholeMilliseconds())
        .RuleFor(container => container.TestDateTime, faker => faker.Date.Past().ToUniversalTime().TruncateToWholeMilliseconds())
        .RuleFor(container => container.TestNullableDateTime, faker => faker.Date.Past().ToUniversalTime().TruncateToWholeMilliseconds())
        .RuleFor(container => container.TestDateOnly, faker => faker.Date.PastDateOnly())
        .RuleFor(container => container.TestNullableDateOnly, faker => faker.Date.PastDateOnly())
        .RuleFor(container => container.TestTimeOnly, faker => faker.Date.RecentTimeOnly().TruncateToWholeMilliseconds())
        .RuleFor(container => container.TestNullableTimeOnly, faker => faker.Date.RecentTimeOnly().TruncateToWholeMilliseconds())
        .RuleFor(container => container.TestTimeSpan, faker => faker.Date.Timespan().TruncateToWholeMilliseconds())
        .RuleFor(container => container.TestNullableTimeSpan, faker => faker.Date.Timespan().TruncateToWholeMilliseconds())
        .RuleFor(container => container.TestEnum, faker => faker.PickRandom<DayOfWeek>())
        .RuleFor(container => container.TestNullableEnum, faker => faker.PickRandom<DayOfWeek>())
        .RuleFor(container => container.TestGuid, faker => faker.Random.Guid())
        .RuleFor(container => container.TestNullableGuid, faker => faker.Random.Guid())
        .RuleFor(container => container.TestUri, faker => new Uri(faker.Internet.UrlWithPath()))
        .RuleFor(container => container.TestNullableUri, faker => new Uri(faker.Internet.UrlWithPath()))
        .RuleFor(container => container.TestIPAddress, faker => faker.Internet.IpAddress())
        .RuleFor(container => container.TestNullableIPAddress, faker => faker.Internet.IpAddress())
        .RuleFor(container => container.TestIPNetwork, faker => CreateNetworkMask(faker.Internet.IpAddress(), faker.Random.Int(0, 32)))
        .RuleFor(container => container.TestNullableIPNetwork, faker => CreateNetworkMask(faker.Internet.IpAddress(), faker.Random.Int(0, 32)))
        .RuleFor(container => container.TestVersion, faker => faker.System.Version())
        .RuleFor(container => container.TestNullableVersion, faker => faker.System.Version()));

    public Faker<TypeContainer> TypeContainer => _lazyTypeContainerFaker.Value;

    private static Complex GetRandomComplex(Faker faker)
    {
        string realValue = faker.Random.Double().ToString(CultureInfo.InvariantCulture);
        string imaginaryValue = faker.Random.Double().ToString(CultureInfo.InvariantCulture);
        return Complex.Parse($"<{realValue}; {imaginaryValue}>", CultureInfo.InvariantCulture);
    }

    private static IPNetwork CreateNetworkMask(IPAddress ipAddress, int prefixLength)
    {
        byte[] ipAddressBytes = ipAddress.GetAddressBytes();
        uint ipAddressValue = BitConverter.ToUInt32(ipAddressBytes);

        uint networkMask = (uint)((long)uint.MaxValue << (32 - prefixLength));

        if (BitConverter.IsLittleEndian)
        {
            networkMask = BinaryPrimitives.ReverseEndianness(networkMask);
        }

        uint networkAddressValue = ipAddressValue & networkMask;
        return new IPNetwork(new IPAddress(networkAddressValue), prefixLength);
    }
}
