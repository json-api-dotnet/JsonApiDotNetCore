﻿using System.Text.Json;
using BlushingPenguin.JsonPath;
using FluentAssertions;
using FluentAssertions.Execution;
using JetBrains.Annotations;

namespace TestBuildingBlocks;

public static class JsonElementExtensions
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

    public static void ShouldNotContainPath(this JsonElement source, string path)
    {
        JsonElement? pathToken = source.SelectToken(path);

        pathToken.Should().BeNull();
    }

    public static void ShouldBeString(this JsonElement source, string value)
    {
        source.ValueKind.Should().Be(JsonValueKind.String);
        source.GetString().Should().Be(value);
    }

    public static void ShouldBeArrayWithElement(this JsonElement source, string value)
    {
        source.ValueKind.Should().Be(JsonValueKind.Array);

        var deserializedCollection = JsonSerializer.Deserialize<List<string>>(source.GetRawText());
        deserializedCollection.Should().Contain(value);
    }

    public static void ShouldBeArrayWithoutElement(this JsonElement source, string value)
    {
        source.ValueKind.Should().Be(JsonValueKind.Array);

        var deserializedCollection = JsonSerializer.Deserialize<List<string>>(source.GetRawText());
        deserializedCollection.Should().NotContain(value);
    }

    public static void ShouldBeInteger(this JsonElement source, int value)
    {
        source.ValueKind.Should().Be(JsonValueKind.Number);
        source.GetInt32().Should().Be(value);
    }

    public static SchemaReferenceIdContainer ShouldBeSchemaReferenceId(this JsonElement source, string value)
    {
        string schemaReferenceId = GetSchemaReferenceId(source);
        schemaReferenceId.Should().Be(value);

        return new SchemaReferenceIdContainer(value);
    }

    private static string GetSchemaReferenceId(this JsonElement source)
    {
        source.ValueKind.Should().Be(JsonValueKind.String);

        string? jsonElementValue = source.GetString();
        jsonElementValue.ShouldNotBeNull();

        string schemaReferenceId = jsonElementValue.Split('/').Last();
        return schemaReferenceId;
    }

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
    }
}
