using System.Net;
using System.Numerics;
using System.Text;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.AttributeTypes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.AttributeTypes")]
public sealed class TypeContainer : Identifiable<long>
{
    // Integral numeric types

    [Attr]
    public bool TestBoolean { get; set; }

    [Attr]
    public bool? TestNullableBoolean { get; set; }

    [Attr]
    public byte TestByte { get; set; }

    [Attr]
    public byte? TestNullableByte { get; set; }

    [Attr]
    public sbyte TestSignedByte { get; set; }

    [Attr]
    public sbyte? TestNullableSignedByte { get; set; }

    [Attr]
    public short TestInt16 { get; set; }

    [Attr]
    public short? TestNullableInt16 { get; set; }

    [Attr]
    public ushort TestUnsignedInt16 { get; set; }

    [Attr]
    public ushort? TestNullableUnsignedInt16 { get; set; }

    [Attr]
    public int TestInt32 { get; set; }

    [Attr]
    public int? TestNullableInt32 { get; set; }

    [Attr]
    public uint TestUnsignedInt32 { get; set; }

    [Attr]
    public uint? TestNullableUnsignedInt32 { get; set; }

    [Attr]
    public long TestInt64 { get; set; }

    [Attr]
    public long? TestNullableInt64 { get; set; }

    [Attr]
    public ulong TestUnsignedInt64 { get; set; }

    [Attr]
    public ulong? TestNullableUnsignedInt64 { get; set; }

    [Attr]
    public Int128 TestInt128 { get; set; }

    [Attr]
    public Int128? TestNullableInt128 { get; set; }

    [Attr]
    public UInt128 TestUnsignedInt128 { get; set; }

    [Attr]
    public UInt128? TestNullableUnsignedInt128 { get; set; }

    [Attr]
    public BigInteger TestBigInteger { get; set; }

    [Attr]
    public BigInteger? TestNullableBigInteger { get; set; }

    // Floating-point numeric types

    [Attr]
    public Half TestHalf { get; set; }

    [Attr]
    public Half? TestNullableHalf { get; set; }

    [Attr]
    public float TestFloat { get; set; }

    [Attr]
    public float? TestNullableFloat { get; set; }

    [Attr]
    public double TestDouble { get; set; }

    [Attr]
    public double? TestNullableDouble { get; set; }

    [Attr]
    public decimal TestDecimal { get; set; }

    [Attr]
    public decimal? TestNullableDecimal { get; set; }

    // Other numeric types

    [Attr]
    public Complex TestComplex { get; set; }

    [Attr]
    public Complex? TestNullableComplex { get; set; }

    // Text types

    [Attr]
    public char TestChar { get; set; }

    [Attr]
    public char? TestNullableChar { get; set; }

    [Attr]
    public required string TestString { get; set; }

    [Attr]
    public string? TestNullableString { get; set; }

    [Attr]
    public Rune TestRune { get; set; }

    [Attr]
    public Rune? TestNullableRune { get; set; }

    // Temporal types

    [Attr]
    public DateTimeOffset TestDateTimeOffset { get; set; }

    [Attr]
    public DateTimeOffset? TestNullableDateTimeOffset { get; set; }

    [Attr]
    public DateTime TestDateTime { get; set; }

    [Attr]
    public DateTime? TestNullableDateTime { get; set; }

    [Attr]
    public DateOnly TestDateOnly { get; set; }

    [Attr]
    public DateOnly? TestNullableDateOnly { get; set; }

    [Attr]
    public TimeOnly TestTimeOnly { get; set; }

    [Attr]
    public TimeOnly? TestNullableTimeOnly { get; set; }

    [Attr]
    public TimeSpan TestTimeSpan { get; set; }

    [Attr]
    public TimeSpan? TestNullableTimeSpan { get; set; }

    // Various other types

    [Attr]
    public DayOfWeek TestEnum { get; set; }

    [Attr]
    public DayOfWeek? TestNullableEnum { get; set; }

    [Attr]
    public Guid TestGuid { get; set; }

    [Attr]
    public Guid? TestNullableGuid { get; set; }

    [Attr]
    public required Uri TestUri { get; set; }

    [Attr]
    public Uri? TestNullableUri { get; set; }

    [Attr]
    public required IPAddress TestIPAddress { get; set; }

    [Attr]
    public IPAddress? TestNullableIPAddress { get; set; }

    [Attr]
    public IPNetwork TestIPNetwork { get; set; }

    [Attr]
    public IPNetwork? TestNullableIPNetwork { get; set; }

    [Attr]
    public required Version TestVersion { get; set; }

    [Attr]
    public Version? TestNullableVersion { get; set; }
}
