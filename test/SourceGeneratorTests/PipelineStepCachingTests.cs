using System.Collections.Immutable;
using FluentAssertions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.SourceGenerators;
using Microsoft.CodeAnalysis;
using Xunit;

namespace SourceGeneratorTests;

public sealed class PipelineStepCachingTests
{
    private static readonly string[] AllTrackingNames = typeof(TrackingNames).GetFields()
        .Where(field => field is { IsLiteral: true, IsInitOnly: false } && field.FieldType == typeof(string))
        .Select(field => (string)field.GetRawConstantValue()!).Where(value => !string.IsNullOrEmpty(value)).ToArray();

    [Fact]
    public void Pipeline_outputs_are_cached_on_generate()
    {
        // Arrange
        var generator = new ControllerSourceGenerator();

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:wrap_before_first_method_call true

        string source = new SourceCodeBuilder()
            .WithNamespaceImportFor(typeof(IIdentifiable))
            .WithNamespaceImportFor(typeof(ResourceAttribute))
            .InNamespace("ExampleApi.Models")
            .WithCode("""
                [Resource]
                public sealed class Item : Identifiable<long>
                {
                    [Attr]
                    public int Value { get; set; }
                }
                """)
            .Build();

        Compilation inputCompilation = new CompilationBuilder()
            .WithDefaultReferences()
            .WithSourceCode(source)
            .Build();

        // @formatter:wrap_before_first_method_call restore
        // @formatter:wrap_chained_method_calls restore

        // Act
        (ImmutableArray<Diagnostic> diagnostics, string[] output) = inputCompilation.AssertOutputsAreCached(generator, AllTrackingNames);

        // Assert
        diagnostics.Should().BeEmpty();
        output.Should().NotBeEmpty();
    }

    [Fact]
    public void Pipeline_outputs_are_cached_on_diagnostic()
    {
        // Arrange
        var generator = new ControllerSourceGenerator();

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:wrap_before_first_method_call true

        string source = new SourceCodeBuilder()
            .WithNamespaceImportFor(typeof(ResourceAttribute))
            .InNamespace("ExampleApi.Models")
            .WithCode("""
                [Resource]
                public sealed class Item
                {
                    [Attr]
                    public int Value { get; set; }
                }
                """)
            .Build();

        Compilation inputCompilation = new CompilationBuilder()
            .WithDefaultReferences()
            .WithSourceCode(source)
            .Build();

        // @formatter:wrap_before_first_method_call restore
        // @formatter:wrap_chained_method_calls restore

        // Act
        (ImmutableArray<Diagnostic> diagnostics, string[] output) = inputCompilation.AssertOutputsAreCached(generator, AllTrackingNames);

        // Assert
        diagnostics.Should().NotBeEmpty();
        output.Should().BeEmpty();
    }
}
