``` ini

BenchmarkDotNet=v0.10.10, OS=Mac OS X 10.12
Processor=Intel Core i5-5257U CPU 2.70GHz (Broadwell), ProcessorCount=4
.NET Core SDK=2.1.4
  [Host]     : .NET Core 2.0.5 (Framework 4.6.0.0), 64bit RyuJIT
  DefaultJob : .NET Core 2.0.5 (Framework 4.6.0.0), 64bit RyuJIT


```
|     Method |      Mean |     Error |    StdDev |  Gen 0 | Allocated |
|----------- |----------:|----------:|----------:|-------:|----------:|
| UsingSplit | 157.28 ns | 2.9689 ns | 5.8602 ns | 0.2134 |     336 B |
|    Current |  39.96 ns | 0.6489 ns | 0.6070 ns |      - |       0 B |
