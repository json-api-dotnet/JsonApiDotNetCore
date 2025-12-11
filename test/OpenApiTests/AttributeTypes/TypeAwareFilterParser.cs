using System.Net;
using System.Numerics;
using System.Text;
using JsonApiDotNetCore.Queries.Parsing;
using JsonApiDotNetCore.Resources;

namespace OpenApiTests.AttributeTypes;

public sealed class TypeAwareFilterParser(IResourceFactory resourceFactory)
    : FilterParser(resourceFactory)
{
    protected override ConstantValueConverter GetConstantValueConverterForType(Type destinationType)
    {
        if (destinationType == typeof(Int128) || destinationType == typeof(Int128?))
        {
            return CreateConverter(destinationType, TypeConverterRegistry.Int128FromString);
        }

        if (destinationType == typeof(UInt128) || destinationType == typeof(UInt128?))
        {
            return CreateConverter(destinationType, TypeConverterRegistry.UInt128FromString);
        }

        if (destinationType == typeof(BigInteger) || destinationType == typeof(BigInteger?))
        {
            return CreateConverter(destinationType, TypeConverterRegistry.BigIntegerFromString);
        }

        if (destinationType == typeof(Half) || destinationType == typeof(Half?))
        {
            return CreateConverter(destinationType, TypeConverterRegistry.HalfFromString);
        }

        if (destinationType == typeof(Complex) || destinationType == typeof(Complex?))
        {
            return CreateConverter(destinationType, TypeConverterRegistry.ComplexFromString);
        }

        if (destinationType == typeof(Rune) || destinationType == typeof(Rune?))
        {
            return CreateConverter(destinationType, TypeConverterRegistry.RuneFromString);
        }

        if (destinationType == typeof(IPAddress))
        {
            return CreateConverter(destinationType, TypeConverterRegistry.IPAddressFromString);
        }

        if (destinationType == typeof(IPNetwork) || destinationType == typeof(IPNetwork?))
        {
            return CreateConverter(destinationType, TypeConverterRegistry.IPNetworkFromString);
        }

        if (destinationType == typeof(Version))
        {
            return CreateConverter(destinationType, TypeConverterRegistry.VersionFromString);
        }

        return base.GetConstantValueConverterForType(destinationType);
    }

    private static ConstantValueConverter CreateConverter<T>(Type destinationType, Func<string, T> converter)
        where T : notnull
    {
        return (stringValue, position) =>
        {
            try
            {
                return converter(stringValue);
            }
            catch (Exception exception) when (exception is FormatException or OverflowException or InvalidCastException or ArgumentException)
            {
                string destinationTypeName = RuntimeTypeConverter.GetFriendlyTypeName(destinationType);
                throw new QueryParseException($"Failed to convert '{stringValue}' of type 'String' to type '{destinationTypeName}'.", position, exception);
            }
        };
    }
}
