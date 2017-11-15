``` ini

BenchmarkDotNet=v0.10.10, OS=Mac OS X 10.12
Processor=Intel Core i5-5257U CPU 2.70GHz (Broadwell), ProcessorCount=4
.NET Core SDK=2.0.0
  [Host]     : .NET Core 1.1.4 (Framework 4.6.25714.03), 64bit RyuJIT
  DefaultJob : .NET Core 1.1.4 (Framework 4.6.25714.03), 64bit RyuJIT


```
|                  Method |     Mean |     Error |    StdDev |
|------------------------ |---------:|----------:|----------:|
| DeserializeSimpleObject | 27.29 us | 0.5275 us | 0.5863 us |
