``` ini

BenchmarkDotNet=v0.10.3.0, OS=Microsoft Windows 10.0.14393
Processor=Intel(R) Core(TM) i7-4790 CPU 3.60GHz, ProcessorCount=8
Frequency=3507499 Hz, Resolution=285.1034 ns, Timer=TSC
dotnet cli version=1.0.0
  [Host]     : .NET Core 4.6.25009.03, 64bit RyuJIT
  DefaultJob : .NET Core 4.6.25009.03, 64bit RyuJIT


```
 |                 Method | Items | Limit |       Mean |    StdDev | Scaled | Scaled-StdDev |
 |----------------------- |------ |------ |----------- |---------- |------- |-------------- |
 |        **ConcurrentQueue** |    **50** |    **-1** |  **7.2422 us** | **0.0720 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |    50 |    -1 |  7.6356 us | 0.0919 us |   1.05 |          0.02 |
 |     BlockingCollection |    50 |    -1 | 25.4929 us | 0.8222 us |   3.52 |          0.11 |
 |      SynchronizedQueue |    50 |    -1 | 12.8276 us | 0.1913 us |   1.77 |          0.03 |
 |        **ConcurrentQueue** |    **50** |    **50** |  **7.2297 us** | **0.0630 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |    50 |    50 |  8.7806 us | 0.0856 us |   1.21 |          0.02 |
 |     BlockingCollection |    50 |    50 | 35.1334 us | 0.7982 us |   4.86 |          0.11 |
 |      SynchronizedQueue |    50 |    50 | 12.8640 us | 0.2109 us |   1.78 |          0.03 |
 |        **ConcurrentQueue** |    **50** |   **100** |  **7.3600 us** | **0.1077 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |    50 |   100 |  8.8626 us | 0.0979 us |   1.20 |          0.02 |
 |     BlockingCollection |    50 |   100 | 36.0215 us | 0.8452 us |   4.90 |          0.13 |
 |      SynchronizedQueue |    50 |   100 | 12.9318 us | 0.1471 us |   1.76 |          0.03 |
 |        **ConcurrentQueue** |   **100** |    **-1** | **10.6257 us** | **0.1468 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |   100 |    -1 | 11.1374 us | 0.1332 us |   1.05 |          0.02 |
 |     BlockingCollection |   100 |    -1 | 49.6138 us | 1.3730 us |   4.67 |          0.14 |
 |      SynchronizedQueue |   100 |    -1 | 23.9550 us | 0.6100 us |   2.25 |          0.06 |
 |        **ConcurrentQueue** |   **100** |    **50** | **10.6249 us** | **0.0920 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |   100 |    50 | 12.9409 us | 0.1868 us |   1.22 |          0.02 |
 |     BlockingCollection |   100 |    50 | 55.0996 us | 2.3733 us |   5.19 |          0.22 |
 |      SynchronizedQueue |   100 |    50 | 22.3050 us | 0.8641 us |   2.10 |          0.08 |
 |        **ConcurrentQueue** |   **100** |   **100** | **10.6000 us** | **0.1713 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |   100 |   100 | 13.2870 us | 0.2239 us |   1.25 |          0.03 |
 |     BlockingCollection |   100 |   100 | 59.8952 us | 2.8346 us |   5.65 |          0.28 |
 |      SynchronizedQueue |   100 |   100 | 23.5773 us | 0.6238 us |   2.22 |          0.07 |
