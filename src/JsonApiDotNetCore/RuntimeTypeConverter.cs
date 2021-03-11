using System;

#pragma warning disable AV1008 // Class should not be static

namespace JsonApiDotNetCore
{
    internal static class RuntimeTypeConverter
    {
        public static object ConvertType(object value, Type type)
        {
            ArgumentGuard.NotNull(type, nameof(type));

            if (value == null)
            {
                if (!CanContainNull(type))
                {
                    throw new FormatException($"Failed to convert 'null' to type '{type.Name}'.");
                }

                return null;
            }

            Type runtimeType = value.GetType();

            if (type == runtimeType || type.IsAssignableFrom(runtimeType))
            {
                return value;
            }

            string stringValue = value.ToString();

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

                if (nonNullableType == typeof(DateTimeOffset))
                {
                    DateTimeOffset convertedValue = DateTimeOffset.Parse(stringValue);
                    return isNullableTypeRequested ? (DateTimeOffset?)convertedValue : convertedValue;
                }

                if (nonNullableType == typeof(TimeSpan))
                {
                    TimeSpan convertedValue = TimeSpan.Parse(stringValue);
                    return isNullableTypeRequested ? (TimeSpan?)convertedValue : convertedValue;
                }

                if (nonNullableType.IsEnum)
                {
                    object convertedValue = Enum.Parse(nonNullableType, stringValue);

                    // https://bradwilson.typepad.com/blog/2008/07/creating-nullab.html
                    return convertedValue;
                }

                // https://bradwilson.typepad.com/blog/2008/07/creating-nullab.html
                return Convert.ChangeType(stringValue, nonNullableType);
            }
            catch (Exception exception) when (exception is FormatException || exception is OverflowException || exception is InvalidCastException ||
                exception is ArgumentException)
            {
                throw new FormatException($"Failed to convert '{value}' of type '{runtimeType.Name}' to type '{type.Name}'.", exception);
            }
        }

        public static bool CanContainNull(Type type)
        {
            return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }

        public static object GetDefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}
