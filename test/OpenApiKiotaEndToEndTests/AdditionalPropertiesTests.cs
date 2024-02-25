using System.Reflection;
using FluentAssertions;
using Xunit;

namespace OpenApiKiotaEndToEndTests;

public sealed class AdditionalPropertiesTests
{
    private static readonly string GeneratedCodeDirectory = $"{Path.PathSeparator}GeneratedCode{Path.PathSeparator}";

    [Fact]
    public async Task Additional_properties_are_only_allowed_in_meta()
    {
        string startDirectory = Path.GetFullPath(Path.Combine(Assembly.GetExecutingAssembly().Location, "../../../../"));

        foreach (string path in Directory.EnumerateFiles(startDirectory, "*.cs", new EnumerationOptions
        {
            MatchType = MatchType.Simple,
            RecurseSubdirectories = true
        }))
        {
            if (path.Contains(GeneratedCodeDirectory, StringComparison.OrdinalIgnoreCase) && !path.EndsWith("_meta.cs", StringComparison.OrdinalIgnoreCase))
            {
                string content = await File.ReadAllTextAsync(path);
                bool containsAdditionalData = content.Contains("public IDictionary<string, object> AdditionalData");

                containsAdditionalData.Should().BeFalse($"file '{path}' should not contain AdditionalData");
            }
        }
    }
}
