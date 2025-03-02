// ReSharper disable UnusedTypeParameter

using FluentAssertions;
using JsonApiDotNetCore.Middleware;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance;

public sealed class ResourceTypeCaptureStore<TResource, TId>
{
    internal Type? LeftDeclaredType { get; set; }
    internal string? LeftReflectedTypeName { get; set; }
    internal HashSet<string> RightTypeNames { get; } = [];
    internal JsonApiRequest? Request { get; set; }

    internal void Reset()
    {
        LeftDeclaredType = null;
        LeftReflectedTypeName = null;
        RightTypeNames.Clear();
        Request = null;
    }

    internal void AssertLeftType<TLeft>()
    {
        LeftDeclaredType.Should().Be<TLeft>();
        LeftReflectedTypeName.Should().Be(typeof(TLeft).Name);

        Request.Should().NotBeNull();
        Request.PrimaryResourceType.Should().NotBeNull();
        Request.PrimaryResourceType.ClrType.Should().Be<TLeft>();
        Request.Relationship?.LeftType.ClrType.Should().Be<TLeft>();
    }

    internal void AssertRightTypes(params Type[] types)
    {
        RightTypeNames.Should().HaveCount(types.Length);

        foreach (Type type in types)
        {
            RightTypeNames.Should().ContainSingle(name => name == type.Name);
        }
    }
}
