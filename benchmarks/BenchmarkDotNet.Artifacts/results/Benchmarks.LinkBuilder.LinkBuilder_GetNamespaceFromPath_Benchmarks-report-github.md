``` ini

BenchmarkDotNet=v0.10.10, OS=Mac OS X 10.12
Processor=Intel Core i5-5257U CPU 2.70GHz (Broadwell), ProcessorCount=4
.NET Core SDK=2.1.4
  [Host]     : .NET Core 2.0.5 (Framework 4.6.0.0), 64bit RyuJIT
  Job-XFMVNE : .NET Core 2.0.5 (Framework 4.6.0.0), 64bit RyuJIT

LaunchCount=3  TargetCount=20  WarmupCount=10  

```
|                     Method |       Mean |     Error |    StdDev |  Gen 0 | Allocated |
|--------------------------- |-----------:|----------:|----------:|-------:|----------:|
|                 UsingSplit | 1,197.6 ns | 11.929 ns | 25.933 ns | 0.9251 |    1456 B |
| UsingSpanWithStringBuilder | 1,542.0 ns | 15.249 ns | 33.792 ns | 0.9460 |    1488 B |
|       UsingSpanWithNoAlloc |   272.6 ns |  2.265 ns |  5.018 ns | 0.0863 |     136 B |
