using System.Numerics;
using System.Text;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.AttributeTypes;

/// <summary>
/// Verifies that collections of certain element types cannot be mapped inside a JSON column, because EF Core does not apply the value converter to the
/// elements of a primitive collection and treats the collection property as a navigation instead. Each type is built in its own tiny model, so a fix in
/// EF Core makes the corresponding row fail, after which the type can be added to <see cref="TypeContainerNested" />.
/// </summary>
public sealed class UnsupportedElementTypeTests
{
    [Theory]
    [InlineData(typeof(Int128?))]
    [InlineData(typeof(UInt128?))]
    [InlineData(typeof(Half?))]
    [InlineData(typeof(Rune?))]
    [InlineData(typeof(Version))]
    [InlineData(typeof(Complex?))]
    public void EF_Core_10_cannot_map_collection_of_element_type_inside_json(Type elementType)
    {
        // Arrange
        Func<IModel> action = () =>
        {
            using var dbContext = new ElementTypeDbContext(elementType);

            // Act
            return dbContext.Model;
        };

        // Assert
        action.Should().Throw<InvalidOperationException>().WithMessage("*as complex types don't support navigations*");
    }

    private sealed class ElementTypeDbContext(Type elementType) : DbContext
    {
        private readonly Type _elementType = elementType;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Database=unused");
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            Type nestedType = typeof(ElementNested<>).MakeGenericType(_elementType);
            Type holderType = typeof(ElementHolder<>).MakeGenericType(_elementType);

            builder.Entity(holderType).ComplexProperty(nestedType, nameof(ElementHolder<>.Nested), complexBuilder => complexBuilder.ToJson());
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder builder)
        {
            AttributeTypesDbContext.ConfigureTypeConventions(builder);
        }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ElementHolder<T>
    {
        public long Id { get; set; }
        public ElementNested<T>? Nested { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ElementNested<T>
    {
        public int Marker { get; set; }
        public List<T> Items { get; set; } = [];
    }
}
