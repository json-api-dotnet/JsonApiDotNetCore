using FluentAssertions;
using JsonApiDotNetCore.Resources;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.TypeConversion;

public sealed class RuntimeTypeConverterTests
{
    [Theory]
    [InlineData(typeof(bool))]
    [InlineData(typeof(byte))]
    [InlineData(typeof(sbyte))]
    [InlineData(typeof(char))]
    [InlineData(typeof(short))]
    [InlineData(typeof(ushort))]
    [InlineData(typeof(int))]
    [InlineData(typeof(uint))]
    [InlineData(typeof(long))]
    [InlineData(typeof(ulong))]
    [InlineData(typeof(float))]
    [InlineData(typeof(double))]
    [InlineData(typeof(decimal))]
    [InlineData(typeof(Guid))]
    [InlineData(typeof(DateTime))]
    [InlineData(typeof(DateTimeOffset))]
    [InlineData(typeof(TimeSpan))]
    [InlineData(typeof(DayOfWeek))]
    public void Cannot_convert_null_to_value_type(Type type)
    {
        // Act
        Action action = () => RuntimeTypeConverter.ConvertType(null, type);

        // Assert
        action.Should().ThrowExactly<FormatException>().WithMessage($"Failed to convert 'null' to type '{type.Name}'.");
    }

    [Theory]
    [InlineData(typeof(bool?))]
    [InlineData(typeof(byte?))]
    [InlineData(typeof(sbyte?))]
    [InlineData(typeof(char?))]
    [InlineData(typeof(short?))]
    [InlineData(typeof(ushort?))]
    [InlineData(typeof(int?))]
    [InlineData(typeof(uint?))]
    [InlineData(typeof(long?))]
    [InlineData(typeof(ulong?))]
    [InlineData(typeof(float?))]
    [InlineData(typeof(double?))]
    [InlineData(typeof(decimal?))]
    [InlineData(typeof(Guid?))]
    [InlineData(typeof(DateTime?))]
    [InlineData(typeof(DateTimeOffset?))]
    [InlineData(typeof(TimeSpan?))]
    [InlineData(typeof(DayOfWeek?))]
    [InlineData(typeof(string))]
    [InlineData(typeof(IFace))]
    [InlineData(typeof(BaseType))]
    [InlineData(typeof(DerivedType))]
    public void Can_convert_null_to_nullable_type(Type type)
    {
        // Act
        object? result = RuntimeTypeConverter.ConvertType(null, type);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Returns_same_instance_for_exact_type()
    {
        // Arrange
        var instance = new DerivedType();
        Type type = typeof(DerivedType);

        // Act
        object? result = RuntimeTypeConverter.ConvertType(instance, type);

        // Assert
        result.Should().Be(instance);
    }

    [Fact]
    public void Returns_same_instance_for_base_type()
    {
        // Arrange
        var instance = new DerivedType();
        Type type = typeof(BaseType);

        // Act
        object? result = RuntimeTypeConverter.ConvertType(instance, type);

        // Assert
        result.Should().Be(instance);
    }

    [Fact]
    public void Returns_same_instance_for_interface()
    {
        // Arrange
        var instance = new DerivedType();
        Type type = typeof(IFace);

        // Act
        object? result = RuntimeTypeConverter.ConvertType(instance, type);

        // Assert
        result.Should().Be(instance);
    }

    [Theory]
    [InlineData(typeof(bool), default(bool))]
    [InlineData(typeof(bool?), null)]
    [InlineData(typeof(byte), default(byte))]
    [InlineData(typeof(byte?), null)]
    [InlineData(typeof(sbyte), default(sbyte))]
    [InlineData(typeof(sbyte?), null)]
    [InlineData(typeof(char), default(char))]
    [InlineData(typeof(char?), null)]
    [InlineData(typeof(short), default(short))]
    [InlineData(typeof(short?), null)]
    [InlineData(typeof(ushort), default(ushort))]
    [InlineData(typeof(ushort?), null)]
    [InlineData(typeof(int), default(int))]
    [InlineData(typeof(int?), null)]
    [InlineData(typeof(uint), default(uint))]
    [InlineData(typeof(uint?), null)]
    [InlineData(typeof(long), default(long))]
    [InlineData(typeof(long?), null)]
    [InlineData(typeof(ulong), default(ulong))]
    [InlineData(typeof(ulong?), null)]
    [InlineData(typeof(float), default(float))]
    [InlineData(typeof(float?), null)]
    [InlineData(typeof(double), default(double))]
    [InlineData(typeof(double?), null)]
    [InlineData(typeof(decimal), 0)]
    [InlineData(typeof(decimal?), null)]
    [InlineData(typeof(DayOfWeek), DayOfWeek.Sunday)]
    [InlineData(typeof(DayOfWeek?), null)]
    [InlineData(typeof(string), "")]
    [InlineData(typeof(IFace), null)]
    [InlineData(typeof(BaseType), null)]
    [InlineData(typeof(DerivedType), null)]
    public void Returns_default_value_for_empty_string(Type type, object? expectedValue)
    {
        // Act
        object? result = RuntimeTypeConverter.ConvertType(string.Empty, type);

        // Assert
        result.Should().Be(expectedValue);
    }

    private interface IFace;

    private class BaseType : IFace;

    private sealed class DerivedType : BaseType;
}
