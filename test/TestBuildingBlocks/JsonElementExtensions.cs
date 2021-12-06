using System;
using System.Linq;
using System.Text.Json;
using BlushingPenguin.JsonPath;
using FluentAssertions;
using FluentAssertions.Execution;

namespace TestBuildingBlocks
{
    public static class JsonElementExtensions
    {
        public static JsonElementAssertions Should(this JsonElement source)
        {
            return new JsonElementAssertions(source);
        }

        public static JsonElement ShouldContainPath(this JsonElement source, string path)
        {
            JsonElement value = default;

            Action action = () => value = source.SelectToken(path, true)!.Value;
            action.Should().NotThrow();

            return value;
        }

        public static void ShouldBeString(this JsonElement source, string value)
        {
            source.ValueKind.Should().Be(JsonValueKind.String);
            source.GetString().Should().Be(value);
        }

        public static ReferenceSchemaIdAssertion ShouldBeReferenceSchemaId(this JsonElement source, string value)
        {
            source.ValueKind.Should().Be(JsonValueKind.String);

            string jsonElementValue = source.GetString();
            jsonElementValue.Should().NotBeNull();

            string referenceSchemaId = jsonElementValue!.Split('/').Last();
            referenceSchemaId.Should().Be(value);

            return new ReferenceSchemaIdAssertion
            {
                SchemaReferenceId = value
            };
        }

        public sealed class ReferenceSchemaIdAssertion
        {
            public string SchemaReferenceId { get; internal init; }
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
                string escapedJson = _subject.ToString()?.Replace("{", "{{").Replace("}", "}}");

                Execute.Assertion.ForCondition(_subject.TryGetProperty(propertyName, out _))
                    .FailWith($"Expected JSON element '{escapedJson}' to contain a property named '{propertyName}'.");
            }
        }
    }
}
