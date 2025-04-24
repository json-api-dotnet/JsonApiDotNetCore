using FluentAssertions;
using JsonApiDotNetCore;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.TypeConversion;

public sealed class CollectionConverterTests
{
    [Fact]
    public void Finds_element_type_for_generic_list()
    {
        // Arrange
        Type sourceType = typeof(List<string>);

        // Act
        Type? elementType = CollectionConverter.Instance.FindCollectionElementType(sourceType);

        // Assert
        elementType.Should().Be<string>();
    }

    [Fact]
    public void Finds_element_type_for_generic_enumerable()
    {
        // Arrange
        Type sourceType = typeof(IEnumerable<string>);

        // Act
        Type? elementType = CollectionConverter.Instance.FindCollectionElementType(sourceType);

        // Assert
        elementType.Should().Be<string>();
    }

    [Fact]
    public void Finds_element_type_for_custom_generic_collection_with_multiple_type_parameters()
    {
        // Arrange
        Type sourceType = typeof(CustomCollection<int, string>);

        // Act
        Type? elementType = CollectionConverter.Instance.FindCollectionElementType(sourceType);

        // Assert
        elementType.Should().Be<string>();
    }

    [Fact]
    public void Finds_element_type_for_custom_non_generic_collection()
    {
        // Arrange
        Type sourceType = typeof(CustomCollectionOfIntString);

        // Act
        Type? elementType = CollectionConverter.Instance.FindCollectionElementType(sourceType);

        // Assert
        elementType.Should().Be<string>();
    }

    [Fact]
    public void Finds_no_element_type_for_non_generic_type()
    {
        // Arrange
        Type sourceType = typeof(int);

        // Act
        Type? elementType = CollectionConverter.Instance.FindCollectionElementType(sourceType);

        // Assert
        elementType.Should().BeNull();
    }

    [Fact]
    public void Finds_no_element_type_for_non_collection_generic_type()
    {
        // Arrange
        Type sourceType = typeof(Tuple<int, string>);

        // Act
        Type? elementType = CollectionConverter.Instance.FindCollectionElementType(sourceType);

        // Assert
        elementType.Should().BeNull();
    }

    [Fact]
    public void Finds_no_element_type_for_unbound_generic_type()
    {
        // Arrange
        Type sourceType = typeof(List<>);

        // Act
        Type? elementType = CollectionConverter.Instance.FindCollectionElementType(sourceType);

        // Assert
        elementType.Should().BeNull();
    }

    // ReSharper disable once UnusedTypeParameter
    private class CustomCollection<TOther, TElement> : List<TElement>;

    private sealed class CustomCollectionOfIntString : CustomCollection<int, string>;
}
