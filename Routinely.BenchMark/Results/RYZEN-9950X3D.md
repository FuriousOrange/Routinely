```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.26200.8037)
Unknown processor
.NET SDK 10.0.104
  [Host]     : .NET 10.0.4 (10.0.426.12010), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  DefaultJob : .NET 10.0.4 (10.0.426.12010), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI


```
| Method                               | Coroutines | Mean              | Error           | StdDev          | Ratio | RatioSD | Gen0     | Gen1     | Gen2     | Allocated | Alloc Ratio |
|------------------------------------- |----------- |------------------:|----------------:|----------------:|------:|--------:|---------:|---------:|---------:|----------:|------------:|
| **Single_Async**                         | **1**          |         **35.115 ns** |       **0.1519 ns** |       **0.1421 ns** |  **1.00** |    **0.01** |        **-** |        **-** |        **-** |         **-** |          **NA** |
| Single_Result_Async                  | 1          |         42.284 ns |       0.0911 ns |       0.0808 ns |  1.20 |    0.01 |        - |        - |        - |         - |          NA |
| Single_Sync                          | 1          |          1.560 ns |       0.0032 ns |       0.0026 ns |  0.04 |    0.00 |        - |        - |        - |         - |          NA |
| Single_Result_Sync                   | 1          |          1.928 ns |       0.0039 ns |       0.0035 ns |  0.05 |    0.00 |        - |        - |        - |         - |          NA |
| Stack_Depth_5_Async                  | 1          |         37.516 ns |       0.0491 ns |       0.0435 ns |  1.07 |    0.00 |        - |        - |        - |         - |          NA |
| Stack_Depth_5_Sync                   | 1          |          4.198 ns |       0.0152 ns |       0.0134 ns |  0.12 |    0.00 |        - |        - |        - |         - |          NA |
| Stack_Depth_5_Result_Async           | 1          |         41.477 ns |       0.0769 ns |       0.0682 ns |  1.18 |    0.00 |        - |        - |        - |         - |          NA |
| Stack_Depth_5_Result_Sync            | 1          |          4.134 ns |       0.0068 ns |       0.0060 ns |  0.12 |    0.00 |        - |        - |        - |         - |          NA |
| Nested_Awaits_Async                  | 1          |        329.469 ns |       0.6871 ns |       0.6427 ns |  9.38 |    0.04 |        - |        - |        - |         - |          NA |
| Nested_Awaits_Sync                   | 1          |         18.345 ns |       0.0573 ns |       0.0508 ns |  0.52 |    0.00 |        - |        - |        - |         - |          NA |
| Nested_Awaits_Result_Async           | 1          |        376.232 ns |       1.9594 ns |       1.7370 ns | 10.71 |    0.06 |        - |        - |        - |         - |          NA |
| Nested_Awaits_Result_Sync            | 1          |         24.478 ns |       0.0411 ns |       0.0364 ns |  0.70 |    0.00 |        - |        - |        - |         - |          NA |
| When_All_Async                       | 1          |        102.740 ns |       0.5495 ns |       0.4588 ns |  2.93 |    0.02 |        - |        - |        - |         - |          NA |
| When_All_Sync                        | 1          |          6.459 ns |       0.0151 ns |       0.0141 ns |  0.18 |    0.00 |        - |        - |        - |         - |          NA |
| When_All_Result_Async                | 1          |        105.364 ns |       0.5121 ns |       0.4540 ns |  3.00 |    0.02 |   0.0006 |        - |        - |      32 B |          NA |
| When_All_Result_Sync                 | 1          |          9.484 ns |       0.1377 ns |       0.1220 ns |  0.27 |    0.00 |   0.0006 |        - |        - |      32 B |          NA |
| When_All_Result_Async_Non_Allocating | 1          |        103.567 ns |       0.8493 ns |       0.7945 ns |  2.95 |    0.02 |        - |        - |        - |         - |          NA |
| When_All_Result_Sync_Non_Allocating  | 1          |          7.873 ns |       0.0261 ns |       0.0244 ns |  0.22 |    0.00 |        - |        - |        - |         - |          NA |
| When_Any_Async                       | 1          |        123.733 ns |       0.8920 ns |       0.7448 ns |  3.52 |    0.02 |        - |        - |        - |         - |          NA |
| When_Any_Sync                        | 1          |          6.956 ns |       0.1260 ns |       0.1179 ns |  0.20 |    0.00 |        - |        - |        - |         - |          NA |
| When_Any_Result_Async                | 1          |        119.537 ns |       0.5694 ns |       0.5048 ns |  3.40 |    0.02 |        - |        - |        - |         - |          NA |
| When_Any_Result_Sync                 | 1          |          8.403 ns |       0.0101 ns |       0.0089 ns |  0.24 |    0.00 |        - |        - |        - |         - |          NA |
| Yield_Steady_State                   | 1          |          9.043 ns |       0.0132 ns |       0.0110 ns |  0.26 |    0.00 |        - |        - |        - |         - |          NA |
| Switch_To_Steady_State               | 1          |         42.329 ns |       0.1209 ns |       0.1131 ns |  1.21 |    0.01 |        - |        - |        - |         - |          NA |
| Switch_To_Context_Steady_State       | 1          |         46.927 ns |       0.2481 ns |       0.2321 ns |  1.34 |    0.01 |        - |        - |        - |         - |          NA |
|                                      |            |                   |                 |                 |       |         |          |          |          |           |             |
| **Single_Async**                         | **1000**       |     **36,676.678 ns** |      **41.1863 ns** |      **38.5256 ns** |  **1.00** |    **0.00** |        **-** |        **-** |        **-** |         **-** |          **NA** |
| Single_Result_Async                  | 1000       |     41,085.299 ns |     169.8811 ns |     158.9069 ns |  1.12 |    0.00 |        - |        - |        - |         - |          NA |
| Single_Sync                          | 1000       |      1,655.050 ns |       3.6889 ns |       3.4506 ns |  0.05 |    0.00 |        - |        - |        - |         - |          NA |
| Single_Result_Sync                   | 1000       |      1,924.666 ns |       3.3376 ns |       3.1220 ns |  0.05 |    0.00 |        - |        - |        - |         - |          NA |
| Stack_Depth_5_Async                  | 1000       |     37,767.880 ns |      64.4213 ns |      60.2597 ns |  1.03 |    0.00 |        - |        - |        - |         - |          NA |
| Stack_Depth_5_Sync                   | 1000       |      2,786.332 ns |       7.1515 ns |       6.6895 ns |  0.08 |    0.00 |        - |        - |        - |         - |          NA |
| Stack_Depth_5_Result_Async           | 1000       |     42,634.801 ns |      88.0716 ns |      78.0732 ns |  1.16 |    0.00 |        - |        - |        - |         - |          NA |
| Stack_Depth_5_Result_Sync            | 1000       |      2,650.187 ns |       1.6746 ns |       1.3984 ns |  0.07 |    0.00 |        - |        - |        - |         - |          NA |
| Nested_Awaits_Async                  | 1000       |    330,970.270 ns |   2,038.3947 ns |   1,906.7156 ns |  9.02 |    0.05 |        - |        - |        - |         - |          NA |
| Nested_Awaits_Sync                   | 1000       |     16,346.788 ns |      11.7622 ns |       9.8220 ns |  0.45 |    0.00 |        - |        - |        - |         - |          NA |
| Nested_Awaits_Result_Async           | 1000       |    372,813.939 ns |   1,762.8073 ns |   1,648.9310 ns | 10.16 |    0.04 |        - |        - |        - |         - |          NA |
| Nested_Awaits_Result_Sync            | 1000       |     20,665.479 ns |      30.8861 ns |      27.3797 ns |  0.56 |    0.00 |        - |        - |        - |         - |          NA |
| When_All_Async                       | 1000       |     68,799.247 ns |     118.1446 ns |     104.7321 ns |  1.88 |    0.00 |        - |        - |        - |         - |          NA |
| When_All_Sync                        | 1000       |      3,677.583 ns |       3.3757 ns |       2.8189 ns |  0.10 |    0.00 |        - |        - |        - |         - |          NA |
| When_All_Result_Async                | 1000       |     68,852.590 ns |     160.4302 ns |     133.9665 ns |  1.88 |    0.00 |        - |        - |        - |    4024 B |          NA |
| When_All_Result_Sync                 | 1000       |      4,918.464 ns |      13.9184 ns |      13.0193 ns |  0.13 |    0.00 |   0.0763 |        - |        - |    4024 B |          NA |
| When_All_Result_Async_Non_Allocating | 1000       |     65,339.206 ns |     279.9638 ns |     261.8783 ns |  1.78 |    0.01 |        - |        - |        - |         - |          NA |
| When_All_Result_Sync_Non_Allocating  | 1000       |      4,712.272 ns |       6.9950 ns |       6.5432 ns |  0.13 |    0.00 |        - |        - |        - |         - |          NA |
| When_Any_Async                       | 1000       |     71,145.020 ns |      66.0112 ns |      55.1223 ns |  1.94 |    0.00 |        - |        - |        - |         - |          NA |
| When_Any_Sync                        | 1000       |      3,385.048 ns |       4.0580 ns |       3.1683 ns |  0.09 |    0.00 |        - |        - |        - |         - |          NA |
| When_Any_Result_Async                | 1000       |     71,060.746 ns |      29.4030 ns |      24.5529 ns |  1.94 |    0.00 |        - |        - |        - |         - |          NA |
| When_Any_Result_Sync                 | 1000       |      4,283.990 ns |       2.5792 ns |       2.0136 ns |  0.12 |    0.00 |        - |        - |        - |         - |          NA |
| Yield_Steady_State                   | 1000       |      7,940.656 ns |      22.5088 ns |      21.0547 ns |  0.22 |    0.00 |        - |        - |        - |         - |          NA |
| Switch_To_Steady_State               | 1000       |     39,897.479 ns |      71.0024 ns |      59.2902 ns |  1.09 |    0.00 |        - |        - |        - |         - |          NA |
| Switch_To_Context_Steady_State       | 1000       |     46,923.118 ns |     930.0237 ns |     869.9447 ns |  1.28 |    0.02 |        - |        - |        - |         - |          NA |
|                                      |            |                   |                 |                 |       |         |          |          |          |           |             |
| **Single_Async**                         | **10000**      |    **371,381.077 ns** |     **689.1385 ns** |     **644.6205 ns** |  **1.00** |    **0.00** |        **-** |        **-** |        **-** |         **-** |          **NA** |
| Single_Result_Async                  | 10000      |    423,840.560 ns |     356.1082 ns |     333.1038 ns |  1.14 |    0.00 |        - |        - |        - |         - |          NA |
| Single_Sync                          | 10000      |     16,426.448 ns |      22.7798 ns |      21.3082 ns |  0.04 |    0.00 |        - |        - |        - |         - |          NA |
| Single_Result_Sync                   | 10000      |     19,199.959 ns |      30.9299 ns |      27.4186 ns |  0.05 |    0.00 |        - |        - |        - |         - |          NA |
| Stack_Depth_5_Async                  | 10000      |    388,469.655 ns |     569.6840 ns |     532.8828 ns |  1.05 |    0.00 |        - |        - |        - |         - |          NA |
| Stack_Depth_5_Sync                   | 10000      |     27,911.315 ns |      33.0263 ns |      30.8928 ns |  0.08 |    0.00 |        - |        - |        - |         - |          NA |
| Stack_Depth_5_Result_Async           | 10000      |    433,731.771 ns |     970.7687 ns |     757.9123 ns |  1.17 |    0.00 |        - |        - |        - |         - |          NA |
| Stack_Depth_5_Result_Sync            | 10000      |     27,577.266 ns |      27.6909 ns |      23.1232 ns |  0.07 |    0.00 |        - |        - |        - |         - |          NA |
| Nested_Awaits_Async                  | 10000      |  3,476,404.395 ns |  52,078.4742 ns |  40,659.4471 ns |  9.36 |    0.11 |        - |        - |        - |         - |          NA |
| Nested_Awaits_Sync                   | 10000      |    167,354.246 ns |     624.8012 ns |     584.4394 ns |  0.45 |    0.00 |        - |        - |        - |         - |          NA |
| Nested_Awaits_Result_Async           | 10000      |  3,953,272.070 ns |  76,359.0820 ns |  81,703.3623 ns | 10.64 |    0.21 |        - |        - |        - |         - |          NA |
| Nested_Awaits_Result_Sync            | 10000      |    202,111.245 ns |     591.8456 ns |     553.6127 ns |  0.54 |    0.00 |        - |        - |        - |         - |          NA |
| When_All_Async                       | 10000      |    851,381.906 ns |   6,982.1545 ns |   6,189.4981 ns |  2.29 |    0.02 |        - |        - |        - |         - |          NA |
| When_All_Sync                        | 10000      |     37,131.548 ns |      90.0918 ns |      84.2719 ns |  0.10 |    0.00 |        - |        - |        - |         - |          NA |
| When_All_Result_Async                | 10000      |    697,326.191 ns |   2,692.5240 ns |   2,518.5886 ns |  1.88 |    0.01 |        - |        - |        - |   40024 B |          NA |
| When_All_Result_Sync                 | 10000      |     48,523.312 ns |     254.7284 ns |     198.8752 ns |  0.13 |    0.00 |   0.7935 |        - |        - |   40024 B |          NA |
| When_All_Result_Async_Non_Allocating | 10000      |    685,632.609 ns |   1,001.4088 ns |     781.8341 ns |  1.85 |    0.00 |        - |        - |        - |         - |          NA |
| When_All_Result_Sync_Non_Allocating  | 10000      |     47,619.546 ns |     204.2141 ns |     170.5280 ns |  0.13 |    0.00 |        - |        - |        - |         - |          NA |
| When_Any_Async                       | 10000      |    752,362.767 ns |   4,922.2202 ns |   4,604.2479 ns |  2.03 |    0.01 |        - |        - |        - |         - |          NA |
| When_Any_Sync                        | 10000      |     32,019.581 ns |      71.7887 ns |      59.9468 ns |  0.09 |    0.00 |        - |        - |        - |         - |          NA |
| When_Any_Result_Async                | 10000      |    734,624.979 ns |   1,718.5159 ns |   1,523.4196 ns |  1.98 |    0.01 |        - |        - |        - |         - |          NA |
| When_Any_Result_Sync                 | 10000      |     43,065.903 ns |     110.9265 ns |     103.7608 ns |  0.12 |    0.00 |        - |        - |        - |         - |          NA |
| Yield_Steady_State                   | 10000      |     81,246.173 ns |     151.3164 ns |     134.1381 ns |  0.22 |    0.00 |        - |        - |        - |         - |          NA |
| Switch_To_Steady_State               | 10000      |    424,123.568 ns |   3,883.3294 ns |   3,632.4687 ns |  1.14 |    0.01 |        - |        - |        - |         - |          NA |
| Switch_To_Context_Steady_State       | 10000      |    492,349.289 ns |   2,355.7210 ns |   2,088.2853 ns |  1.33 |    0.01 |        - |        - |        - |         - |          NA |
|                                      |            |                   |                 |                 |       |         |          |          |          |           |             |
| **Single_Async**                         | **100000**     |  **3,982,885.397 ns** |   **3,583.8487 ns** |   **2,992.6760 ns** |  **1.00** |    **0.00** |        **-** |        **-** |        **-** |         **-** |          **NA** |
| Single_Result_Async                  | 100000     |  4,314,389.286 ns |   5,201.2109 ns |   4,610.7380 ns |  1.08 |    0.00 |        - |        - |        - |         - |          NA |
| Single_Sync                          | 100000     |    170,142.274 ns |     453.4551 ns |     424.1622 ns |  0.04 |    0.00 |        - |        - |        - |         - |          NA |
| Single_Result_Sync                   | 100000     |    196,938.364 ns |     278.4667 ns |     260.4779 ns |  0.05 |    0.00 |        - |        - |        - |         - |          NA |
| Stack_Depth_5_Async                  | 100000     |  4,179,227.240 ns |   9,307.9746 ns |   8,706.6852 ns |  1.05 |    0.00 |        - |        - |        - |         - |          NA |
| Stack_Depth_5_Sync                   | 100000     |    305,761.263 ns |   1,172.7839 ns |   1,097.0228 ns |  0.08 |    0.00 |        - |        - |        - |         - |          NA |
| Stack_Depth_5_Result_Async           | 100000     |  4,661,674.790 ns |   5,327.9821 ns |   4,449.1064 ns |  1.17 |    0.00 |        - |        - |        - |         - |          NA |
| Stack_Depth_5_Result_Sync            | 100000     |    275,021.113 ns |     181.6134 ns |     151.6554 ns |  0.07 |    0.00 |        - |        - |        - |       6 B |          NA |
| Nested_Awaits_Async                  | 100000     | 46,395,137.500 ns | 320,077.3500 ns | 283,740.2338 ns | 11.65 |    0.07 |        - |        - |        - |         - |          NA |
| Nested_Awaits_Sync                   | 100000     |  1,676,962.812 ns |   4,154.5765 ns |   3,886.1934 ns |  0.42 |    0.00 |        - |        - |        - |         - |          NA |
| Nested_Awaits_Result_Async           | 100000     | 51,949,209.333 ns | 488,298.6522 ns | 456,754.8624 ns | 13.04 |    0.11 |        - |        - |        - |         - |          NA |
| Nested_Awaits_Result_Sync            | 100000     |  2,145,787.081 ns |  10,282.4592 ns |   9,115.1322 ns |  0.54 |    0.00 |        - |        - |        - |         - |          NA |
| When_All_Async                       | 100000     |  9,004,231.808 ns |  49,895.9942 ns |  44,231.4992 ns |  2.26 |    0.01 |        - |        - |        - |         - |          NA |
| When_All_Sync                        | 100000     |    352,393.535 ns |   1,220.8893 ns |   1,142.0206 ns |  0.09 |    0.00 |        - |        - |        - |         - |          NA |
| When_All_Result_Async                | 100000     |  8,564,897.500 ns |  48,549.1403 ns |  45,412.8960 ns |  2.15 |    0.01 |        - |        - |        - |  400024 B |          NA |
| When_All_Result_Sync                 | 100000     |    580,612.070 ns |   2,832.7182 ns |   2,649.7264 ns |  0.15 |    0.00 | 107.4219 | 107.4219 | 107.4219 |  400057 B |          NA |
| When_All_Result_Async_Non_Allocating | 100000     |  8,379,529.464 ns |  16,716.0640 ns |  14,818.3553 ns |  2.10 |    0.00 |        - |        - |        - |         - |          NA |
| When_All_Result_Sync_Non_Allocating  | 100000     |    477,534.553 ns |     526.5556 ns |     466.7778 ns |  0.12 |    0.00 |        - |        - |        - |         - |          NA |
| When_Any_Async                       | 100000     |  9,793,344.688 ns |  65,203.3103 ns |  60,991.2170 ns |  2.46 |    0.01 |        - |        - |        - |         - |          NA |
| When_Any_Sync                        | 100000     |    319,977.763 ns |     599.6927 ns |     468.2006 ns |  0.08 |    0.00 |        - |        - |        - |         - |          NA |
| When_Any_Result_Async                | 100000     |  8,948,331.920 ns |  19,820.9216 ns |  17,570.7307 ns |  2.25 |    0.00 |        - |        - |        - |         - |          NA |
| When_Any_Result_Sync                 | 100000     |    431,813.571 ns |     876.4202 ns |     819.8040 ns |  0.11 |    0.00 |        - |        - |        - |         - |          NA |
| Yield_Steady_State                   | 100000     |    957,795.905 ns |   1,939.8354 ns |   1,719.6136 ns |  0.24 |    0.00 |        - |        - |        - |         - |          NA |
| Switch_To_Steady_State               | 100000     |  4,364,417.812 ns |  17,168.2112 ns |  16,059.1555 ns |  1.10 |    0.00 |        - |        - |        - |         - |          NA |
| Switch_To_Context_Steady_State       | 100000     |  4,993,877.760 ns |  46,037.0799 ns |  43,063.1131 ns |  1.25 |    0.01 |        - |        - |        - |         - |          NA |
