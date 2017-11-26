``` ini

BenchmarkDotNet=v0.10.10, OS=Mac OS X 10.12
Processor=Intel Core i5-5257U CPU 2.70GHz (Broadwell), ProcessorCount=4
.NET Core SDK=2.0.0
  [Host]     : .NET Core 1.1.4 (Framework 4.6.25714.03), 64bit RyuJIT
  Job-OWXJBF : .NET Core 1.1.4 (Framework 4.6.25714.03), 64bit RyuJIT

LaunchCount=3  TargetCount=20  WarmupCount=10  

```
|         Method |         Mean |      Error |      StdDev |       Median |
|--------------- |-------------:|-----------:|------------:|-------------:|
|  AscendingSort |     4.451 us |  1.5230 us |   3.2457 us |     3.305 us |
| DescendingSort |     3.287 us |  0.0307 us |   0.0673 us |     3.263 us |
|   ComplexQuery | 1,973.029 us | 67.5600 us | 143.9759 us | 1,952.663 us |
