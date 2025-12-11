using System.Globalization;
using System.Net;
using System.Numerics;
using System.Text;
using TestBuildingBlocks;

namespace OpenApiTests.AttributeTypes;

public sealed class TypeConverterRegistry
{
    internal static Func<Int128, string> Int128ToString { get; } = value => value.ToString(CultureInfo.InvariantCulture);
    internal static Func<string, Int128> Int128FromString { get; } = value => Int128.Parse(value, CultureInfo.InvariantCulture);

    internal static Func<UInt128, string> UInt128ToString { get; } = value => value.ToString(CultureInfo.InvariantCulture);
    internal static Func<string, UInt128> UInt128FromString { get; } = value => UInt128.Parse(value, CultureInfo.InvariantCulture);

    internal static Func<BigInteger, string> BigIntegerToString { get; } = value => value.ToString(CultureInfo.InvariantCulture);
    internal static Func<string, BigInteger> BigIntegerFromString { get; } = value => BigInteger.Parse(value, CultureInfo.InvariantCulture);

    internal static Func<Half, float> HalfToFloat { get; } = value => value.AsFloat();
    internal static Func<float, Half> HalfFromFloat { get; } = Half.CreateChecked;
    internal static Func<string, Half> HalfFromString { get; } = value => Half.Parse(value, CultureInfo.InvariantCulture);

    internal static Func<Complex, string> ComplexToString { get; } = value => value.ToString(CultureInfo.InvariantCulture);
    internal static Func<string, Complex> ComplexFromString { get; } = value => Complex.Parse(value, CultureInfo.InvariantCulture);

    internal static Func<Rune, string> RuneToString { get; } = value => value.ToString();
    internal static Func<string, Rune> RuneFromString { get; } = value => Rune.GetRuneAt(value, 0);

    internal static Func<IPAddress, string> IPAddressToString { get; } = value => value.ToString();
    internal static Func<string, IPAddress> IPAddressFromString { get; } = IPAddress.Parse;

    internal static Func<IPNetwork, string> IPNetworkToString { get; } = ipNetwork => ipNetwork.ToString();
    internal static Func<string, IPNetwork> IPNetworkFromString { get; } = IPNetwork.Parse;

    internal static Func<Version, string> VersionToString { get; } = value => value.ToString();
    internal static Func<string, Version> VersionFromString { get; } = Version.Parse;

    public static TypeConverterRegistry Instance { get; } = new();

    private TypeConverterRegistry()
    {
    }

    public Func<object, string>? FindToStringConverter(Type type)
    {
        if (type == typeof(Int128) || type == typeof(Int128?))
        {
            return typedValue => Int128ToString((Int128)typedValue);
        }

        if (type == typeof(UInt128) || type == typeof(UInt128?))
        {
            return typedValue => UInt128ToString((UInt128)typedValue);
        }

        if (type == typeof(BigInteger) || type == typeof(BigInteger?))
        {
            return typedValue => BigIntegerToString((BigInteger)typedValue);
        }

        if (type == typeof(Complex) || type == typeof(Complex?))
        {
            return typedValue => ComplexToString((Complex)typedValue);
        }

        if (type == typeof(Rune) || type == typeof(Rune?))
        {
            return typedValue => RuneToString((Rune)typedValue);
        }

        if (type == typeof(IPAddress))
        {
            return typedValue => IPAddressToString((IPAddress)typedValue);
        }

        if (type == typeof(IPNetwork) || type == typeof(IPNetwork?))
        {
            return typedValue => IPNetworkToString((IPNetwork)typedValue);
        }

        if (type == typeof(Version))
        {
            return typedValue => VersionToString((Version)typedValue);
        }

        return null;
    }
}
