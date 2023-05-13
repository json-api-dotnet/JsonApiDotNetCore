using System.Globalization;
using JetBrains.Annotations;

#pragma warning disable AV1008 // Class should not be static

namespace JsonApiDotNetCore.Resources.Internal;

[PublicAPI]
public static class RuntimeTypeConverter
{
    private const string ParseQueryStringsUsingCurrentCultureSwitchName = "JsonApiDotNetCore.ParseQueryStringsUsingCurrentCulture";

    public static object? ConvertType(object? value, Type type)
    {
        ArgumentGuard.NotNull(type);

        // Earlier versions of JsonApiDotNetCore failed to pass CultureInfo.InvariantCulture in the parsing below, which resulted in the 'current'
        // culture being used. Unlike parsing JSON request/response bodies, this effectively meant that query strings were parsed based on the
        // OS-level regional settings of the web server.
        // Because this was fixed in a non-major release, the switch below enables to revert to the old behavior.

        // With the switch activated, API developers can still choose between:
        // - Requiring localized date/number formats: parsing occurs using the OS-level regional settings (the default).
        // - Requiring culture-invariant date/number formats: requires setting CultureInfo.DefaultThreadCurrentCulture to CultureInfo.InvariantCulture at startup.
        // - Allowing clients to choose by sending an Accept-Language HTTP header: requires app.UseRequestLocalization() at startup.

        CultureInfo? cultureInfo = AppContext.TryGetSwitch(ParseQueryStringsUsingCurrentCultureSwitchName, out bool useCurrentCulture) && useCurrentCulture
            ? null
            : CultureInfo.InvariantCulture;

        if (value == null)
        {
            if (!CanContainNull(type))
            {
                string targetTypeName = type.GetFriendlyTypeName();
                throw new FormatException($"Failed to convert 'null' to type '{targetTypeName}'.");
            }

            return null;
        }

        Type runtimeType = value.GetType();

        if (type == runtimeType || type.IsAssignableFrom(runtimeType))
        {
            return value;
        }

        string? stringValue = value is IFormattable cultureAwareValue ? cultureAwareValue.ToString(null, cultureInfo) : value.ToString();

        if (string.IsNullOrEmpty(stringValue))
        {
            return GetDefaultValue(type);
        }

        bool isNullableTypeRequested = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        Type nonNullableType = Nullable.GetUnderlyingType(type) ?? type;

        try
        {
            if (nonNullableType == typeof(Guid))
            {
                Guid convertedValue = Guid.Parse(stringValue);
                return isNullableTypeRequested ? (Guid?)convertedValue : convertedValue;
            }

            if (nonNullableType == typeof(DateTime))
            {
                DateTime convertedValue = DateTime.Parse(stringValue, cultureInfo, DateTimeStyles.RoundtripKind);
                return isNullableTypeRequested ? (DateTime?)convertedValue : convertedValue;
            }

            if (nonNullableType == typeof(DateTimeOffset))
            {
                DateTimeOffset convertedValue = DateTimeOffset.Parse(stringValue, cultureInfo, DateTimeStyles.RoundtripKind);
                return isNullableTypeRequested ? (DateTimeOffset?)convertedValue : convertedValue;
            }

            if (nonNullableType == typeof(TimeSpan))
            {
                TimeSpan convertedValue = TimeSpan.Parse(stringValue, cultureInfo);
                return isNullableTypeRequested ? (TimeSpan?)convertedValue : convertedValue;
            }

            if (nonNullableType == typeof(DateOnly))
            {
                DateOnly convertedValue = DateOnly.Parse(stringValue, cultureInfo);
                return isNullableTypeRequested ? (DateOnly?)convertedValue : convertedValue;
            }

            if (nonNullableType == typeof(TimeOnly))
            {
                TimeOnly convertedValue = TimeOnly.Parse(stringValue, cultureInfo);
                return isNullableTypeRequested ? (TimeOnly?)convertedValue : convertedValue;
            }

            if (nonNullableType.IsEnum)
            {
                object convertedValue = Enum.Parse(nonNullableType, stringValue);

                // https://bradwilson.typepad.com/blog/2008/07/creating-nullab.html
                return convertedValue;
            }

            // https://bradwilson.typepad.com/blog/2008/07/creating-nullab.html
            return Convert.ChangeType(stringValue, nonNullableType, cultureInfo);
        }
        catch (Exception exception) when (exception is FormatException or OverflowException or InvalidCastException or ArgumentException)
        {
            string runtimeTypeName = runtimeType.GetFriendlyTypeName();
            string targetTypeName = type.GetFriendlyTypeName();

            throw new FormatException($"Failed to convert '{value}' of type '{runtimeTypeName}' to type '{targetTypeName}'.", exception);
        }
    }

    public static bool CanContainNull(Type type)
    {
        return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
    }

    public static object? GetDefaultValue(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }
}
