```ini

Host Process Environment Information:
BenchmarkDotNet.Core=v0.9.9.0
OS=Microsoft Windows NT 6.2.9200.0
Processor=Intel(R) Core(TM) i7-4790 CPU 3.60GHz, ProcessorCount=8
Frequency=3507522 ticks, Resolution=285.1016 ns, Timer=TSC
CLR=MS.NET 4.0.30319.42000, Arch=64-bit RELEASE [RyuJIT]
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1586.0

Type=BoundedQueue_Enqueue_Benchmark  Mode=Throughput  

```
                 Method | Items | Limit | ConcurrencyLevel |        Median |     StdDev | Scaled | Scaled-SD |
----------------------- |------ |------ |----------------- |-------------- |----------- |------- |---------- |
        **ConcurrentQueue** |    **50** |    **-1** |               **-1** |     **4.5754 us** |  **0.0268 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |    50 |    -1 |               -1 |     4.9072 us |  0.0208 us |   1.07 |      0.01 |
     BlockingCollection |    50 |    -1 |               -1 |    15.2352 us |  0.4025 us |   3.33 |      0.09 |
      SynchronizedQueue |    50 |    -1 |               -1 |     7.0956 us |  0.0633 us |   1.55 |      0.02 |
        **ConcurrentQueue** |    **50** |    **-1** |                **1** |     **2.5920 us** |  **0.0757 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |    50 |    -1 |                1 |     2.7877 us |  0.0632 us |   1.07 |      0.04 |
     BlockingCollection |    50 |    -1 |                1 |     5.4940 us |  0.1542 us |   2.13 |      0.08 |
      SynchronizedQueue |    50 |    -1 |                1 |     3.1676 us |  0.0917 us |   1.23 |      0.05 |
        **ConcurrentQueue** |    **50** |    **50** |               **-1** |     **4.5830 us** |  **0.0276 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |    50 |    50 |               -1 |     5.6700 us |  0.0270 us |   1.24 |      0.01 |
     BlockingCollection |    50 |    50 |               -1 |    23.7805 us |  2.4256 us |   5.36 |      0.52 |
      SynchronizedQueue |    50 |    50 |               -1 |     6.9731 us |  0.0735 us |   1.53 |      0.02 |
        **ConcurrentQueue** |    **50** |    **50** |                **1** |     **2.6240 us** |  **0.0599 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |    50 |    50 |                1 |     3.0955 us |  0.0685 us |   1.19 |      0.04 |
     BlockingCollection |    50 |    50 |                1 |     8.1912 us |  0.5452 us |   3.12 |      0.22 |
      SynchronizedQueue |    50 |    50 |                1 |     3.1104 us |  0.1233 us |   1.20 |      0.05 |
        **ConcurrentQueue** |    **50** |   **100** |               **-1** |     **4.6141 us** |  **0.0494 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |    50 |   100 |               -1 |     5.6492 us |  0.1037 us |   1.23 |      0.03 |
     BlockingCollection |    50 |   100 |               -1 |    24.1106 us |  1.8002 us |   5.29 |      0.39 |
      SynchronizedQueue |    50 |   100 |               -1 |     7.0290 us |  0.0761 us |   1.53 |      0.02 |
        **ConcurrentQueue** |    **50** |   **100** |                **1** |     **2.6345 us** |  **0.0863 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |    50 |   100 |                1 |     3.0886 us |  0.0814 us |   1.18 |      0.05 |
     BlockingCollection |    50 |   100 |                1 |     8.1912 us |  0.5398 us |   3.12 |      0.23 |
      SynchronizedQueue |    50 |   100 |                1 |     3.1038 us |  0.0882 us |   1.19 |      0.05 |
        **ConcurrentQueue** |   **100** |    **-1** |               **-1** |     **6.8298 us** |  **0.0668 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |   100 |    -1 |               -1 |     7.2814 us |  0.0375 us |   1.06 |      0.01 |
     BlockingCollection |   100 |    -1 |               -1 |    36.2428 us |  5.7991 us |   5.54 |      0.84 |
      SynchronizedQueue |   100 |    -1 |               -1 |    14.5968 us |  0.3317 us |   2.15 |      0.05 |
        **ConcurrentQueue** |   **100** |    **-1** |                **1** |     **3.7145 us** |  **0.1000 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |   100 |    -1 |                1 |     4.0368 us |  0.0925 us |   1.09 |      0.04 |
     BlockingCollection |   100 |    -1 |                1 |     8.7960 us |  0.5201 us |   2.40 |      0.15 |
      SynchronizedQueue |   100 |    -1 |                1 |     4.7581 us |  0.1434 us |   1.29 |      0.05 |
        **ConcurrentQueue** |   **100** |    **50** |               **-1** |     **6.8500 us** |  **0.0586 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |   100 |    50 |               -1 |     7.9982 us |  0.0915 us |   1.16 |      0.02 |
     BlockingCollection |   100 |    50 |               -1 |   382.8411 us |  3.2581 us |  55.94 |      0.66 |
      SynchronizedQueue |   100 |    50 |               -1 |    11.9375 us |  0.1305 us |   1.74 |      0.02 |
        **ConcurrentQueue** |   **100** |    **50** |                **1** |     **3.7684 us** |  **0.1039 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |   100 |    50 |                1 |     4.0127 us |  0.0859 us |   1.07 |      0.04 |
     BlockingCollection |   100 |    50 |                1 | 1,511.4670 us | 17.2323 us | 402.71 |     11.61 |
      SynchronizedQueue |   100 |    50 |                1 |     4.3747 us |  0.1618 us |   1.15 |      0.05 |
        **ConcurrentQueue** |   **100** |   **100** |               **-1** |     **6.8885 us** |  **0.0845 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |   100 |   100 |               -1 |     8.7115 us |  0.0501 us |   1.26 |      0.02 |
     BlockingCollection |   100 |   100 |               -1 |    51.7677 us |  5.4497 us |   7.71 |      0.79 |
      SynchronizedQueue |   100 |   100 |               -1 |    14.4318 us |  0.4317 us |   2.09 |      0.07 |
        **ConcurrentQueue** |   **100** |   **100** |                **1** |     **3.7928 us** |  **0.0980 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |   100 |   100 |                1 |     4.7090 us |  0.0941 us |   1.24 |      0.04 |
     BlockingCollection |   100 |   100 |                1 |    12.9744 us |  0.0729 us |   3.43 |      0.09 |
      SynchronizedQueue |   100 |   100 |                1 |     4.6922 us |  0.1199 us |   1.24 |      0.04 |
