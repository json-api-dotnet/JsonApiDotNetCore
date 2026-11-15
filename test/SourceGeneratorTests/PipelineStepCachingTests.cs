using System.Collections.Immutable;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.SourceGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

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

        MarkedSourceCode source = new SourceCodeBuilder()
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
            .WithSourceCode(source.Text)
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
    public void Pragma_suppression_is_applied_after_incremental_source_change()
    {
        // Arrange
        var generator = new ControllerSourceGenerator();

        // @formatter:wrap_chained_method_calls chop_always
        // @formatter:wrap_before_first_method_call true

        MarkedSourceCode source = new SourceCodeBuilder()
            .WithNamespaceImportFor(typeof(ResourceAttribute))
            .InNamespace("ExampleApi.Models")
            .WithCode("""
                [Resource]
                #pragma warning disable CS999999
                public sealed class [|Item|]
                #pragma warning restore CS999999
                {
                    [Attr]
                    public int Value { get; set; }
                }
                """)
            .Build();

        Compilation inputCompilation = new CompilationBuilder()
            .WithDefaultReferences()
            .WithSourceCode(source.Text)
            .Build();

        // @formatter:wrap_before_first_method_call restore
        // @formatter:wrap_chained_method_calls restore

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out _, out _, TestContext.Current.CancellationToken);

        SyntaxTree originalTree = inputCompilation.SyntaxTrees.Single();
        string updatedSource = source.Text.Replace("CS999999", "JADNC001", StringComparison.Ordinal);
        SyntaxTree updatedTree = originalTree.WithChangedText(SourceText.From(updatedSource));
        Compilation updatedCompilation = inputCompilation.ReplaceSyntaxTree(originalTree, updatedTree);

        // Act
        driver = driver.RunGeneratorsAndUpdateCompilation(updatedCompilation, out Compilation outputCompilation, out _, TestContext.Current.CancellationToken);

        // Assert
        inputCompilation.GetDiagnostics(TestContext.Current.CancellationToken).Should().BeEmpty();
        outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken).Should().BeEmpty();

        GeneratorDriverRunResult runResult = driver.GetRunResult();

        runResult.Should().HaveSingleDiagnostic(source.DiagnosticSpan,
            "Type 'Item' must implement IIdentifiable<TId> when using ResourceAttribute to auto-generate ASP.NET controllers", true);

        runResult.Should().NotHaveProducedSourceCode();
    }
}
