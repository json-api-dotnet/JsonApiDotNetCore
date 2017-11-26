``` ini

BenchmarkDotNet=v0.10.10, OS=Mac OS X 10.12
Processor=Intel Core i5-5257U CPU 2.70GHz (Broadwell), ProcessorCount=4
.NET Core SDK=2.0.0
  [Host]     : .NET Core 1.1.4 (Framework 4.6.25714.03), 64bit RyuJIT
  Job-ROPOBW : .NET Core 1.1.4 (Framework 4.6.25714.03), 64bit RyuJIT

LaunchCount=3  TargetCount=20  WarmupCount=10  

```
|         Method |     Mean |     Error |    StdDev |
|--------------- |---------:|----------:|----------:|
|  AscendingSort | 3.101 us | 0.0298 us | 0.0636 us |
| DescendingSort | 3.195 us | 0.0297 us | 0.0632 us |
