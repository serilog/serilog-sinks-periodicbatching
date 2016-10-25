```ini

Host Process Environment Information:
BenchmarkDotNet.Core=v0.9.9.0
OS=Windows
Processor=?, ProcessorCount=8
Frequency=3507522 ticks, Resolution=285.1016 ns, Timer=TSC
CLR=CORE, Arch=64-bit ? [RyuJIT]
GC=Concurrent Workstation
dotnet cli version: 1.0.0-preview2-003131

Type=BoundedQueue_Enqueue_Dequeue_Benchmark  Mode=Throughput  

```
                 Method | Items | Limit |      Median |    StdDev | Scaled | Scaled-SD |
----------------------- |------ |------ |------------ |---------- |------- |---------- |
        **ConcurrentQueue** |    **50** |    **-1** |   **7.0749 us** | **0.0914 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |    50 |    -1 |   7.4228 us | 0.0827 us |   1.05 |      0.02 |
     BlockingCollection |    50 |    -1 |  36.9636 us | 2.4036 us |   5.34 |      0.34 |
      SynchronizedQueue |    50 |    -1 |  15.3732 us | 0.5077 us |   2.18 |      0.08 |
        **ConcurrentQueue** |    **50** |    **50** |   **7.0484 us** | **0.0917 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |    50 |    50 |   8.4134 us | 0.1526 us |   1.20 |      0.03 |
     BlockingCollection |    50 |    50 |  50.2187 us | 9.5376 us |   7.48 |      1.34 |
      SynchronizedQueue |    50 |    50 |  15.0374 us | 0.3315 us |   2.13 |      0.05 |
        **ConcurrentQueue** |    **50** |   **100** |   **7.0603 us** | **0.1021 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |    50 |   100 |   8.3543 us | 0.0798 us |   1.18 |      0.02 |
     BlockingCollection |    50 |   100 |  51.4431 us | 5.1939 us |   7.34 |      0.73 |
      SynchronizedQueue |    50 |   100 |  15.2502 us | 0.6226 us |   2.17 |      0.09 |
        **ConcurrentQueue** |   **100** |    **-1** |   **9.9110 us** | **0.1666 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |   100 |    -1 |  10.1370 us | 0.1678 us |   1.03 |      0.02 |
     BlockingCollection |   100 |    -1 |  58.9588 us | 3.7403 us |   6.01 |      0.38 |
      SynchronizedQueue |   100 |    -1 |  29.6655 us | 2.0825 us |   3.03 |      0.21 |
        **ConcurrentQueue** |   **100** |    **50** |   **9.9629 us** | **0.1519 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |   100 |    50 |  12.6271 us | 0.3635 us |   1.28 |      0.04 |
     BlockingCollection |   100 |    50 | 172.5832 us | 3.3398 us |  17.46 |      0.42 |
      SynchronizedQueue |   100 |    50 |  28.2467 us | 4.9548 us |   2.99 |      0.50 |
        **ConcurrentQueue** |   **100** |   **100** |  **10.0306 us** | **0.1885 us** |   **1.00** |      **0.00** |
 BoundedConcurrentQueue |   100 |   100 |  12.4031 us | 0.2527 us |   1.24 |      0.03 |
     BlockingCollection |   100 |   100 |  70.7232 us | 2.5452 us |   7.03 |      0.28 |
      SynchronizedQueue |   100 |   100 |  29.4855 us | 3.1710 us |   3.02 |      0.32 |
