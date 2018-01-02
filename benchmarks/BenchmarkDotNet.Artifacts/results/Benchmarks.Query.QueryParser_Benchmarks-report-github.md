``` ini

BenchmarkDotNet=v0.10.10, OS=Mac OS X 10.12
Processor=Intel Core i5-5257U CPU 2.70GHz (Broadwell), ProcessorCount=4
.NET Core SDK=2.0.0
  [Host]     : .NET Core 1.1.4 (Framework 4.6.25714.03), 64bit RyuJIT
  Job-WKDOLS : .NET Core 1.1.4 (Framework 4.6.25714.03), 64bit RyuJIT

LaunchCount=3  TargetCount=20  WarmupCount=10  

```
|         Method |         Mean |      Error |     StdDev |    Gen 0 |   Gen 1 | Allocated |
|--------------- |-------------:|-----------:|-----------:|---------:|--------:|----------:|
|  AscendingSort |     4.316 us |  1.3773 us |  3.0232 us |   0.5066 |  0.1303 |   1.08 KB |
| DescendingSort |     3.300 us |  0.0314 us |  0.0682 us |   0.5123 |  0.1318 |   1.13 KB |
|   ComplexQuery | 2,041.642 us | 41.5631 us | 92.1010 us | 312.5000 | 80.2734 | 648.99 KB |
