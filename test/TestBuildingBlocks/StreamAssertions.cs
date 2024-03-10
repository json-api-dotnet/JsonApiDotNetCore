using FluentAssertions;
using FluentAssertions.Streams;

namespace TestBuildingBlocks;

public static class StreamAssertionsExtensions
{
    /// <summary>
    /// Asserts that the current <see cref="Stream" /> is empty.
    /// </summary>
    /// <param name="parent">
    /// The assertion for the <see cref="Stream" /> to inspect.
    /// </param>
    /// <param name="because">
    /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion is needed. If the phrase does not
    /// start with the word <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    [CustomAssertion]
    public static void BeNullOrEmpty<TSubject, TAssertions>(this StreamAssertions<TSubject, TAssertions> parent, string because = "",
        params object[] becauseArgs)
        where TSubject : Stream
        where TAssertions : StreamAssertions<TSubject, TAssertions>
    {
        if (parent.Subject != null)
        {
            using var reader = new StreamReader(parent.Subject);
            string content = reader.ReadToEnd();

            content.Should().BeEmpty(because, becauseArgs);
        }
    }
}
