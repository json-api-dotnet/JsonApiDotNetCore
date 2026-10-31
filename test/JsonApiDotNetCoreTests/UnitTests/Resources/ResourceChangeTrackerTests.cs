using FluentAssertions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.Resources;

public sealed class ResourceChangeTrackerTests
{
    [Fact]
    public void Identical_compound_has_no_change()
    {
        // Arrange
        TrackedResource request = CreateResource();
        TrackedResource stored = CreateResource();

        // Act
        bool hasChanges = HasImplicitChanges(nameof(TrackedResource.Profile), request, stored);

        // Assert
        hasChanges.Should().BeFalse();
    }

    [Fact]
    public void Nested_leaf_that_differs_is_a_change()
    {
        // Arrange
        TrackedResource request = CreateResource();
        TrackedResource stored = CreateResource();
        stored.Profile!.Address!.City = "Other";

        // Act
        bool hasChanges = HasImplicitChanges(nameof(TrackedResource.Profile), request, stored);

        // Assert
        hasChanges.Should().BeTrue();
    }

    [Fact]
    public void Property_order_inside_compound_is_ignored()
    {
        // Arrange
        TrackedResource request = CreateResource();
        request.Profile!.Extras["a"] = "1";
        request.Profile.Extras["b"] = "2";

        TrackedResource stored = CreateResource();
        stored.Profile!.Extras["b"] = "2";
        stored.Profile.Extras["a"] = "1";

        // Act
        bool hasChanges = HasImplicitChanges(nameof(TrackedResource.Profile), request, stored);

        // Assert
        hasChanges.Should().BeFalse();
    }

    [Fact]
    public void Collection_element_order_is_a_change()
    {
        // Arrange
        TrackedResource request = CreateResource();
        TrackedResource stored = CreateResource();
        stored.Profile!.Phones.Reverse();

        // Act
        bool hasChanges = HasImplicitChanges(nameof(TrackedResource.Profile), request, stored);

        // Assert
        hasChanges.Should().BeTrue();
    }

    [Fact]
    public void Null_versus_empty_collection_is_a_change()
    {
        // Arrange
        TrackedResource request = CreateResource();
        request.Phones = null;

        TrackedResource stored = CreateResource();
        stored.Phones = [];

        // Act
        bool requestNullStoredEmpty = HasImplicitChanges(nameof(TrackedResource.Phones), request, stored);
        bool requestEmptyStoredNull = HasImplicitChanges(nameof(TrackedResource.Phones), stored, request);

        // Assert
        requestNullStoredEmpty.Should().BeTrue();
        requestEmptyStoredNull.Should().BeTrue();
    }

    [Fact]
    public void Null_versus_compound_is_a_change()
    {
        // Arrange
        TrackedResource request = CreateResource();
        request.Profile = null;

        TrackedResource stored = CreateResource();

        // Act
        bool requestNullStoredValue = HasImplicitChanges(nameof(TrackedResource.Profile), request, stored);
        bool requestValueStoredNull = HasImplicitChanges(nameof(TrackedResource.Profile), stored, request);
        bool bothNull = HasImplicitChanges(nameof(TrackedResource.Profile), request, new TrackedResource());

        // Assert
        requestNullStoredValue.Should().BeTrue();
        requestValueStoredNull.Should().BeTrue();
        bothNull.Should().BeFalse();
    }

    [Fact]
    public void One_differing_element_in_compound_collection_is_a_change()
    {
        // Arrange
        TrackedResource request = CreateResource();
        TrackedResource stored = CreateResource();
        stored.Phones![1].Number = "000";

        // Act
        bool hasChanges = HasImplicitChanges(nameof(TrackedResource.Phones), request, stored);

        // Assert
        hasChanges.Should().BeTrue();
    }

    [Fact]
    public void Identical_compound_collection_has_no_change()
    {
        // Arrange
        TrackedResource request = CreateResource();
        TrackedResource stored = CreateResource();

        // Act
        bool hasChanges = HasImplicitChanges(nameof(TrackedResource.Phones), request, stored);

        // Assert
        hasChanges.Should().BeFalse();
    }

    [Fact]
    public void Scalar_collections_of_Guid_are_compared_by_content()
    {
        // Arrange
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();

        var request = new TrackedResource
        {
            Ids = [first, second]
        };

        var equalStored = new TrackedResource
        {
            Ids = [first, second]
        };

        var changedStored = new TrackedResource
        {
            Ids = [first, Guid.NewGuid()]
        };

        // Act
        bool equal = HasImplicitChanges(nameof(TrackedResource.Ids), request, equalStored);
        bool changed = HasImplicitChanges(nameof(TrackedResource.Ids), request, changedStored);

        // Assert
        equal.Should().BeFalse();
        changed.Should().BeTrue();
    }

    [Fact]
    public void Scalar_collections_of_TimeSpan_are_compared_by_content()
    {
        // Arrange
        var request = new TrackedResource
        {
            Durations =
            [
                TimeSpan.FromMinutes(5),
                TimeSpan.FromHours(1)
            ]
        };

        var equalStored = new TrackedResource
        {
            Durations =
            [
                TimeSpan.FromMinutes(5),
                TimeSpan.FromHours(1)
            ]
        };

        var changedStored = new TrackedResource
        {
            Durations =
            [
                TimeSpan.FromMinutes(5),
                TimeSpan.FromHours(2)
            ]
        };

        // Act
        bool equal = HasImplicitChanges(nameof(TrackedResource.Durations), request, equalStored);
        bool changed = HasImplicitChanges(nameof(TrackedResource.Durations), request, changedStored);

        // Assert
        equal.Should().BeFalse();
        changed.Should().BeTrue();
    }

    [Fact]
    public void Byte_array_is_compared_by_reference_and_always_a_change()
    {
        // Arrange
        var request = new TrackedResource
        {
            Data = [1, 2, 3]
        };

        var equalStored = new TrackedResource
        {
            Data = [1, 2, 3]
        };

        var changedStored = new TrackedResource
        {
            Data = [1, 2, 4]
        };

        // Act
        bool equal = HasImplicitChanges(nameof(TrackedResource.Data), request, equalStored);
        bool changed = HasImplicitChanges(nameof(TrackedResource.Data), request, changedStored);

        // Assert
        equal.Should().BeTrue();
        changed.Should().BeTrue();
    }

    [Fact]
    public void Non_compound_complex_type_is_always_a_change_for_new_instance()
    {
        // Arrange
        var request = new TrackedResource
        {
            Plain = new PlainComplex
            {
                Value = "same"
            }
        };

        var stored = new TrackedResource
        {
            Plain = new PlainComplex
            {
                Value = "same"
            }
        };

        // Act
        bool differentInstances = HasImplicitChanges(nameof(TrackedResource.Plain), request, stored);
        bool sameInstance = HasImplicitChanges(nameof(TrackedResource.Plain), request, request);

        // Assert
        differentInstances.Should().BeTrue();
        sameInstance.Should().BeFalse();
    }

    [Fact]
    public void Scalar_with_Equals_implementation_is_compared_with_Equals()
    {
        // Arrange
        var request = new TrackedResource
        {
            Price = new Money(9.99m)
        };

        var equalStored = new TrackedResource
        {
            Price = new Money(9.99m)
        };

        var changedStored = new TrackedResource
        {
            Price = new Money(1.00m)
        };

        // Act
        bool equal = HasImplicitChanges(nameof(TrackedResource.Price), request, equalStored);
        bool changed = HasImplicitChanges(nameof(TrackedResource.Price), request, changedStored);

        // Assert
        equal.Should().BeFalse();
        changed.Should().BeTrue();
    }

    private static TrackedResource CreateResource()
    {
        return new TrackedResource
        {
            Profile = new ProfileCompound
            {
                Name = "Alice",
                Address = new AddressCompound
                {
                    City = "Springfield"
                },
                Phones =
                [
                    new PhoneCompound
                    {
                        Number = "111"
                    },
                    new PhoneCompound
                    {
                        Number = "222"
                    }
                ]
            },
            Phones =
            [
                new PhoneCompound
                {
                    Number = "111"
                },
                new PhoneCompound
                {
                    Number = "222"
                }
            ]
        };
    }

    private static bool HasImplicitChanges(string targetedPropertyName, TrackedResource requestResource, TrackedResource storedResource)
    {
        var options = new JsonApiOptions();
        IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Add<TrackedResource, long>().Build();
        ResourceType resourceType = resourceGraph.GetResourceType<TrackedResource>();

        var request = new JsonApiRequest
        {
            Kind = EndpointKind.Primary,
            PrimaryResourceType = resourceType
        };

        var targetedFields = new TargetedFields();
        targetedFields.Attributes.Add(resourceType.GetAttributeByPropertyName(targetedPropertyName));

        var tracker = new ResourceChangeTracker<TrackedResource>(request, targetedFields);
        tracker.SetInitiallyStoredAttributeValues(storedResource);
        tracker.SetRequestAttributeValues(requestResource);
        tracker.SetFinallyStoredAttributeValues(storedResource);

        return tracker.HasImplicitChanges();
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class TrackedResource : Identifiable<long>
    {
        [Attr(IsCompound = true)]
        public ProfileCompound? Profile { get; set; }

        [Attr(IsCompound = true)]
        public List<PhoneCompound>? Phones { get; set; }

        [Attr]
        public List<Guid>? Ids { get; set; }

        [Attr]
        public List<TimeSpan>? Durations { get; set; }

        [Attr]
        public byte[]? Data { get; set; }

        [Attr]
        public PlainComplex? Plain { get; set; }

        [Attr]
        public Money? Price { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class ProfileCompound
    {
        [Attr]
        public string? Name { get; set; }

        [Attr(IsCompound = true)]
        public AddressCompound? Address { get; set; }

        [Attr(IsCompound = true)]
        public List<PhoneCompound> Phones { get; set; } = [];

        // Not exposed as attribute, but included when the change tracker serializes the whole object.
        public Dictionary<string, string> Extras { get; } = [];
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private sealed class AddressCompound
    {
        [Attr]
        public string? City { get; set; }
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

    private sealed record Money(decimal Amount);
}
