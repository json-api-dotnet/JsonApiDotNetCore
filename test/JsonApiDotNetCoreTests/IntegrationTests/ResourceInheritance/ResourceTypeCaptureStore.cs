// ReSharper disable UnusedTypeParameter

using FluentAssertions;
using JsonApiDotNetCore.Middleware;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance;

public sealed class ResourceTypeCaptureStore<TResource, TId>
{
    internal Type? LeftDeclaredType { get; set; }
    internal string? LeftReflectedTypeName { get; set; }
    internal ISet<string> RightTypeNames { get; } = new HashSet<string>();
    internal IJsonApiRequest? Request { get; set; }

    internal void Reset()
    {
        LeftDeclaredType = null;
        LeftReflectedTypeName = null;
        RightTypeNames.Clear();
        Request = null;
    }

    internal void AssertLeftType<TLeft>()
    {
        LeftDeclaredType.Should().Be(typeof(TLeft));
        LeftReflectedTypeName.Should().Be(typeof(TLeft).Name);

        Request.ShouldNotBeNull();
        Request.PrimaryResourceType.ShouldNotBeNull();
        Request.PrimaryResourceType.ClrType.Should().Be(typeof(TLeft));
        Request.Relationship?.LeftType.ClrType.Should().Be(typeof(TLeft));
    }

    internal void AssertRightTypes(params Type[] types)
    {
        RightTypeNames.ShouldHaveCount(types.Length);

        foreach (Type type in types)
        {
            RightTypeNames.Should().ContainSingle(name => name == type.Name);
        }
    }
}
