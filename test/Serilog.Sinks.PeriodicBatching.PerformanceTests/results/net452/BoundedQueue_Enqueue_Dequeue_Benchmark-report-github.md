``` ini

BenchmarkDotNet=v0.10.3.0, OS=Microsoft Windows NT 6.2.9200.0
Processor=Intel(R) Core(TM) i7-4790 CPU 3.60GHz, ProcessorCount=8
Frequency=3507499 Hz, Resolution=285.1034 ns, Timer=TSC
  [Host]     : Clr 4.0.30319.42000, 32bit LegacyJIT-v4.6.1586.0
  DefaultJob : Clr 4.0.30319.42000, 32bit LegacyJIT-v4.6.1586.0


```
 |                 Method | Items | Limit |        Mean |    StdErr |    StdDev | Scaled | Scaled-StdDev |
 |----------------------- |------ |------ |------------ |---------- |---------- |------- |-------------- |
 |        **ConcurrentQueue** |    **50** |    **-1** |  **10.3018 us** | **0.0261 us** | **0.0940 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |    50 |    -1 |  11.0476 us | 0.0208 us | 0.0804 us |   1.07 |          0.01 |
 |     BlockingCollection |    50 |    -1 |  37.3304 us | 0.1895 us | 0.7091 us |   3.62 |          0.07 |
 |      SynchronizedQueue |    50 |    -1 |  20.8889 us | 0.0861 us | 0.3223 us |   2.03 |          0.03 |
 |        **ConcurrentQueue** |    **50** |    **50** |  **10.3639 us** | **0.0167 us** | **0.0648 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |    50 |    50 |  12.5340 us | 0.0182 us | 0.0706 us |   1.21 |          0.01 |
 |     BlockingCollection |    50 |    50 |  52.3100 us | 0.5042 us | 1.8865 us |   5.05 |          0.18 |
 |      SynchronizedQueue |    50 |    50 |  20.8874 us | 0.1339 us | 0.5186 us |   2.02 |          0.05 |
 |        **ConcurrentQueue** |    **50** |   **100** |  **10.2835 us** | **0.0262 us** | **0.0981 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |    50 |   100 |  12.5809 us | 0.0318 us | 0.1230 us |   1.22 |          0.02 |
 |     BlockingCollection |    50 |   100 |  50.8533 us | 0.4826 us | 1.9898 us |   4.95 |          0.19 |
 |      SynchronizedQueue |    50 |   100 |  20.5994 us | 0.1159 us | 0.4338 us |   2.00 |          0.04 |
 |        **ConcurrentQueue** |   **100** |    **-1** |  **14.7901 us** | **0.0334 us** | **0.1295 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |   100 |    -1 |  15.3619 us | 0.0328 us | 0.1270 us |   1.04 |          0.01 |
 |     BlockingCollection |   100 |    -1 |  67.4710 us | 0.3917 us | 1.4122 us |   4.56 |          0.10 |
 |      SynchronizedQueue |   100 |    -1 |  41.1039 us | 0.4912 us | 2.0254 us |   2.78 |          0.13 |
 |        **ConcurrentQueue** |   **100** |    **50** |  **14.8436 us** | **0.0276 us** | **0.0956 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |   100 |    50 |  17.3593 us | 0.0352 us | 0.1364 us |   1.17 |          0.01 |
 |     BlockingCollection |   100 |    50 | 173.0738 us | 1.0031 us | 3.7533 us |  11.66 |          0.25 |
 |      SynchronizedQueue |   100 |    50 |  36.4733 us | 0.2613 us | 1.0121 us |   2.46 |          0.07 |
 |        **ConcurrentQueue** |   **100** |   **100** |  **14.8464 us** | **0.0231 us** | **0.0894 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |   100 |   100 |  17.4290 us | 0.0391 us | 0.1516 us |   1.17 |          0.01 |
 |     BlockingCollection |   100 |   100 |  91.5932 us | 0.9253 us | 4.3401 us |   6.17 |          0.29 |
 |      SynchronizedQueue |   100 |   100 |  40.4670 us | 0.3657 us | 1.3186 us |   2.73 |          0.09 |
