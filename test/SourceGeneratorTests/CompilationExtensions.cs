using System.Collections;
using System.Collections.Immutable;
using System.Reflection;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SourceGeneratorTests;

/// <summary>
/// Based on https://andrewlock.net/creating-a-source-generator-part-10-testing-your-incremental-generator-pipeline-outputs-are-cacheable/. Checks for
/// banned types and verifies that outputs of pipeline stages are cached. Well, it actually only tests the FIRST stage, but at least it's something.
/// </summary>
internal static class CompilationExtensions
{
    public static (ImmutableArray<Diagnostic> Diagnostics, string[] Output) AssertOutputsAreCached(this Compilation compilation,
        IIncrementalGenerator generator, string[] trackingNames)
    {
        ISourceGenerator sourceGenerator = generator.AsSourceGenerator();

        // Tell the driver to track all the incremental generator outputs.
        // Without this, you'll have no tracked outputs!
        var options = new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, true);

        GeneratorDriver driver = CSharpGeneratorDriver.Create([sourceGenerator], driverOptions: options);

        // Create a clone of the compilation that we will use later.
        Compilation clone = compilation.Clone();

        // Do the initial run. Note that we store the returned driver value, as it contains cached previous outputs.
        driver = driver.RunGenerators(compilation);
        GeneratorDriverRunResult runResult1 = driver.GetRunResult();

        // Run again, using the same driver, with a clone of the compilation.
        GeneratorDriverRunResult runResult2 = driver.RunGenerators(clone).GetRunResult();

        // Compare all the tracked outputs, throw if there's a failure.
        AssertRunsEqual(runResult1, runResult2, trackingNames);

        // Verify the second run only generated cached source outputs.
        runResult2.Results[0].TrackedOutputSteps.SelectMany(pair => pair.Value) // step executions
            .SelectMany(step => step.Outputs) // execution results
            .Should().OnlyContain(pair => pair.Reason == IncrementalStepRunReason.Cached);

        // Return the generator diagnostics and generated sources.
        return (runResult1.Diagnostics, runResult1.GeneratedTrees.Select(tree => tree.ToString()).ToArray());
    }

    private static void AssertRunsEqual(GeneratorDriverRunResult runResult1, GeneratorDriverRunResult runResult2, string[] trackingNames)
    {
        // We're given all the tracking names, but not all the stages will necessarily execute, so extract all the output steps, and filter to ones we know about.
        Dictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> trackedSteps1 = GetTrackedSteps(runResult1, trackingNames);
        Dictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> trackedSteps2 = GetTrackedSteps(runResult2, trackingNames);

        // Both runs should have the same tracked steps.
        trackedSteps1.Should().NotBeEmpty().And.HaveSameCount(trackedSteps2).And.ContainKeys(trackedSteps2.Keys);

        // Get the IncrementalGeneratorRunStep collection for each run.
        foreach ((string trackingName, ImmutableArray<IncrementalGeneratorRunStep> runSteps1) in trackedSteps1)
        {
            // Assert that both runs produced the same outputs.
            ImmutableArray<IncrementalGeneratorRunStep> runSteps2 = trackedSteps2[trackingName];
            AssertEqual(runSteps1, runSteps2, trackingName);
        }

        // Local function that extracts the tracked steps.
        static Dictionary<string, ImmutableArray<IncrementalGeneratorRunStep>> GetTrackedSteps(GeneratorDriverRunResult runResult, string[] trackingNames)
        {
            return runResult.Results[0] // we're only running a single generator, so this is safe
                .TrackedSteps // get the pipeline outputs
                .Where(step => trackingNames.Contains(step.Key)) // filter to known steps
                .ToDictionary(pair => pair.Key, pair => pair.Value); // convert to a dictionary
        }
    }

    private static void AssertEqual(ImmutableArray<IncrementalGeneratorRunStep> runSteps1, ImmutableArray<IncrementalGeneratorRunStep> runSteps2,
        string stepName)
    {
        runSteps1.Should().HaveSameCount(runSteps2);

        for (int index = 0; index < runSteps1.Length; index++)
        {
            IncrementalGeneratorRunStep runStep1 = runSteps1[index];
            IncrementalGeneratorRunStep runStep2 = runSteps2[index];

            // The outputs should be equal between different runs.
            IEnumerable<object> outputs1 = runStep1.Outputs.Select(pair => pair.Value);
            IEnumerable<object> outputs2 = runStep2.Outputs.Select(pair => pair.Value);

            outputs1.Should().Equal(outputs2, $"because {stepName} should produce cacheable outputs");

            // Therefore, on the second run the results should always be cached or unchanged!
            // - Unchanged is when the _input_ has changed, but the output hasn't.
            // - Cached is when the input has not changed, so the cached output is used.
            runStep2.Outputs.Should().OnlyContain(pair => pair.Reason == IncrementalStepRunReason.Cached || pair.Reason == IncrementalStepRunReason.Unchanged,
                $"{stepName} expected to have reason {IncrementalStepRunReason.Cached} or {IncrementalStepRunReason.Unchanged}");

            // Make sure we're not using anything we shouldn't.
            AssertObjectGraph(runStep1, stepName);
        }
    }

    private static void AssertObjectGraph(IncrementalGeneratorRunStep runStep, string stepName)
    {
        // Including the step name in error messages to make it easy to isolate issues.
        string because = $"{stepName} shouldn't contain banned types";
        var visited = new HashSet<object>();

        // Check all the outputs - probably overkill, but why not.
        foreach ((object obj, IncrementalStepRunReason _) in runStep.Outputs)
        {
            Visit(obj);
        }

        void Visit(object? node)
        {
            // If we've already seen this object, or it's null, stop.
            if (node is null || !visited.Add(node))
            {
                return;
            }

            // Make sure it's not a banned type.
            node.Should().NotBeAssignableTo<Compilation>(because).And.NotBeAssignableTo<ISymbol>(because).And.NotBeAssignableTo<SyntaxNode>(because);

            // Examine the object.
            Type type = node.GetType();

            if (type.IsPrimitive || type.IsEnum || type == typeof(string))
            {
                return;
            }

            // If the object is a collection, check each of the values.
            if (node is IEnumerable collection and not string)
            {
                foreach (object element in collection)
                {
                    // Recursively check each element in the collection.
                    Visit(element);
                }
            }
            else
            {
                // Recursively check each field in the object.
                foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    object? fieldValue = field.GetValue(node);
                    Visit(fieldValue);
                }
            }
        }
    }
}
