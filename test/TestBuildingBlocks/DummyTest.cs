using FluentAssertions;
using Xunit;

namespace TestBuildingBlocks;

public sealed class DummyTest
{
    [Fact]
    public void Empty()
    {
        // This dummy test exists solely to suppress the warning
        // during test runs that no tests were found in this project.
    }

    [Fact(Skip = "Example test that is skipped.")]
    public void SkipAlways()
    {
        true.Should().BeFalse();
    }
}
