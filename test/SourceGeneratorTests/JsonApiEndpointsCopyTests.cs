using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.SourceGenerators;
using Xunit;

namespace SourceGeneratorTests
{
    public sealed class JsonApiEndpointsCopyTests
    {
        [Fact]
        public void Enum_underlying_types_are_identical()
        {
            Type sourceType = Enum.GetUnderlyingType(typeof(JsonApiEndpoints));
            Type copyType = Enum.GetUnderlyingType(typeof(JsonApiEndpointsCopy));

            copyType.Should().Be(sourceType);
        }

        [Fact]
        public void Enum_attributes_in_order_are_identical()
        {
            Attribute[] sourceAttributes = typeof(JsonApiEndpoints).GetCustomAttributes().ToArray();
            Attribute[] copyAttributes = typeof(JsonApiEndpointsCopy).GetCustomAttributes().ToArray();

            copyAttributes.Should().BeEquivalentTo(sourceAttributes, options => options.WithStrictOrdering());
        }

        [Fact]
        public void Enum_member_names_in_order_are_identical()
        {
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;

            string[] sourceNames = typeof(JsonApiEndpoints).GetMembers(bindingFlags).Select(memberInfo => memberInfo.Name).ToArray();
            string[] copyNames = typeof(JsonApiEndpointsCopy).GetMembers(bindingFlags).Select(memberInfo => memberInfo.Name).ToArray();

            copyNames.Should().BeEquivalentTo(sourceNames, options => options.WithStrictOrdering());
        }

        [Fact]
        public void Enum_member_values_are_identical()
        {
            IEnumerable<int> sourceValues = Enum.GetValues<JsonApiEndpoints>().Select(value => (int)value).ToArray();
            int[] copyValues = Enum.GetValues<JsonApiEndpointsCopy>().Select(value => (int)value).ToArray();

            copyValues.Should().BeEquivalentTo(sourceValues, options => options.WithStrictOrdering());
        }
    }
}
