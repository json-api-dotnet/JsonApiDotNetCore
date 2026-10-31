using System.Net;
using System.Numerics;
using System.Text;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.AttributeTypes;

/// <summary>
/// One property per converter family, stored in a JSON column. Collection properties exist only for element types that EF Core can map inside a JSON
/// column, see <see cref="NestedAttributeTypeTests" /> for the types that are not supported.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class TypeContainerNested
{
    [Attr]
    public Int128 TestInt128 { get; set; }

    [Attr]
    public UInt128 TestUnsignedInt128 { get; set; }

    [Attr]
    public BigInteger TestBigInteger { get; set; }

    [Attr]
    public Half TestHalf { get; set; }

    [Attr]
    public Rune TestRune { get; set; }

    [Attr]
    public IPAddress? TestIPAddress { get; set; }

    [Attr]
    public IPNetwork TestIPNetwork { get; set; }

    [Attr]
    public Version? TestVersion { get; set; }

    [Attr]
    public Uri? TestUri { get; set; }

    [Attr]
    public TimeSpan TestTimeSpan { get; set; }

    [Attr]
    public DateOnly TestDateOnly { get; set; }

    [Attr]
    public TimeOnly TestTimeOnly { get; set; }

    [Attr]
    public DateTimeOffset TestDateTimeOffset { get; set; }

    [Attr]
    public Guid TestGuid { get; set; }

    [Attr]
    public decimal TestDecimal { get; set; }

    [Attr]
    public DayOfWeek TestEnum { get; set; }

    [Attr]
    public List<int?> TestInts { get; set; } = [];

    [Attr]
    public List<BigInteger?> TestBigIntegers { get; set; } = [];

    [Attr]
    public List<IPAddress?> TestIPAddresses { get; set; } = [];

    [Attr]
    public List<IPNetwork?> TestIPNetworks { get; set; } = [];

    [Attr]
    public List<Uri?> TestUris { get; set; } = [];

    [Attr]
    public List<TimeSpan?> TestTimeSpans { get; set; } = [];

    [Attr]
    public List<DateOnly?> TestDateOnlys { get; set; } = [];

    [Attr]
    public List<TimeOnly?> TestTimeOnlys { get; set; } = [];

    [Attr]
    public List<DateTimeOffset?> TestDateTimeOffsets { get; set; } = [];

    [Attr]
    public List<Guid?> TestGuids { get; set; } = [];

    [Attr]
    public List<decimal?> TestDecimals { get; set; } = [];

    [Attr]
    public List<DayOfWeek?> TestEnums { get; set; } = [];
}
