using FluentAssertions;
using FluentAssertions.Primitives;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SourceGeneratorTests
{
    internal static class GeneratorDriverRunResultExtensions
    {
        public static GeneratorDriverRunResultAssertions Should(this GeneratorDriverRunResult instance)
        {
            return new GeneratorDriverRunResultAssertions(instance);
        }

        internal sealed class GeneratorDriverRunResultAssertions : ReferenceTypeAssertions<GeneratorDriverRunResult, GeneratorDriverRunResultAssertions>
        {
            protected override string Identifier => nameof(GeneratorDriverRunResult);

            public GeneratorDriverRunResultAssertions(GeneratorDriverRunResult subject)
                : base(subject)
            {
            }

            public void NotHaveDiagnostics()
            {
                Subject.Diagnostics.Should().BeEmpty();
            }

            public void HaveSingleDiagnostic(string message)
            {
                Subject.Diagnostics.Should().HaveCount(1);
                Subject.Diagnostics[0].ToString().Should().Be(message);
            }

            public void NotHaveProducedSourceCode()
            {
                Subject.Results.Should().HaveCount(1);

                GeneratorRunResult generatorResult = Subject.Results[0];
                generatorResult.GeneratedSources.Should().BeEmpty();
            }

            public void HaveProducedSourceCode(string expectedCode)
            {
                string generatedSourceText = GetGeneratedSourceText();

                string? generatedSourceTextNormalized = NormalizeLineEndings(generatedSourceText);
                string? expectedTextNormalized = NormalizeLineEndings(expectedCode);

                generatedSourceTextNormalized.Should().Be(expectedTextNormalized);
            }

            public void HaveProducedSourceCodeContaining(string expectedText)
            {
                string generatedSourceText = GetGeneratedSourceText();

                generatedSourceText.Should().Contain(expectedText);
            }

            private string GetGeneratedSourceText()
            {
                Subject.Results.Should().HaveCount(1);

                GeneratorRunResult generatorResult = Subject.Results[0];
                generatorResult.GeneratedSources.Should().HaveCount(1);

                SourceText generatedSource = generatorResult.GeneratedSources[0].SourceText;
                return generatedSource.ToString();
            }

            private static string? NormalizeLineEndings(string? text)
            {
                return text?.Replace("\r\n", "\n").Replace("\r", "\n");
            }
        }
    }
}
