namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

public sealed class SparseFieldSetEvaluatorHitCounter
{
    internal List<string> Calls { get; } = [];

    internal List<string> QueryResults { get; } = [];

    internal void Reset()
    {
        QueryResults.Clear();
        Calls.Clear();
    }
}
