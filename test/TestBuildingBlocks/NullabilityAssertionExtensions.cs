using FluentAssertions;
using JetBrains.Annotations;
using SysNotNull = System.Diagnostics.CodeAnalysis.NotNullAttribute;

// ReSharper disable PossibleMultipleEnumeration
#pragma warning disable CS8777 // Parameter must have a non-null value when exiting.

namespace TestBuildingBlocks
{
    public static class NullabilityAssertionExtensions
    {
        [CustomAssertion]
        public static T ShouldNotBeNull<T>([SysNotNull] this T? subject)
        {
            subject.Should().NotBeNull();
            return subject!;
        }

        [CustomAssertion]
        public static void ShouldNotBeEmpty([SysNotNull] this string? subject)
        {
            subject.Should().NotBeEmpty();
        }

        [CustomAssertion]
        public static void ShouldNotBeEmpty<T>([SysNotNull] this IEnumerable<T>? subject)
        {
            subject.Should().NotBeEmpty();
        }

        [CustomAssertion]
        public static void ShouldNotBeNullOrEmpty([SysNotNull] this string? subject)
        {
            subject.Should().NotBeNullOrEmpty();
        }

        [CustomAssertion]
        public static void ShouldHaveCount<T>([SysNotNull] this IEnumerable<T>? subject, int expected)
        {
            subject.Should().HaveCount(expected);
        }

        [CustomAssertion]
        public static TValue? ShouldContainKey<TKey, TValue>([SysNotNull] this IDictionary<TKey, TValue?>? subject, TKey expected)
        {
            subject.Should().ContainKey(expected);

            return subject![expected];
        }

        public static void With<T>(this T subject, [InstantHandle] Action<T> continuation)
        {
            continuation(subject);
        }
    }
}
