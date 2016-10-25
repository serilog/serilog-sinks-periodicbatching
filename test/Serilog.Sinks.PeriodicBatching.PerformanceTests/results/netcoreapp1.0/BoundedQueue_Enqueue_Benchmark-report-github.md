```ini

Host Process Environment Information:
BenchmarkDotNet.Core=v0.9.9.0
OS=Windows
Processor=?, ProcessorCount=8
Frequency=3507522 ticks, Resolution=285.1016 ns, Timer=TSC
CLR=CORE, Arch=64-bit ? [RyuJIT]
GC=Concurrent Workstation
dotnet cli version: 1.0.0-preview2-003131

Type=BoundedQueue_Enqueue_Benchmark  Mode=Throughput  

```
                 Method | Items | Limit | ConcurrencyLevel |        Median |    StdDev | Scaled | Scaled-SD |
----------------------- |------ |------ |----------------- |-------------- |---------- |------- |---------- |
        **ConcurrentQueue** |    **50** |    **-1** |               **-1** |     **4.7730 us** | **0.0834 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |    50 |    -1 |               -1 |     5.1570 us | 0.0419 us |   1.08 |      0.02 |
     BlockingCollection |    50 |    -1 |               -1 |    15.2293 us | 0.8234 us |   3.25 |      0.18 |
      SynchronizedQueue |    50 |    -1 |               -1 |     7.6734 us | 0.1142 us |   1.60 |      0.04 |
        **ConcurrentQueue** |    **50** |    **-1** |                **1** |     **1.6705 us** | **0.0147 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |    50 |    -1 |                1 |     1.9790 us | 0.0764 us |   1.18 |      0.05 |
     BlockingCollection |    50 |    -1 |                1 |     4.1712 us | 0.0623 us |   2.49 |      0.04 |
      SynchronizedQueue |    50 |    -1 |                1 |     2.2616 us | 0.0833 us |   1.35 |      0.05 |
        **ConcurrentQueue** |    **50** |    **50** |               **-1** |     **4.7571 us** | **0.0811 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |    50 |    50 |               -1 |     5.6806 us | 0.0393 us |   1.20 |      0.02 |
     BlockingCollection |    50 |    50 |               -1 |    22.4318 us | 1.3097 us |   4.78 |      0.28 |
      SynchronizedQueue |    50 |    50 |               -1 |     7.4808 us | 0.0458 us |   1.57 |      0.03 |
        **ConcurrentQueue** |    **50** |    **50** |                **1** |     **1.7504 us** | **0.0806 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |    50 |    50 |                1 |     2.2882 us | 0.0541 us |   1.31 |      0.07 |
     BlockingCollection |    50 |    50 |                1 |     6.5085 us | 0.0938 us |   3.73 |      0.18 |
      SynchronizedQueue |    50 |    50 |                1 |     2.1800 us | 0.0139 us |   1.25 |      0.06 |
        **ConcurrentQueue** |    **50** |   **100** |               **-1** |     **4.6862 us** | **0.0208 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |    50 |   100 |               -1 |     5.7488 us | 0.0804 us |   1.23 |      0.02 |
     BlockingCollection |    50 |   100 |               -1 |    22.8035 us | 3.0473 us |   5.01 |      0.64 |
      SynchronizedQueue |    50 |   100 |               -1 |     7.5296 us | 0.0931 us |   1.61 |      0.02 |
        **ConcurrentQueue** |    **50** |   **100** |                **1** |     **1.6933 us** | **0.0110 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |    50 |   100 |                1 |     2.2958 us | 0.0697 us |   1.36 |      0.04 |
     BlockingCollection |    50 |   100 |                1 |     6.4820 us | 0.0494 us |   3.82 |      0.04 |
      SynchronizedQueue |    50 |   100 |                1 |     2.1791 us | 0.0099 us |   1.29 |      0.01 |
        **ConcurrentQueue** |   **100** |    **-1** |               **-1** |     **6.6384 us** | **0.0787 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |   100 |    -1 |               -1 |     6.9712 us | 0.0814 us |   1.05 |      0.02 |
     BlockingCollection |   100 |    -1 |               -1 |    35.1049 us | 4.4887 us |   5.44 |      0.67 |
      SynchronizedQueue |   100 |    -1 |               -1 |    15.7968 us | 0.6234 us |   2.39 |      0.10 |
        **ConcurrentQueue** |   **100** |    **-1** |                **1** |     **2.7321 us** | **0.0932 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |   100 |    -1 |                1 |     3.0368 us | 0.0262 us |   1.12 |      0.04 |
     BlockingCollection |   100 |    -1 |                1 |     7.4407 us | 0.0607 us |   2.74 |      0.09 |
      SynchronizedQueue |   100 |    -1 |                1 |     3.5805 us | 0.0960 us |   1.32 |      0.06 |
        **ConcurrentQueue** |   **100** |    **50** |               **-1** |     **6.7204 us** | **0.1360 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |   100 |    50 |               -1 |     7.8006 us | 0.0774 us |   1.16 |      0.03 |
     BlockingCollection |   100 |    50 |               -1 |   359.9633 us | 1.8706 us |  53.51 |      1.09 |
      SynchronizedQueue |   100 |    50 |               -1 |    12.7241 us | 0.2875 us |   1.90 |      0.06 |
        **ConcurrentQueue** |   **100** |    **50** |                **1** |     **2.6575 us** | **0.0158 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |   100 |    50 |                1 |     3.1897 us | 0.0207 us |   1.20 |      0.01 |
     BlockingCollection |   100 |    50 |                1 | 1,391.3352 us | 8.2379 us | 522.99 |      4.27 |
      SynchronizedQueue |   100 |    50 |                1 |     3.2691 us | 0.1015 us |   1.23 |      0.04 |
        **ConcurrentQueue** |   **100** |   **100** |               **-1** |     **6.5719 us** | **0.0547 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |   100 |   100 |               -1 |     8.5435 us | 0.1015 us |   1.30 |      0.02 |
     BlockingCollection |   100 |   100 |               -1 |    48.7996 us | 9.4255 us |   7.67 |      1.42 |
      SynchronizedQueue |   100 |   100 |               -1 |    15.7037 us | 0.8678 us |   2.40 |      0.13 |
        **ConcurrentQueue** |   **100** |   **100** |                **1** |     **2.6204 us** | **0.0091 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |   100 |   100 |                1 |     3.7370 us | 0.0743 us |   1.43 |      0.03 |
     BlockingCollection |   100 |   100 |                1 |    11.8234 us | 0.0873 us |   4.52 |      0.04 |
      SynchronizedQueue |   100 |   100 |                1 |     3.4981 us | 0.0177 us |   1.34 |      0.01 |
