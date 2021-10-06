using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using FluentAssertions;
using FluentAssertions.Extensions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.JsonConverters;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCore.Serialization.Request.Adapters;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace JsonApiDotNetCoreTests.UnitTests.Serialization
{
    public sealed class InputConversionTests
    {
        [Fact]
        public void Converts_various_data_types_with_values()
        {
            // Arrange
            DocumentAdapter documentAdapter = CreateDocumentAdapter<ResourceWithVariousDataTypes>(resourceGraph => new JsonApiRequest
            {
                Kind = EndpointKind.Primary,
                PrimaryResource = resourceGraph.GetResourceContext<ResourceWithVariousDataTypes>(),
                WriteOperation = WriteOperationKind.CreateResource
            });

            const bool booleanValue = true;
            const bool nullableBooleanValue = false;
            const char charValue = 'A';
            const char nullableCharValue = '?';
            const ulong unsignedLongValue = ulong.MaxValue;
            const ulong nullableUnsignedLongValue = 9_000_000_000UL;
            const decimal decimalValue = 19.95m;
            const decimal nullableDecimalValue = 12.50m;
            const float floatValue = (float)1 / 3;
            const float nullableFloatValue = (float)3 / 5;
            const string stringValue = "text";
            var guidValue = Guid.NewGuid();
            var nullableGuidValue = Guid.NewGuid();
            DateTime dateTimeValue = 12.July(1982);
            DateTime nullableDateTimeValue = 18.October(2028);
            DateTimeOffset dateTimeOffsetValue = 3.March(1999).WithOffset(7.Hours());
            DateTimeOffset nullableDateTimeOffsetValue = 28.February(2009).WithOffset(-2.Hours());
            TimeSpan timeSpanValue = 4.Hours().And(58.Minutes());
            TimeSpan nullableTimeSpanValue = 35.Seconds().And(44.Milliseconds());
            const DayOfWeek enumValue = DayOfWeek.Wednesday;
            const DayOfWeek nullableEnumValue = DayOfWeek.Sunday;

            var complexObject = new ComplexObject
            {
                Value = "Single"
            };

            var complexObjectList = new List<ComplexObject>
            {
                new()
                {
                    Value = "One"
                },
                new()
                {
                    Value = "Two"
                }
            };

            var document = new Document
            {
                Data = new SingleOrManyData<ResourceObject>(new ResourceObject
                {
                    Type = "resourceWithVariousDataTypes",
                    Attributes = new Dictionary<string, object>
                    {
                        ["boolean"] = booleanValue,
                        ["nullableBoolean"] = nullableBooleanValue,
                        ["char"] = charValue,
                        ["nullableChar"] = nullableCharValue,
                        ["unsignedLong"] = unsignedLongValue,
                        ["nullableUnsignedLong"] = nullableUnsignedLongValue,
                        ["decimal"] = decimalValue,
                        ["nullableDecimal"] = nullableDecimalValue,
                        ["float"] = floatValue,
                        ["nullableFloat"] = nullableFloatValue,
                        ["string"] = stringValue,
                        ["guid"] = guidValue,
                        ["nullableGuid"] = nullableGuidValue,
                        ["dateTime"] = dateTimeValue,
                        ["nullableDateTime"] = nullableDateTimeValue,
                        ["dateTimeOffset"] = dateTimeOffsetValue,
                        ["nullableDateTimeOffset"] = nullableDateTimeOffsetValue,
                        ["timeSpan"] = timeSpanValue,
                        ["nullableTimeSpan"] = nullableTimeSpanValue,
                        ["enum"] = enumValue,
                        ["nullableEnum"] = nullableEnumValue,
                        ["complexObject"] = complexObject,
                        ["complexObjectList"] = complexObjectList
                    }
                })
            };

            // Act
            var model = (ResourceWithVariousDataTypes)documentAdapter.Convert(document);

            // Assert
            model.Should().NotBeNull();

            model.Boolean.Should().Be(booleanValue);
            model.NullableBoolean.Should().Be(nullableBooleanValue);
            model.Char.Should().Be(charValue);
            model.NullableChar.Should().Be(nullableCharValue);
            model.UnsignedLong.Should().Be(unsignedLongValue);
            model.NullableUnsignedLong.Should().Be(nullableUnsignedLongValue);
            model.Decimal.Should().Be(decimalValue);
            model.NullableDecimal.Should().Be(nullableDecimalValue);
            model.Float.Should().Be(floatValue);
            model.NullableFloat.Should().Be(nullableFloatValue);
            model.String.Should().Be(stringValue);
            model.Guid.Should().Be(guidValue);
            model.NullableGuid.Should().Be(nullableGuidValue);
            model.DateTime.Should().Be(dateTimeValue);
            model.NullableDateTime.Should().Be(nullableDateTimeValue);
            model.DateTimeOffset.Should().Be(dateTimeOffsetValue);
            model.NullableDateTimeOffset.Should().Be(nullableDateTimeOffsetValue);
            model.TimeSpan.Should().Be(timeSpanValue);
            model.NullableTimeSpan.Should().Be(nullableTimeSpanValue);
            model.Enum.Should().Be(enumValue);
            model.NullableEnum.Should().Be(nullableEnumValue);

            model.ComplexObject.Should().NotBeNull();
            model.ComplexObject.Value.Should().Be(complexObject.Value);

            model.ComplexObjectList.Should().HaveCount(2);
            model.ComplexObjectList[0].Value.Should().Be(complexObjectList[0].Value);
            model.ComplexObjectList[1].Value.Should().Be(complexObjectList[1].Value);
        }

        [Fact]
        public void Converts_various_data_types_with_defaults()
        {
            // Arrange
            DocumentAdapter documentAdapter = CreateDocumentAdapter<ResourceWithVariousDataTypes>(resourceGraph => new JsonApiRequest
            {
                Kind = EndpointKind.Primary,
                PrimaryResource = resourceGraph.GetResourceContext<ResourceWithVariousDataTypes>(),
                WriteOperation = WriteOperationKind.CreateResource
            });

            const bool booleanValue = default;
            const bool nullableBooleanValue = default;
            const char charValue = default;
            const char nullableCharValue = default;
            const ulong unsignedLongValue = default;
            const ulong nullableUnsignedLongValue = default;
            const decimal decimalValue = default;
            const decimal nullableDecimalValue = default;
            const float floatValue = default;
            const float nullableFloatValue = default;
            const string stringValue = default;
            Guid guidValue = default;
            Guid nullableGuidValue = default;
            DateTime dateTimeValue = default;
            DateTime nullableDateTimeValue = default;
            DateTimeOffset dateTimeOffsetValue = default;
            DateTimeOffset nullableDateTimeOffsetValue = default;
            TimeSpan timeSpanValue = default;
            TimeSpan nullableTimeSpanValue = default;
            const DayOfWeek enumValue = default;
            const DayOfWeek nullableEnumValue = default;

            var document = new Document
            {
                Data = new SingleOrManyData<ResourceObject>(new ResourceObject
                {
                    Type = "resourceWithVariousDataTypes",
                    Attributes = new Dictionary<string, object>
                    {
                        ["boolean"] = booleanValue,
                        ["nullableBoolean"] = nullableBooleanValue,
                        ["char"] = charValue,
                        ["nullableChar"] = nullableCharValue,
                        ["unsignedLong"] = unsignedLongValue,
                        ["nullableUnsignedLong"] = nullableUnsignedLongValue,
                        ["decimal"] = decimalValue,
                        ["nullableDecimal"] = nullableDecimalValue,
                        ["float"] = floatValue,
                        ["nullableFloat"] = nullableFloatValue,
                        ["string"] = stringValue,
                        ["guid"] = guidValue,
                        ["nullableGuid"] = nullableGuidValue,
                        ["dateTime"] = dateTimeValue,
                        ["nullableDateTime"] = nullableDateTimeValue,
                        ["dateTimeOffset"] = dateTimeOffsetValue,
                        ["nullableDateTimeOffset"] = nullableDateTimeOffsetValue,
                        ["timeSpan"] = timeSpanValue,
                        ["nullableTimeSpan"] = nullableTimeSpanValue,
                        ["enum"] = enumValue,
                        ["nullableEnum"] = nullableEnumValue,
                        ["complexObject"] = null,
                        ["complexObjectList"] = null
                    }
                })
            };

            // Act
            var model = (ResourceWithVariousDataTypes)documentAdapter.Convert(document);

            // Assert
            model.Should().NotBeNull();

            model.Boolean.Should().Be(booleanValue);
            model.NullableBoolean.Should().Be(nullableBooleanValue);
            model.Char.Should().Be(charValue);
            model.NullableChar.Should().Be(nullableCharValue);
            model.UnsignedLong.Should().Be(unsignedLongValue);
            model.NullableUnsignedLong.Should().Be(nullableUnsignedLongValue);
            model.Decimal.Should().Be(decimalValue);
            model.NullableDecimal.Should().Be(nullableDecimalValue);
            model.Float.Should().Be(floatValue);
            model.NullableFloat.Should().Be(nullableFloatValue);
            model.String.Should().Be(stringValue);
            model.Guid.Should().Be(guidValue);
            model.NullableGuid.Should().Be(nullableGuidValue);
            model.DateTime.Should().Be(dateTimeValue);
            model.NullableDateTime.Should().Be(nullableDateTimeValue);
            model.DateTimeOffset.Should().Be(dateTimeOffsetValue);
            model.NullableDateTimeOffset.Should().Be(nullableDateTimeOffsetValue);
            model.TimeSpan.Should().Be(timeSpanValue);
            model.NullableTimeSpan.Should().Be(nullableTimeSpanValue);
            model.Enum.Should().Be(enumValue);
            model.NullableEnum.Should().Be(nullableEnumValue);
            model.ComplexObject.Should().BeNull();
            model.ComplexObjectList.Should().BeNull();
        }

        private static DocumentAdapter CreateDocumentAdapter<TResource>(Func<IResourceGraph, JsonApiRequest> createRequest)
            where TResource : Identifiable
        {
            var options = new JsonApiOptions();
            IResourceGraph resourceGraph = new ResourceGraphBuilder(options, NullLoggerFactory.Instance).Add<TResource>().Build();
            options.SerializerOptions.Converters.Add(new ResourceObjectConverter(resourceGraph));

            var serviceContainer = new ServiceContainer();
            var resourceFactory = new ResourceFactory(serviceContainer);
            var resourceDefinitionAccessor = new ResourceDefinitionAccessor(resourceGraph, serviceContainer);

            serviceContainer.AddService(typeof(IResourceDefinitionAccessor), resourceDefinitionAccessor);
            serviceContainer.AddService(typeof(IResourceDefinition<TResource>), new JsonApiResourceDefinition<TResource>(resourceGraph));

            JsonApiRequest request = createRequest(resourceGraph);
            var targetedFields = new TargetedFields();

            var resourceIdentifierObjectAdapter = new ResourceIdentifierObjectAdapter(resourceGraph, resourceFactory);
            var relationshipDataAdapter = new RelationshipDataAdapter(resourceGraph, resourceIdentifierObjectAdapter);
            var resourceObjectAdapter = new ResourceObjectAdapter(resourceGraph, resourceFactory, options, relationshipDataAdapter);
            var resourceDataAdapter = new ResourceDataAdapter(resourceDefinitionAccessor, resourceObjectAdapter);

            var atomicReferenceAdapter = new AtomicReferenceAdapter(resourceGraph, resourceFactory);
            var atomicOperationResourceDataAdapter = new ResourceDataInOperationsRequestAdapter(resourceDefinitionAccessor, resourceObjectAdapter);

            var atomicOperationObjectAdapter = new AtomicOperationObjectAdapter(resourceGraph, options, atomicReferenceAdapter,
                atomicOperationResourceDataAdapter, relationshipDataAdapter);

            var resourceDocumentAdapter = new DocumentInResourceOrRelationshipRequestAdapter(options, resourceDataAdapter, relationshipDataAdapter);
            var operationsDocumentAdapter = new DocumentInOperationsRequestAdapter(options, atomicOperationObjectAdapter);

            return new DocumentAdapter(request, targetedFields, resourceDocumentAdapter, operationsDocumentAdapter);
        }

        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        public sealed class ResourceWithVariousDataTypes : Identifiable
        {
            [Attr]
            public bool Boolean { get; set; }

            [Attr]
            public bool? NullableBoolean { get; set; }

            [Attr]
            public char Char { get; set; }

            [Attr]
            public char? NullableChar { get; set; }

            [Attr]
            public ulong UnsignedLong { get; set; }

            [Attr]
            public ulong? NullableUnsignedLong { get; set; }

            [Attr]
            public decimal Decimal { get; set; }

            [Attr]
            public decimal? NullableDecimal { get; set; }

            [Attr]
            public float Float { get; set; }

            [Attr]
            public float? NullableFloat { get; set; }

            [Attr]
            public string String { get; set; }

            [Attr]
            public Guid Guid { get; set; }

            [Attr]
            public Guid? NullableGuid { get; set; }

            [Attr]
            public DateTime DateTime { get; set; }

            [Attr]
            public DateTime? NullableDateTime { get; set; }

            [Attr]
            public DateTimeOffset DateTimeOffset { get; set; }

            [Attr]
            public DateTimeOffset? NullableDateTimeOffset { get; set; }

            [Attr]
            public TimeSpan TimeSpan { get; set; }

            [Attr]
            public TimeSpan? NullableTimeSpan { get; set; }

            [Attr]
            public DayOfWeek Enum { get; set; }

            [Attr]
            public DayOfWeek? NullableEnum { get; set; }

            [Attr]
            public ComplexObject ComplexObject { get; set; }

            [Attr]
            public IList<ComplexObject> ComplexObjectList { get; set; }
        }

        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        public sealed class ComplexObject
        {
            public string Value { get; set; }
        }
    }
}
