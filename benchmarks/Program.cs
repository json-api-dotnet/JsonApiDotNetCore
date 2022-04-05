using BenchmarkDotNet.Running;
using Benchmarks.ResourceConstruction;

namespace Benchmarks;

internal static class Program
{
    private static void Main(string[] args)
    {
        /*
        var b1 = new ActivatorBenchmarks();

        for (int index = 0; index < 10_000; index++)
        //while (true)
        {
            b1.Activator_CreateParameterizedInstance();
        }
        */

        /*
        var b1 = new ActivatorBenchmarks();
        b1.Activator_CreateInstance();
        b1.Activator_CreateParameterizedInstance();

        var b2 = new ExpressionBenchmarks();
        b2.Expression_CreateInstance();
        b2.Expression_CreateParameterizedInstance();

        var b3 = new CachingExpressionBenchmarks();
        b3.CachingExpression_CreateInstance();
        b3.CachingExpression_CreateParameterizedInstance();
        */

        var switcher = new BenchmarkSwitcher(new[]
        {
            typeof(CtorBenchmarks),
            //typeof(ActivatorBenchmarks),
            //typeof(ExpressionBenchmarks),
            //typeof(CachingExpressionBenchmarks)

            //typeof(ResourceDeserializationBenchmarks),
            //typeof(OperationsDeserializationBenchmarks),
            //typeof(ResourceSerializationBenchmarks),
            //typeof(OperationsSerializationBenchmarks),
            //typeof(QueryStringParserBenchmarks)
        });

        switcher.Run(args);
    }
}
