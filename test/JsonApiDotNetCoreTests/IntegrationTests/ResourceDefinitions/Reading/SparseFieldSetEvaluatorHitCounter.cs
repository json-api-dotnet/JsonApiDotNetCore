namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

public sealed class SparseFieldSetEvaluatorHitCounter
{
    internal List<string> Calls { get; } = [];

    internal void Reset()
    {
        Calls.Clear();
    }
}
