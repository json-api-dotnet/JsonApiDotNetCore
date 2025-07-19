using System.Text.Json;
using BlushingPenguin.JsonPath;
using FluentAssertions;
using FluentAssertions.Execution;
using JetBrains.Annotations;

namespace TestBuildingBlocks;

public static class FluentJsonElementExtensions
{
    private const string ComponentSchemaPrefix = "#/components/schemas/";

    public static JsonElementAssertions Should(this JsonElement source)
    {
        return new JsonElementAssertions(source);
    }

    [CustomAssertion]
    public static SchemaReferenceIdContainer ShouldBeSchemaReferenceId(this JsonElement source, string value)
    {
        string schemaReferenceId = GetSchemaReferenceId(source);
        schemaReferenceId.Should().Be(value);

        return new SchemaReferenceIdContainer(value);
    }

    [CustomAssertion]
    public static string GetSchemaReferenceId(this JsonElement source)
    {
        source.ValueKind.Should().Be(JsonValueKind.String);

        string? jsonElementValue = source.GetString();
        jsonElementValue.Should().StartWith(ComponentSchemaPrefix);

        return jsonElementValue[ComponentSchemaPrefix.Length..];
    }

    [CustomAssertion]
    public static void WithSchemaReferenceId(this JsonElement subject, [InstantHandle] Action<string> continuation)
    {
        string schemaReferenceId = GetSchemaReferenceId(subject);

        continuation(schemaReferenceId);
    }

    public sealed class SchemaReferenceIdContainer
    {
        public string SchemaReferenceId { get; }

        internal SchemaReferenceIdContainer(string schemaReferenceId)
        {
            SchemaReferenceId = schemaReferenceId;
        }
    }

    public sealed class JsonElementAssertions : JsonElementAssertions<JsonElementAssertions>
    {
        internal JsonElementAssertions(JsonElement subject)
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

        public JsonElement ContainPath(string jsonPath)
        {
            Func<JsonElement> elementSelector = () => _subject.SelectToken(jsonPath, true)!.Value;
            return elementSelector.Should().NotThrow().Subject;
        }

        public void NotContainPath(string jsonPath)
        {
            JsonElement? pathToken = _subject.SelectToken(jsonPath);
            pathToken.Should().BeNull();
        }

        public void Be(object? value)
        {
            if (value == null)
            {
                _subject.ValueKind.Should().Be(JsonValueKind.Null);
            }
            else if (value is bool boolValue)
            {
                _subject.ValueKind.Should().Be(boolValue ? JsonValueKind.True : JsonValueKind.False);
            }
            else if (value is int intValue)
            {
                _subject.ValueKind.Should().Be(JsonValueKind.Number);
                _subject.GetInt32().Should().Be(intValue);
            }
            else if (value is double doubleValue)
            {
                _subject.ValueKind.Should().Be(JsonValueKind.Number);
                _subject.GetDouble().Should().Be(doubleValue);
            }
            else if (value is string stringValue)
            {
                _subject.ValueKind.Should().Be(JsonValueKind.String);
                _subject.GetString().Should().Be(stringValue);
            }
            else
            {
                throw new NotSupportedException($"Unknown object of type '{value.GetType()}'.");
            }
        }

        public void HaveProperty(string jsonPath, object? propertyValue)
        {
            _subject.Should().ContainPath(jsonPath).With(element => element.Should().Be(propertyValue));
        }

        public void ContainArrayElement(string value)
        {
            _subject.ValueKind.Should().Be(JsonValueKind.Array);

            string?[] stringValues = _subject.EnumerateArray().Where(element => element.ValueKind == JsonValueKind.String)
                .Select(element => element.GetString()).ToArray();

            stringValues.Should().Contain(value);
        }

        public void NotContainArrayElement(string value)
        {
            _subject.ValueKind.Should().Be(JsonValueKind.Array);

            string?[] stringValues = _subject.EnumerateArray().Where(element => element.ValueKind == JsonValueKind.String)
                .Select(element => element.GetString()).ToArray();

            stringValues.Should().NotContain(value);
        }

        public void BeJson(string json)
        {
            _subject.ToString().Should().BeJson(json);
        }
    }
}
