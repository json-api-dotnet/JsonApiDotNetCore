using System.Text.Json.Serialization;
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
    public void Non_attribute_member_of_compound_that_differs_is_a_change()
    {
        // Arrange
        TrackedResource request = CreateResource();
        request.Profile!.LastUpdated = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        TrackedResource stored = CreateResource();
        stored.Profile!.LastUpdated = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        bool hasChanges = HasImplicitChanges(nameof(TrackedResource.Profile), request, stored);

        // Assert
        hasChanges.Should().BeTrue();
    }

    [Fact]
    public void Non_attribute_member_of_compound_that_is_set_afterwards_is_a_change()
    {
        // Arrange
        TrackedResource request = CreateResource();
        TrackedResource stored = CreateResource();

        // Act
        bool hasChanges = HasImplicitChangesAfterMutation(nameof(TrackedResource.Plain), request, stored,
            resource => resource.Profile!.LastUpdated = DateTime.UtcNow);

        // Assert
        hasChanges.Should().BeTrue();
    }

    [Fact]
    public void Member_with_JsonIgnore_is_not_compared()
    {
        // Arrange
        TrackedResource request = CreateResource();
        request.Profile!.Hidden = "one";

        TrackedResource stored = CreateResource();
        stored.Profile!.Hidden = "two";

        // Act
        bool hasChanges = HasImplicitChanges(nameof(TrackedResource.Profile), request, stored);

        // Assert
        hasChanges.Should().BeFalse();
    }

    [Fact]
    public void Can_compare_NaN_and_Infinity_in_scalar_collection_and_compound_member()
    {
        // Arrange
        TrackedResource request = CreateResource();

        request.Doubles =
        [
            double.NaN,
            double.PositiveInfinity,
            double.NegativeInfinity
        ];

        request.Profile!.Ratio = double.NaN;

        TrackedResource stored = CreateResource();

        stored.Doubles =
        [
            double.NaN,
            double.PositiveInfinity,
            double.NegativeInfinity
        ];

        stored.Profile!.Ratio = double.NaN;

        TrackedResource changed = CreateResource();

        changed.Doubles =
        [
            double.NaN,
            double.PositiveInfinity,
            1
        ];

        changed.Profile!.Ratio = double.NaN;

        // Act
        bool equalCollection = HasImplicitChanges(nameof(TrackedResource.Doubles), request, stored);
        bool equalCompound = HasImplicitChanges(nameof(TrackedResource.Profile), request, stored);
        bool changedCollection = HasImplicitChanges(nameof(TrackedResource.Doubles), request, changed);

        // Assert
        equalCollection.Should().BeFalse();
        equalCompound.Should().BeFalse();
        changedCollection.Should().BeTrue();
    }

    [Fact]
    public void Untargeted_attribute_with_NaN_does_not_fail()
    {
        // Arrange
        TrackedResource request = CreateResource();
        TrackedResource stored = CreateResource();
        stored.Doubles = [double.NaN];

        // Act
        bool hasChanges = HasImplicitChanges(nameof(TrackedResource.Plain), request, stored);

        // Assert
        hasChanges.Should().BeFalse();
    }

    [Fact]
    public void Can_compare_struct_compound()
    {
        // Arrange
        var request = new TrackedResource
        {
            Position = new GeoStruct
            {
                X = 1,
                Y = 2
            }
        };

        var equalStored = new TrackedResource
        {
            Position = new GeoStruct
            {
                X = 1,
                Y = 2
            }
        };

        var changedStored = new TrackedResource
        {
            Position = new GeoStruct
            {
                X = 1,
                Y = 3
            }
        };

        // Act
        bool equal = HasImplicitChanges(nameof(TrackedResource.Position), request, equalStored);
        bool changed = HasImplicitChanges(nameof(TrackedResource.Position), request, changedStored);

        // Assert
        equal.Should().BeFalse();
        changed.Should().BeTrue();
    }

    [Fact]
    public void Can_compare_nullable_struct_compound()
    {
        // Arrange
        var withValue = new TrackedResource
        {
            NullablePosition = new GeoStruct
            {
                X = 1,
                Y = 2
            }
        };

        var sameValue = new TrackedResource
        {
            NullablePosition = new GeoStruct
            {
                X = 1,
                Y = 2
            }
        };

        var otherValue = new TrackedResource
        {
            NullablePosition = new GeoStruct
            {
                X = 9,
                Y = 2
            }
        };

        var withoutValue = new TrackedResource();

        // Act
        bool equal = HasImplicitChanges(nameof(TrackedResource.NullablePosition), withValue, sameValue);
        bool changed = HasImplicitChanges(nameof(TrackedResource.NullablePosition), withValue, otherValue);
        bool nullToValue = HasImplicitChanges(nameof(TrackedResource.NullablePosition), withValue, withoutValue);
        bool valueToNull = HasImplicitChanges(nameof(TrackedResource.NullablePosition), withoutValue, withValue);
        bool bothNull = HasImplicitChanges(nameof(TrackedResource.NullablePosition), withoutValue, new TrackedResource());

        // Assert
        equal.Should().BeFalse();
        changed.Should().BeTrue();
        nullToValue.Should().BeTrue();
        valueToNull.Should().BeTrue();
        bothNull.Should().BeFalse();
    }

    [Fact]
    public void Compound_with_all_members_null_differs_from_null_compound()
    {
        // Arrange
        var emptyObject = new TrackedResource
        {
            Profile = new ProfileCompound()
        };

        var nullValue = new TrackedResource
        {
            Profile = null
        };

        // Act
        bool requestEmptyObjectStoredNull = HasImplicitChanges(nameof(TrackedResource.Profile), emptyObject, nullValue);
        bool requestNullStoredEmptyObject = HasImplicitChanges(nameof(TrackedResource.Profile), nullValue, emptyObject);

        // Assert
        requestEmptyObjectStoredNull.Should().BeTrue();
        requestNullStoredEmptyObject.Should().BeTrue();
    }

    [Fact]
    public void Null_element_in_compound_collection_is_compared()
    {
        // Arrange
        var request = new TrackedResource
        {
            Phones =
            [
                new PhoneCompound(),
                null!
            ]
        };

        var equalStored = new TrackedResource
        {
            Phones =
            [
                new PhoneCompound(),
                null!
            ]
        };

        var changedStored = new TrackedResource
        {
            Phones =
            [
                new PhoneCompound(),
                new PhoneCompound()
            ]
        };

        // Act
        bool equal = HasImplicitChanges(nameof(TrackedResource.Phones), request, equalStored);
        bool changed = HasImplicitChanges(nameof(TrackedResource.Phones), request, changedStored);

        // Assert
        equal.Should().BeFalse();
        changed.Should().BeTrue();
    }

    [Fact]
    public void Set_is_compared_by_enumeration_order_and_content()
    {
        // Arrange
        var request = new TrackedResource
        {
            Set = new HashSet<int>([
                1,
                2,
                3
            ])
        };

        var sameStored = new TrackedResource
        {
            Set = new HashSet<int>([
                1,
                2,
                3
            ])
        };

        var changedStored = new TrackedResource
        {
            Set = new HashSet<int>([
                3,
                2,
                4
            ])
        };

        var smallerStored = new TrackedResource
        {
            Set = new HashSet<int>([
                1,
                2
            ])
        };

        // Act
        bool same = HasImplicitChanges(nameof(TrackedResource.Set), request, sameStored);
        bool changed = HasImplicitChanges(nameof(TrackedResource.Set), request, changedStored);
        bool smaller = HasImplicitChanges(nameof(TrackedResource.Set), request, smallerStored);

        // Assert
        same.Should().BeFalse();
        changed.Should().BeTrue();
        smaller.Should().BeTrue();
    }

    [Fact]
    public void List_of_scalars_with_different_order_is_a_change()
    {
        // Arrange
        var request = new TrackedResource
        {
            Numbers =
            [
                1,
                2,
                3
            ]
        };

        var stored = new TrackedResource
        {
            Numbers =
            [
                3,
                2,
                1
            ]
        };

        // Act
        bool hasChanges = HasImplicitChanges(nameof(TrackedResource.Numbers), request, stored);

        // Assert
        hasChanges.Should().BeTrue();
    }

    [Fact]
    public void List_of_scalars_with_identical_content_is_not_a_change()
    {
        // Arrange
        var request = new TrackedResource
        {
            Numbers =
            [
                1,
                2,
                3
            ]
        };

        var stored = new TrackedResource
        {
            Numbers =
            [
                1,
                2,
                3
            ]
        };

        // Act
        bool hasChanges = HasImplicitChanges(nameof(TrackedResource.Numbers), request, stored);

        // Assert
        hasChanges.Should().BeFalse();
    }

    [Fact]
    public void Byte_array_member_inside_compound_is_compared_by_content()
    {
        // Arrange
        TrackedResource request = CreateResource();

        request.Profile!.Blob =
        [
            1,
            2,
            3
        ];

        TrackedResource equalStored = CreateResource();

        equalStored.Profile!.Blob =
        [
            1,
            2,
            3
        ];

        TrackedResource changedStored = CreateResource();

        changedStored.Profile!.Blob =
        [
            1,
            2,
            4
        ];

        // Act
        bool equal = HasImplicitChanges(nameof(TrackedResource.Profile), request, equalStored);
        bool changed = HasImplicitChanges(nameof(TrackedResource.Profile), request, changedStored);

        // Assert
        equal.Should().BeFalse();
        changed.Should().BeTrue();
    }

    [Fact]
    public void In_place_mutation_of_nested_compound_member_is_detected()
    {
        // Arrange
        TrackedResource request = CreateResource();
        TrackedResource stored = CreateResource();

        // Act
        bool hasChanges =
            HasImplicitChangesAfterMutation(nameof(TrackedResource.Plain), request, stored, resource => resource.Profile!.Address!.City = "Other");

        // Assert
        hasChanges.Should().BeTrue();
    }

    [Fact]
    public void In_place_mutation_of_element_in_compound_collection_is_detected()
    {
        // Arrange
        TrackedResource request = CreateResource();
        TrackedResource stored = CreateResource();

        // Act
        bool hasChanges = HasImplicitChangesAfterMutation(nameof(TrackedResource.Plain), request, stored, resource => resource.Phones![0].Number = "999");

        // Assert
        hasChanges.Should().BeTrue();
    }

    [Fact]
    public void In_place_mutation_of_targeted_compound_is_detected()
    {
        // Arrange
        TrackedResource request = CreateResource();
        TrackedResource stored = CreateResource();

        // Act
        bool hasChanges = HasImplicitChangesAfterMutation(nameof(TrackedResource.Profile), request, stored, resource => resource.Profile!.Name = "Bob");

        // Assert
        hasChanges.Should().BeTrue();
    }

    [Fact]
    public void In_place_mutation_of_scalar_collection_is_detected()
    {
        // Arrange
        var request = new TrackedResource
        {
            Numbers =
            [
                1,
                2
            ]
        };

        var stored = new TrackedResource
        {
            Numbers =
            [
                1,
                2
            ]
        };

        // Act
        bool hasChanges = HasImplicitChangesAfterMutation(nameof(TrackedResource.Numbers), request, stored, resource => resource.Numbers!.Add(3));

        // Assert
        hasChanges.Should().BeTrue();
    }

    [Fact]
    public void In_place_mutation_of_untargeted_byte_array_is_detected()
    {
        // Arrange
        var request = new TrackedResource();

        var stored = new TrackedResource
        {
            Data =
            [
                1,
                2,
                3
            ]
        };

        // Act
        bool hasChanges = HasImplicitChangesAfterMutation(nameof(TrackedResource.Plain), request, stored, resource => resource.Data![0] = 9);

        // Assert
        hasChanges.Should().BeTrue();
    }

    [Fact]
    public void In_place_mutation_of_nullable_struct_compound_is_detected()
    {
        // Arrange
        var request = new TrackedResource
        {
            NullablePosition = new GeoStruct
            {
                X = 1,
                Y = 2
            }
        };

        var stored = new TrackedResource
        {
            NullablePosition = new GeoStruct
            {
                X = 1,
                Y = 2
            }
        };

        // Act
        bool hasChanges = HasImplicitChangesAfterMutation(nameof(TrackedResource.NullablePosition), request, stored, resource => resource.NullablePosition =
            new GeoStruct
            {
                X = 5,
                Y = 2
            });

        // Assert
        hasChanges.Should().BeTrue();
    }

    [Fact]
    public void In_place_mutation_of_set_is_detected()
    {
        // Arrange
        var request = new TrackedResource
        {
            Set = new HashSet<int>([
                1,
                2
            ])
        };

        var stored = new TrackedResource
        {
            Set = new HashSet<int>([
                1,
                2
            ])
        };

        // Act
        bool hasChanges = HasImplicitChangesAfterMutation(nameof(TrackedResource.Set), request, stored, resource => resource.Set!.Add(3));

        // Assert
        hasChanges.Should().BeTrue();
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
            Ids =
            [
                first,
                second
            ]
        };

        var equalStored = new TrackedResource
        {
            Ids =
            [
                first,
                second
            ]
        };

        var changedStored = new TrackedResource
        {
            Ids =
            [
                first,
                Guid.NewGuid()
            ]
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
    public void Byte_array_is_compared_by_content()
    {
        // Arrange
        var request = new TrackedResource
        {
            Data =
            [
                1,
                2,
                3
            ]
        };

        var equalStored = new TrackedResource
        {
            Data =
            [
                1,
                2,
                3
            ]
        };

        var changedStored = new TrackedResource
        {
            Data =
            [
                1,
                2,
                4
            ]
        };

        // Act
        bool hasChangesForEqualContent = HasImplicitChanges(nameof(TrackedResource.Data), request, equalStored);
        bool hasChangesForChangedContent = HasImplicitChanges(nameof(TrackedResource.Data), request, changedStored);

        // Assert
        hasChangesForEqualContent.Should().BeFalse();
        hasChangesForChangedContent.Should().BeTrue();
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
        return HasImplicitChangesAfterMutation(targetedPropertyName, requestResource, storedResource, _ =>
        {
        });
    }

    private static bool HasImplicitChangesAfterMutation(string targetedPropertyName, TrackedResource requestResource, TrackedResource storedResource,
        Action<TrackedResource> mutateStoredResource)
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

        mutateStoredResource(storedResource);
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

        [Attr]
        public List<double>? Doubles { get; set; }

        [Attr]
        public List<int>? Numbers { get; set; }

        [Attr]
        public HashSet<int>? Set { get; set; }

        [Attr(IsCompound = true)]
        public GeoStruct Position { get; set; }

        [Attr(IsCompound = true)]
        public GeoStruct? NullablePosition { get; set; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private struct GeoStruct
    {
        [Attr]
        public double X { get; set; }

        [Attr]
        public double Y { get; set; }
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

        [Attr]
        [JsonIgnore]
        public string? Hidden { get; set; }

        [Attr]
        public double? Ratio { get; set; }

        [Attr]
        public byte[]? Blob { get; set; }

        // Not exposed as attribute, but still compared.
        public DateTime? LastUpdated { get; set; }
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
