using System.Text.Json;
using BlushingPenguin.JsonPath;
using FluentAssertions;
using FluentAssertions.Execution;
using TestBuildingBlocks;

namespace OpenApiTests;

internal static class JsonElementExtensions
{
    public static JsonElementAssertions Should(this JsonElement source)
    {
        return new JsonElementAssertions(source);
    }

    public static JsonElement ShouldContainPath(this JsonElement source, string path)
    {
        Func<JsonElement> elementSelector = () => source.SelectToken(path, true)!.Value;
        return elementSelector.Should().NotThrow().Subject;
    }

    public static void ShouldBeString(this JsonElement source, string value)
    {
        source.ValueKind.Should().Be(JsonValueKind.String);
        source.GetString().Should().Be(value);
    }

    public static ReferenceSchemaIdContainer ShouldBeReferenceSchemaId(this JsonElement source, string value)
    {
        source.ValueKind.Should().Be(JsonValueKind.String);

        string? jsonElementValue = source.GetString();
        jsonElementValue.ShouldNotBeNull();

        string referenceSchemaId = jsonElementValue.Split('/').Last();
        referenceSchemaId.Should().Be(value);

        return new ReferenceSchemaIdContainer
        {
            ReferenceSchemaId = value
        };
    }

    public sealed class ReferenceSchemaIdContainer
    {
        internal string ReferenceSchemaId { get; init; } = null!;
    }

    public sealed class JsonElementAssertions : JsonElementAssertions<JsonElementAssertions>
    {
        public JsonElementAssertions(JsonElement subject)
            : base(subject)
        {
        }
    }

    public class JsonElementAssertions<TAssertions>
        where TAssertions : JsonElementAssertions<TAssertions>
    {
        private readonly JsonElement _subject;

        protected JsonElementAssertions(JsonElement subject)
        {
            _subject = subject;
        }

        public void ContainProperty(string propertyName)
        {
            string json = _subject.ToString();

            string escapedJson = json.Replace("{", "{{").Replace("}", "}}");

            Execute.Assertion.ForCondition(_subject.TryGetProperty(propertyName, out _))
                .FailWith($"Expected JSON element '{escapedJson}' to contain a property named '{propertyName}'.");
        }

        public void BeJson(string expectedDocument)
        {
            string json = _subject.ToString();

            json.Should().BeJson(expectedDocument);
        }
    }
}
