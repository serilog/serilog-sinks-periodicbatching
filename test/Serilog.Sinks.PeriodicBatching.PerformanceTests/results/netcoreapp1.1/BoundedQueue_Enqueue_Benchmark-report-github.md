``` ini

BenchmarkDotNet=v0.10.3.0, OS=Microsoft Windows 10.0.14393
Processor=Intel(R) Core(TM) i7-4790 CPU 3.60GHz, ProcessorCount=8
Frequency=3507499 Hz, Resolution=285.1034 ns, Timer=TSC
dotnet cli version=1.0.0
  [Host]     : .NET Core 4.6.25009.03, 64bit RyuJIT
  DefaultJob : .NET Core 4.6.25009.03, 64bit RyuJIT


```
 |                 Method | Items | Limit | ConcurrencyLevel |       Mean |    StdErr |    StdDev | Scaled | Scaled-StdDev |
 |----------------------- |------ |------ |----------------- |----------- |---------- |---------- |------- |-------------- |
 |        **ConcurrentQueue** |    **50** |    **-1** |               **-1** |  **5.1811 us** | **0.0061 us** | **0.0237 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |    50 |    -1 |               -1 |  5.5466 us | 0.0111 us | 0.0431 us |   1.07 |          0.01 |
 |     BlockingCollection |    50 |    -1 |               -1 | 14.6056 us | 0.0977 us | 0.3783 us |   2.82 |          0.07 |
 |      SynchronizedQueue |    50 |    -1 |               -1 |  7.6025 us | 0.0130 us | 0.0505 us |   1.47 |          0.01 |
 |        **ConcurrentQueue** |    **50** |    **-1** |                **1** |  **1.9633 us** | **0.0018 us** | **0.0069 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |    50 |    -1 |                1 |  2.1595 us | 0.0067 us | 0.0232 us |   1.10 |          0.01 |
 |     BlockingCollection |    50 |    -1 |                1 |  4.4581 us | 0.0030 us | 0.0111 us |   2.27 |          0.01 |
 |      SynchronizedQueue |    50 |    -1 |                1 |  2.4642 us | 0.0039 us | 0.0151 us |   1.26 |          0.01 |
 |        **ConcurrentQueue** |    **50** |    **50** |               **-1** |  **5.1484 us** | **0.0080 us** | **0.0312 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |    50 |    50 |               -1 |  6.3047 us | 0.0042 us | 0.0153 us |   1.22 |          0.01 |
 |     BlockingCollection |    50 |    50 |               -1 | 21.8181 us | 0.2179 us | 1.1734 us |   4.24 |          0.23 |
 |      SynchronizedQueue |    50 |    50 |               -1 |  7.5549 us | 0.0140 us | 0.0542 us |   1.47 |          0.01 |
 |        **ConcurrentQueue** |    **50** |    **50** |                **1** |  **1.9670 us** | **0.0023 us** | **0.0090 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |    50 |    50 |                1 |  2.4802 us | 0.0022 us | 0.0083 us |   1.26 |          0.01 |
 |     BlockingCollection |    50 |    50 |                1 |  6.9471 us | 0.0075 us | 0.0281 us |   3.53 |          0.02 |
 |      SynchronizedQueue |    50 |    50 |                1 |  2.4790 us | 0.0012 us | 0.0048 us |   1.26 |          0.01 |
 |        **ConcurrentQueue** |    **50** |   **100** |               **-1** |  **5.5230 us** | **0.0074 us** | **0.0286 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |    50 |   100 |               -1 |  6.3623 us | 0.0122 us | 0.0456 us |   1.15 |          0.01 |
 |     BlockingCollection |    50 |   100 |               -1 | 21.1418 us | 0.1764 us | 0.6600 us |   3.83 |          0.12 |
 |      SynchronizedQueue |    50 |   100 |               -1 |  7.5030 us | 0.0113 us | 0.0422 us |   1.36 |          0.01 |
 |        **ConcurrentQueue** |    **50** |   **100** |                **1** |  **1.9761 us** | **0.0030 us** | **0.0118 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |    50 |   100 |                1 |  2.4867 us | 0.0015 us | 0.0058 us |   1.26 |          0.01 |
 |     BlockingCollection |    50 |   100 |                1 |  6.9420 us | 0.0099 us | 0.0385 us |   3.51 |          0.03 |
 |      SynchronizedQueue |    50 |   100 |                1 |  2.4812 us | 0.0021 us | 0.0083 us |   1.26 |          0.01 |
 |        **ConcurrentQueue** |   **100** |    **-1** |               **-1** |  **7.6981 us** | **0.0179 us** | **0.0692 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |   100 |    -1 |               -1 |  8.0702 us | 0.0197 us | 0.0763 us |   1.05 |          0.01 |
 |     BlockingCollection |   100 |    -1 |               -1 | 32.4460 us | 0.4472 us | 1.6731 us |   4.22 |          0.21 |
 |      SynchronizedQueue |   100 |    -1 |               -1 | 12.7754 us | 0.0340 us | 0.1272 us |   1.66 |          0.02 |
 |        **ConcurrentQueue** |   **100** |    **-1** |                **1** |  **3.0726 us** | **0.0040 us** | **0.0154 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |   100 |    -1 |                1 |  3.4069 us | 0.0024 us | 0.0095 us |   1.11 |          0.01 |
 |     BlockingCollection |   100 |    -1 |                1 |  7.7433 us | 0.0116 us | 0.0435 us |   2.52 |          0.02 |
 |      SynchronizedQueue |   100 |    -1 |                1 |  3.7620 us | 0.0022 us | 0.0079 us |   1.22 |          0.01 |
 |        **ConcurrentQueue** |   **100** |    **50** |               **-1** |  **7.8410 us** | **0.0306 us** | **0.1184 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |   100 |    50 |               -1 |  8.4654 us | 0.0106 us | 0.0395 us |   1.08 |          0.02 |
 |     BlockingCollection |   100 |    50 |               -1 | 28.9868 us | 0.2876 us | 1.0761 us |   3.70 |          0.14 |
 |      SynchronizedQueue |   100 |    50 |               -1 | 11.0092 us | 0.0305 us | 0.1183 us |   1.40 |          0.03 |
 |        **ConcurrentQueue** |   **100** |    **50** |                **1** |  **3.0622 us** | **0.0029 us** | **0.0103 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |   100 |    50 |                1 |  3.3265 us | 0.0031 us | 0.0122 us |   1.09 |          0.01 |
 |     BlockingCollection |   100 |    50 |                1 |  8.1403 us | 0.0100 us | 0.0386 us |   2.66 |          0.01 |
 |      SynchronizedQueue |   100 |    50 |                1 |  3.4894 us | 0.0032 us | 0.0126 us |   1.14 |          0.01 |
 |        **ConcurrentQueue** |   **100** |   **100** |               **-1** |  **7.6139 us** | **0.0162 us** | **0.0607 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |   100 |   100 |               -1 |  9.5550 us | 0.0392 us | 0.1517 us |   1.26 |          0.02 |
 |     BlockingCollection |   100 |   100 |               -1 | 47.2014 us | 0.4680 us | 2.3402 us |   6.20 |          0.30 |
 |      SynchronizedQueue |   100 |   100 |               -1 | 12.6909 us | 0.0638 us | 0.2471 us |   1.67 |          0.03 |
 |        **ConcurrentQueue** |   **100** |   **100** |                **1** |  **3.0812 us** | **0.0045 us** | **0.0174 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |   100 |   100 |                1 |  4.0404 us | 0.0046 us | 0.0179 us |   1.31 |          0.01 |
 |     BlockingCollection |   100 |   100 |                1 | 12.7325 us | 0.0157 us | 0.0607 us |   4.13 |          0.03 |
 |      SynchronizedQueue |   100 |   100 |                1 |  3.7607 us | 0.0032 us | 0.0119 us |   1.22 |          0.01 |
