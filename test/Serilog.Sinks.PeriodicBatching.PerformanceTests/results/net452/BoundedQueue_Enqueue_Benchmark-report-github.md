``` ini

BenchmarkDotNet=v0.10.3.0, OS=Microsoft Windows NT 6.2.9200.0
Processor=Intel(R) Core(TM) i7-4790 CPU 3.60GHz, ProcessorCount=8
Frequency=3507499 Hz, Resolution=285.1034 ns, Timer=TSC
  [Host]     : Clr 4.0.30319.42000, 32bit LegacyJIT-v4.6.1586.0
  DefaultJob : Clr 4.0.30319.42000, 32bit LegacyJIT-v4.6.1586.0


```
 |                 Method | Items | Limit | ConcurrencyLevel |          Mean |    StdErr |    StdDev | Scaled | Scaled-StdDev |
 |----------------------- |------ |------ |----------------- |-------------- |---------- |---------- |------- |-------------- |
 |        **ConcurrentQueue** |    **50** |    **-1** |               **-1** |     **7.2804 us** | **0.0103 us** | **0.0401 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |    50 |    -1 |               -1 |     7.2908 us | 0.0388 us | 0.1503 us |   1.00 |          0.02 |
 |     BlockingCollection |    50 |    -1 |               -1 |    17.7474 us | 0.0848 us | 0.3059 us |   2.44 |          0.04 |
 |      SynchronizedQueue |    50 |    -1 |               -1 |    12.1052 us | 0.0216 us | 0.0837 us |   1.66 |          0.01 |
 |        **ConcurrentQueue** |    **50** |    **-1** |                **1** |     **3.4143 us** | **0.0338 us** | **0.1551 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |    50 |    -1 |                1 |     3.5365 us | 0.0352 us | 0.1409 us |   1.04 |          0.06 |
 |     BlockingCollection |    50 |    -1 |                1 |     6.8568 us | 0.0684 us | 0.3683 us |   2.01 |          0.14 |
 |      SynchronizedQueue |    50 |    -1 |                1 |     4.4259 us | 0.0376 us | 0.1455 us |   1.30 |          0.07 |
 |        **ConcurrentQueue** |    **50** |    **50** |               **-1** |     **7.2713 us** | **0.0079 us** | **0.0297 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |    50 |    50 |               -1 |     8.4583 us | 0.0461 us | 0.1787 us |   1.16 |          0.02 |
 |     BlockingCollection |    50 |    50 |               -1 |    23.0957 us | 0.1349 us | 0.5224 us |   3.18 |          0.07 |
 |      SynchronizedQueue |    50 |    50 |               -1 |    12.0785 us | 0.0264 us | 0.1023 us |   1.66 |          0.02 |
 |        **ConcurrentQueue** |    **50** |    **50** |                **1** |     **3.3591 us** | **0.0283 us** | **0.1019 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |    50 |    50 |                1 |     3.8662 us | 0.0378 us | 0.1730 us |   1.15 |          0.06 |
 |     BlockingCollection |    50 |    50 |                1 |     9.2685 us | 0.0939 us | 0.8237 us |   2.76 |          0.26 |
 |      SynchronizedQueue |    50 |    50 |                1 |     4.3973 us | 0.0456 us | 0.3288 us |   1.31 |          0.10 |
 |        **ConcurrentQueue** |    **50** |   **100** |               **-1** |     **7.2897 us** | **0.0112 us** | **0.0434 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |    50 |   100 |               -1 |     8.4600 us | 0.0271 us | 0.1049 us |   1.16 |          0.02 |
 |     BlockingCollection |    50 |   100 |               -1 |    22.9237 us | 0.1219 us | 0.4723 us |   3.14 |          0.07 |
 |      SynchronizedQueue |    50 |   100 |               -1 |    12.0344 us | 0.0149 us | 0.0578 us |   1.65 |          0.01 |
 |        **ConcurrentQueue** |    **50** |   **100** |                **1** |     **3.4225 us** | **0.0333 us** | **0.1596 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |    50 |   100 |                1 |     4.0642 us | 0.0207 us | 0.0716 us |   1.19 |          0.06 |
 |     BlockingCollection |    50 |   100 |                1 |     9.2182 us | 0.0922 us | 0.8194 us |   2.70 |          0.27 |
 |      SynchronizedQueue |    50 |   100 |                1 |     4.3935 us | 0.0434 us | 0.2713 us |   1.29 |          0.10 |
 |        **ConcurrentQueue** |   **100** |    **-1** |               **-1** |    **10.4820 us** | **0.0080 us** | **0.0301 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |   100 |    -1 |               -1 |    11.2203 us | 0.0277 us | 0.1075 us |   1.07 |          0.01 |
 |     BlockingCollection |   100 |    -1 |               -1 |    36.4722 us | 0.2981 us | 1.1544 us |   3.48 |          0.11 |
 |      SynchronizedQueue |   100 |    -1 |               -1 |    22.3040 us | 0.1371 us | 0.5130 us |   2.13 |          0.05 |
 |        **ConcurrentQueue** |   **100** |    **-1** |                **1** |     **4.7591 us** | **0.0396 us** | **0.1534 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |   100 |    -1 |                1 |     5.0724 us | 0.0504 us | 0.2715 us |   1.07 |          0.07 |
 |     BlockingCollection |   100 |    -1 |                1 |    10.6618 us | 0.1146 us | 0.7856 us |   2.24 |          0.18 |
 |      SynchronizedQueue |   100 |    -1 |                1 |     6.5492 us | 0.0650 us | 0.5438 us |   1.38 |          0.12 |
 |        **ConcurrentQueue** |   **100** |    **50** |               **-1** |    **10.5168 us** | **0.0088 us** | **0.0329 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |   100 |    50 |               -1 |    11.8475 us | 0.0321 us | 0.1201 us |   1.13 |          0.01 |
 |     BlockingCollection |   100 |    50 |               -1 |   368.4878 us | 0.3846 us | 1.4392 us |  35.04 |          0.17 |
 |      SynchronizedQueue |   100 |    50 |               -1 |    19.3906 us | 0.1377 us | 0.5333 us |   1.84 |          0.05 |
 |        **ConcurrentQueue** |   **100** |    **50** |                **1** |     **5.8222 us** | **0.0265 us** | **0.1243 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |   100 |    50 |                1 |     5.0835 us | 0.0500 us | 0.2447 us |   0.87 |          0.05 |
 |     BlockingCollection |   100 |    50 |                1 | 1,418.6158 us | 1.9614 us | 7.5965 us | 243.77 |          5.69 |
 |      SynchronizedQueue |   100 |    50 |                1 |     6.1161 us | 0.0601 us | 0.4073 us |   1.05 |          0.07 |
 |        **ConcurrentQueue** |   **100** |   **100** |               **-1** |    **10.4971 us** | **0.0164 us** | **0.0633 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |   100 |   100 |               -1 |    12.6199 us | 0.0405 us | 0.1569 us |   1.20 |          0.02 |
 |     BlockingCollection |   100 |   100 |               -1 |    51.6912 us | 0.5041 us | 2.5203 us |   4.92 |          0.24 |
 |      SynchronizedQueue |   100 |   100 |               -1 |    23.4763 us | 0.1860 us | 0.7204 us |   2.24 |          0.07 |
 |        **ConcurrentQueue** |   **100** |   **100** |                **1** |     **4.8530 us** | **0.0388 us** | **0.1503 us** |   **1.00** |          **0.00** |
 | BoundedConcurrentQueue |   100 |   100 |                1 |     5.8461 us | 0.0580 us | 0.4141 us |   1.21 |          0.09 |
 |     BlockingCollection |   100 |   100 |                1 |    14.9473 us | 0.1493 us | 1.3015 us |   3.08 |          0.28 |
 |      SynchronizedQueue |   100 |   100 |                1 |     6.5940 us | 0.0655 us | 0.5360 us |   1.36 |          0.12 |
