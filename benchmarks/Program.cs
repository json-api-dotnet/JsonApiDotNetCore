using BenchmarkDotNet.Running;
using Benchmarks.Deserialization;
using Benchmarks.QueryString;
using Benchmarks.Serialization;

var switcher = new BenchmarkSwitcher([
    typeof(ResourceDeserializationBenchmarks),
    typeof(OperationsDeserializationBenchmarks),
    typeof(ResourceSerializationBenchmarks),
    typeof(OperationsSerializationBenchmarks),
    typeof(QueryStringParserBenchmarks)
]);

switcher.Run(args);
