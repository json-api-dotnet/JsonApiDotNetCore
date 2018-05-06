```ini
BenchmarkDotNet=v0.10.10, OS=Mac OS X 10.12
Processor=Intel Core i5-5257U CPU 2.70GHz (Broadwell), ProcessorCount=4
.NET Core SDK=2.1.4
  [Host]     : .NET Core 2.0.5 (Framework 4.6.0.0), 64bit RyuJIT
  DefaultJob : .NET Core 2.0.5 (Framework 4.6.0.0), 64bit RyuJIT
```

| Method     |      Mean |      Error |     StdDev |  Gen 0 | Allocated |
| ---------- | --------: | ---------: | ---------: | -----: | --------: |
| UsingSplit | 421.08 ns | 19.3905 ns | 54.0529 ns | 0.4725 |     744 B |
| Current    |  52.23 ns |  0.8052 ns |  0.7532 ns |      - |       0 B |
