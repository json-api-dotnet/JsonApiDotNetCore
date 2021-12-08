using FluentAssertions;
using JsonApiDotNetCore.Resources;
using Xunit;

namespace UnitTests.Models;

public sealed class IdentifiableTests
{
    [Fact]
    public void Can_Set_StringId_To_Value_Type()
    {
        var resource = new IntId
        {
            StringId = "1"
        };

        resource.Id.Should().Be(1);
    }

    [Fact]
    public void Setting_StringId_To_Null_Sets_Id_As_Default()
    {
        var resource = new IntId
        {
            StringId = null
        };

        resource.Id.Should().Be(0);
    }

    [Fact]
    public void GetStringId_Returns_Null_If_Object_Is_Default()
    {
        var resource = new IntId();

        string? stringId = resource.ExposedGetStringId(default);

        stringId.Should().BeNull();
    }

    private sealed class IntId : Identifiable<int>
    {
        public string? ExposedGetStringId(int value)
        {
            return GetStringId(value);
        }
    }
}
