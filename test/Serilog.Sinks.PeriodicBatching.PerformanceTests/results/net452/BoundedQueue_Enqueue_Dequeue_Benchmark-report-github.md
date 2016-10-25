```ini

Host Process Environment Information:
BenchmarkDotNet.Core=v0.9.9.0
OS=Microsoft Windows NT 6.2.9200.0
Processor=Intel(R) Core(TM) i7-4790 CPU 3.60GHz, ProcessorCount=8
Frequency=3507522 ticks, Resolution=285.1016 ns, Timer=TSC
CLR=MS.NET 4.0.30319.42000, Arch=64-bit RELEASE [RyuJIT]
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1586.0

Type=BoundedQueue_Enqueue_Dequeue_Benchmark  Mode=Throughput  

```
                 Method | Items | Limit |      Median |     StdDev | Scaled | Scaled-SD |
----------------------- |------ |------ |------------ |----------- |------- |---------- |
        **ConcurrentQueue** |    **50** |    **-1** |   **6.7418 us** |  **0.0746 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |    50 |    -1 |   7.1363 us |  0.0520 us |   1.05 |      0.01 |
     BlockingCollection |    50 |    -1 |  37.4587 us |  1.5276 us |   5.53 |      0.23 |
      SynchronizedQueue |    50 |    -1 |  14.1155 us |  0.4030 us |   2.10 |      0.06 |
        **ConcurrentQueue** |    **50** |    **50** |   **6.6811 us** |  **0.0531 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |    50 |    50 |   8.3724 us |  0.0714 us |   1.25 |      0.01 |
     BlockingCollection |    50 |    50 |  52.3829 us |  7.3325 us |   8.07 |      1.08 |
      SynchronizedQueue |    50 |    50 |  13.7312 us |  0.6096 us |   2.08 |      0.09 |
        **ConcurrentQueue** |    **50** |   **100** |   **6.7392 us** |  **0.0449 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |    50 |   100 |   8.4151 us |  0.1020 us |   1.25 |      0.02 |
     BlockingCollection |    50 |   100 |  53.9248 us | 13.1645 us |   8.61 |      1.94 |
      SynchronizedQueue |    50 |   100 |  13.7264 us |  0.3052 us |   2.05 |      0.05 |
        **ConcurrentQueue** |   **100** |    **-1** |   **9.8471 us** |  **0.2358 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |   100 |    -1 |  10.1752 us |  0.1060 us |   1.03 |      0.03 |
     BlockingCollection |   100 |    -1 |  59.9669 us |  2.6132 us |   6.13 |      0.29 |
      SynchronizedQueue |   100 |    -1 |  29.4077 us |  2.2109 us |   3.01 |      0.23 |
        **ConcurrentQueue** |   **100** |    **50** |   **9.7427 us** |  **0.1430 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |   100 |    50 |  12.3263 us |  0.1486 us |   1.26 |      0.02 |
     BlockingCollection |   100 |    50 | 176.2485 us |  6.4545 us |  18.22 |      0.69 |
      SynchronizedQueue |   100 |    50 |  26.1062 us |  2.8776 us |   2.77 |      0.29 |
        **ConcurrentQueue** |   **100** |   **100** |   **9.7086 us** |  **0.0943 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |   100 |   100 |  12.3967 us |  0.1212 us |   1.28 |      0.02 |
     BlockingCollection |   100 |   100 |  71.4098 us |  8.7627 us |   7.65 |      0.88 |
      SynchronizedQueue |   100 |   100 |  28.9372 us |  1.5055 us |   2.99 |      0.16 |
