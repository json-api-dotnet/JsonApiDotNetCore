using FluentAssertions;
using JetBrains.Annotations;
using SysNotNull = System.Diagnostics.CodeAnalysis.NotNullAttribute;

// ReSharper disable PossibleMultipleEnumeration
#pragma warning disable CS8777 // Parameter must have a non-null value when exiting.

namespace TestBuildingBlocks;

public static class NullabilityAssertionExtensions
{
    [CustomAssertion]
    public static T ShouldNotBeNull<T>([SysNotNull] this T? subject)
    {
        subject.Should().NotBeNull();
        return subject;
    }

    public static void With<T>(this T subject, [InstantHandle] Action<T> continuation)
    {
        continuation(subject);
    }
}
