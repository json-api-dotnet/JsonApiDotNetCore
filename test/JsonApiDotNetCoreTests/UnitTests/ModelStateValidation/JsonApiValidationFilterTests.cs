using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.ModelStateValidation;

public sealed class JsonApiValidationFilterTests
{
    private static readonly EmptyModelMetadataProvider MetadataProvider = new();

    [Fact]
    public void Validates_nested_entry_inside_compound_type()
    {
        // Arrange
        JsonApiValidationFilter filter = CreateFilter(WriteOperationKind.CreateResource, EndpointKind.Primary);
        ValidationEntry parent = CreateTopLevelPropertyEntry(nameof(Customer.Profile));
        ValidationEntry entry = CreatePropertyEntry(typeof(ProfileCompound), nameof(ProfileCompound.Name), "Profile.Name");

        // Act
        bool result = filter.ShouldValidateEntry(entry, parent);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Skips_nested_entry_inside_non_compound_complex_type()
    {
        // Arrange
        JsonApiValidationFilter filter = CreateFilter(WriteOperationKind.CreateResource, EndpointKind.Primary);
        ValidationEntry parent = CreateTopLevelPropertyEntry(nameof(Customer.Plain));
        ValidationEntry entry = CreatePropertyEntry(typeof(PlainComplex), nameof(PlainComplex.Value), "Plain.Value");

        // Act
        bool result = filter.ShouldValidateEntry(entry, parent);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Validates_nested_entry_inside_compound_type_when_patching_targeted_compound()
    {
        // Arrange
        JsonApiValidationFilter filter = CreateFilter(WriteOperationKind.UpdateResource, EndpointKind.Primary, nameof(Customer.Profile));
        ValidationEntry rootEntry = CreateRootEntry();
        ValidationEntry compoundEntry = CreateTopLevelPropertyEntry(nameof(Customer.Profile));
        ValidationEntry nestedEntry = CreatePropertyEntry(typeof(ProfileCompound), nameof(ProfileCompound.Name), "Profile.Name");

        // Act
        bool topLevelResult = filter.ShouldValidateEntry(compoundEntry, rootEntry);
        bool nestedResult = filter.ShouldValidateEntry(nestedEntry, compoundEntry);

        // Assert
        topLevelResult.Should().BeTrue();
        nestedResult.Should().BeTrue();
    }

    [Fact]
    public void Skips_compound_that_is_not_targeted_when_patching()
    {
        // Arrange
        JsonApiValidationFilter filter = CreateFilter(WriteOperationKind.UpdateResource, EndpointKind.Primary, nameof(Customer.Title));
        ValidationEntry rootEntry = CreateRootEntry();
        ValidationEntry compoundEntry = CreateTopLevelPropertyEntry(nameof(Customer.Profile));

        // Act
        bool result = filter.ShouldValidateEntry(compoundEntry, rootEntry);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(EndpointKind.Relationship)]
    [InlineData(EndpointKind.Secondary)]
    public void Skips_entries_at_non_primary_endpoint(EndpointKind endpointKind)
    {
        // Arrange
        JsonApiValidationFilter filter = CreateFilter(WriteOperationKind.SetRelationship, endpointKind);
        ValidationEntry parent = CreateTopLevelPropertyEntry(nameof(Customer.Profile));
        ValidationEntry entry = CreatePropertyEntry(typeof(ProfileCompound), nameof(ProfileCompound.Name), "Profile.Name");

        // Act
        bool result = filter.ShouldValidateEntry(entry, parent);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Validates_element_of_compound_collection_because_it_is_a_type_entry_and_validates_its_members()
    {
        // Arrange
        JsonApiValidationFilter filter = CreateFilter(WriteOperationKind.CreateResource, EndpointKind.Primary);

        ValidationEntry collectionEntry = CreateTopLevelPropertyEntry(nameof(Customer.Phones));
        var elementEntry = new ValidationEntry(MetadataProvider.GetMetadataForType(typeof(PhoneCompound)), "Phones[0]", new object());
        ValidationEntry elementPropertyEntry = CreatePropertyEntry(typeof(PhoneCompound), nameof(PhoneCompound.Number), "Phones[0].Number");

        // Act
        bool elementResult = filter.ShouldValidateEntry(elementEntry, collectionEntry);
        bool elementPropertyResult = filter.ShouldValidateEntry(elementPropertyEntry, elementEntry);

        // Assert
        elementResult.Should().BeTrue();
        elementPropertyResult.Should().BeTrue();
    }

    private static JsonApiValidationFilter CreateFilter(WriteOperationKind writeOperation, EndpointKind endpointKind, params string[] targetedPropertyNames)
    {
        var options = new JsonApiOptions();
        IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Add<Customer, long>().Build();
        ResourceType resourceType = resourceGraph.GetResourceType<Customer>();

        var request = new JsonApiRequest
        {
            Kind = endpointKind,
            PrimaryResourceType = resourceType,
            WriteOperation = writeOperation
        };

        var targetedFields = new TargetedFields();

        foreach (string propertyName in targetedPropertyNames)
        {
            targetedFields.Attributes.Add(resourceType.GetAttributeByPropertyName(propertyName));
        }

        ServiceProvider serviceProvider = new ServiceCollection().AddSingleton<IJsonApiRequest>(request).AddSingleton<ITargetedFields>(targetedFields)
            .AddSingleton(resourceGraph).BuildServiceProvider();

        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider
            }
        };

        return new JsonApiValidationFilter(httpContextAccessor);
    }

    private static ValidationEntry CreateRootEntry()
    {
        return new ValidationEntry(MetadataProvider.GetMetadataForType(typeof(Customer)), string.Empty, new object());
    }

    private static ValidationEntry CreateTopLevelPropertyEntry(string propertyName)
    {
        return CreatePropertyEntry(typeof(Customer), propertyName, propertyName);
    }

    private static ValidationEntry CreatePropertyEntry(Type containerType, string propertyName, string key)
    {
        ModelMetadata metadata = MetadataProvider.GetMetadataForProperties(containerType).Single(property => property.PropertyName == propertyName);
        return new ValidationEntry(metadata, key, new object());
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class Customer : Identifiable<long>
    {
        [Attr]
        public string? Title { get; set; }

        [Attr(IsCompound = true)]
        public ProfileCompound? Profile { get; set; }

        [Attr(IsCompound = true)]
        public List<PhoneCompound>? Phones { get; set; }

        [Attr]
        public PlainComplex? Plain { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ProfileCompound
    {
        [Attr]
        public string? Name { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class PhoneCompound
    {
        [Attr]
        public string? Number { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class PlainComplex
    {
        public string? Value { get; set; }
    }
}
